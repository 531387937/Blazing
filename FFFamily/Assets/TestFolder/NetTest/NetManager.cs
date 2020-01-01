using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using UnityEngine.UI;
using System;
using ProtoBuf;
public static class NetManager
{
    //定义套接字
    static Socket socket;

    //接受缓冲区
    static ByteArray readBuff;

    //写入队列
    static Queue<ByteArray> writeQueue;
    //事件
    public enum NetEvent
    {
        ConnectSucc = 1,
        ConnectFail = 2,
        Close = 3,
    }
    //事件委托类型
    public delegate void EventListener(string err);
    public delegate void MsgListener(IExtensible msg);
    static List<IExtensible> msgList = new List<IExtensible>();
    static int msgCount = 0;
    readonly static int MAX_MESSAGE_FIRE = 10;
    //事件监听列表
    private static Dictionary<NetEvent, EventListener> eventListeners = new Dictionary<NetEvent, EventListener>();
    private static Dictionary<string, MsgListener> msgListeners = new Dictionary<string, MsgListener>();
    static bool isConnecting = false;
    static bool isClosing = false;
    #region public方法
    /// <summary>
    /// 添加事件监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void AddEventListener(NetEvent netEvent, EventListener listener)
    {
        //添加事件
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] += listener;
        }
        else
        {
            eventListeners[netEvent] = listener;
        }
    }
    public static void AddMsgListner(string msgName,MsgListener listener)
    {
        if(msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] += listener;
        }
        else
        {
            msgListeners[msgName] = listener;
        }
    }
    /// <summary>
    /// 删除事件监听
    /// </summary>
    /// <param name="netEvent"></param>
    /// <param name="listener"></param>
    public static void RemoveEventListener(NetEvent netEvent, EventListener listener)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent] -= listener;
            if (eventListeners[netEvent] == null)
            {
                eventListeners.Remove(netEvent);
            }
        }
    }
    public static void RemoveMsgListener(string msgName,MsgListener listener)
    {
        if(msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName] -= listener;
            if(msgListeners[msgName]==null)
            {
                msgListeners.Remove(msgName);
            }
        }
    }
    /// <summary>
    /// 连接
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    public static void Connect(string ip, int port)
    {
        //状态判断
        if (socket != null && socket.Connected)
        {
            Debug.Log("Connect faul,already connected!");
            return;
        }
        if (isConnecting)
        {
            Debug.Log("Connect faul,already isConnecting!");
            return;
        }
        //初始化成员
        InitState();
        socket.NoDelay = true;
        isConnecting = true;
        socket.BeginConnect(ip, port, ConnectCallback, socket);
    }
    public static void Close()
    {
        //状态判断
        if(socket == null||!socket.Connected)
        {
            return;
        }
        if(isConnecting)
        {
            return;
        }
        if(writeQueue.Count>0)
        {
            isConnecting = true;
        }
        else
        {
            socket.Close();
            FireEvent(NetEvent.Close, "");
        }
    }
    public static void Send(IExtensible msg)
    {
        if(socket == null || !socket.Connected)
        {
            return;
        }
        if(isConnecting)
        {
            return;
        }
        if(isClosing)
        {
            return;
        }
        byte[] nameBytes = MsgUtils.EncodeName(msg);
        byte[] bodyBytes = MsgUtils.Encode(msg);
        int len = nameBytes.Length + bodyBytes.Length;
        byte[] sendBytes = new byte[2 + len];
        sendBytes[0] = (byte)(len % 256);
        sendBytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, sendBytes, 2, nameBytes.Length);
        Array.Copy(bodyBytes, 0, sendBytes, 2 + nameBytes.Length, bodyBytes.Length);
        ByteArray ba = new ByteArray(sendBytes);
        int count = 0;
        lock(writeQueue)
        {
            writeQueue.Enqueue(ba);
            count = writeQueue.Count;
        }
        if(count==1)
        {
            socket.BeginSend(sendBytes, 0, sendBytes.Length, 0, SendCallback, socket);
        }
    }
    public static void SendCallback(IAsyncResult ar)
    {
        Socket socket = (Socket)ar.AsyncState;
        if(socket == null||!socket.Connected)
        {
            return;
        }
        int count = socket.EndSend(ar);
        ByteArray ba;
        lock(writeQueue)
        {
            ba = writeQueue.Peek();
        }
        ba.readIdx += count;
        if(ba.length==0)
        {
            lock(writeQueue)
            {
                writeQueue.Dequeue();
                ba = writeQueue.Peek();
            }
        }
        if(ba!=null)
        {
            socket.BeginSend(ba.bytes, ba.readIdx, ba.length, 0, SendCallback, socket);
        }
        else if(isClosing)
        {
            socket.Close();
        }
    }
    public static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            //获取接收数据长度
            int count = socket.EndReceive(ar);
            if(count == 0)
            {
                Close();
                return;
            }
            readBuff.writeIdx += count;
            OnReceiveData();
            if(readBuff.remain<8)
            {
                readBuff.MoveBytes();
                readBuff.ReSize(readBuff.length * 2);
            }
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Receive fail" + ex.ToString());
        }
    }
    public static void OnReceiveData()
    {
        if(readBuff.length<=2)
        {
            return;
        }
        //获取消息体长度
        int readidx = readBuff.readIdx;
        byte[] bytes = readBuff.bytes;
        Int16 bodyLength = (Int16)((bytes[readidx + 1] << 8) | bytes[readidx]);
        if(readBuff.length<bodyLength+2)
        {
            return;
        }
        readBuff.readIdx += 2;
        //解析协议名
        int nameCount = 0;
        string protoName = MsgUtils.DecodeName(readBuff.bytes, readBuff.readIdx, out nameCount);
        if(protoName == "")
        {
            Debug.Log("OnReceiveData MsgUtils.DecodeName fail");
            return;
        }
        readBuff.readIdx += nameCount;
        //解析协议体
        int bodyCount = bodyLength - nameCount;
        IExtensible msg = MsgUtils.Decode(protoName, readBuff.bytes, readBuff.readIdx, bodyCount);
        readBuff.readIdx += bodyCount;
        readBuff.CheckAndMoveBytes();
        lock(msgList)
        {
            msgList.Add(msg);
        }
        msgCount++;
        if(readBuff.length>2)
        {
            OnReceiveData();
        }
    }
    /// <summary>
    /// NetWork的Update
    /// </summary>
    public static void Update()
    {
        MsgUpdate();
    }
    public static void MsgUpdate()
    {
        if(msgCount == 0)
        {
            return;
        }
        for(int i =0;i<MAX_MESSAGE_FIRE;i++)
        {
            //获得第一条信息
            IExtensible msg = null;
            lock(msgList)
            {
                if(msgList.Count>0)
                {
                    msg = msgList[0];
                    msgList.RemoveAt(0);
                    msgCount--;
                }
            }
            //分发信息
            if(msg!=null)
            {
                FireMsg(msg.ToString(), msg);
            }
            else
            {
                break;
            }
        }
    }
    #endregion
# region private方法
    //分发事件
    private static void FireEvent(NetEvent netEvent, String err)
    {
        if (eventListeners.ContainsKey(netEvent))
        {
            eventListeners[netEvent](err);
        }
    }
    private static void FireMsg(string msgName,IExtensible msg)
    {
        if(msgListeners.ContainsKey(msgName))
        {
            msgListeners[msgName](msg);
        }
    }
    //初始化状态
    private static void InitState()
    {
        msgList.Clear();
        msgCount = 0;
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        readBuff = new ByteArray();
        writeQueue = new Queue<ByteArray>();
        isConnecting = false;
        isClosing = false;
    }
    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            Debug.Log("Socket Connect Succ ");
            FireEvent(NetEvent.ConnectSucc, "");
            isConnecting = false;
            //开始接收
            socket.BeginReceive(readBuff.bytes, readBuff.writeIdx, readBuff.remain, 0, ReceiveCallback, socket);
        }
        catch(SocketException ex)
        {
            Debug.Log("Socket Connect fail " + ex.ToString());
            FireEvent(NetEvent.ConnectFail, ex.ToString());
            isConnecting = false;
        }
    }
    #endregion
}
