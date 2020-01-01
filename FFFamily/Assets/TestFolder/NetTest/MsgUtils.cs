using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MsgUtils
{
    public static byte[] Encode(IExtensible msg)
    {
        using (var memory = new System.IO.MemoryStream())
        {
            Serializer.Serialize(memory, msg);
            return memory.ToArray();
        }
    }
    public static IExtensible Decode(string protoName, byte[] bytes, int offset, int count)
    {
        using (var memory = new System.IO.MemoryStream(bytes, offset, count))
        {
            System.Type t = System.Type.GetType(protoName);
            return (IExtensible)Serializer.NonGeneric.Deserialize(t, memory);
        }
    }
    public static byte[] EncodeName(IExtensible msg)
    {
        byte[] nameBytes = System.Text.Encoding.UTF8.GetBytes(msg.ToString());
        Int16 len = (Int16)nameBytes.Length;
        byte[] bytes = new byte[2 + len];
        bytes[0] = (byte)(len & 256);
        bytes[1] = (byte)(len / 256);
        Array.Copy(nameBytes, 0, bytes, 2, len);
        return bytes;
    }
    public static string DecodeName(byte[] bytes,int offset,out int count)
    {
        count = 0;
        //必须大于2字节
        if(offset+2>bytes.Length)
        {
            return "";
        }
        //读取长度
        Int16 len = (Int16)((bytes[offset + 1] << 8) | bytes[offset]);
        if(len<=0)
        {
            return "";
        }
        if(offset +2+len>bytes.Length)
        {
            return "";
        }
        //解析
        count = 2 + len;
        string name = System.Text.Encoding.UTF8.GetString(bytes, offset + 2, len);
        return name;
    }
}
