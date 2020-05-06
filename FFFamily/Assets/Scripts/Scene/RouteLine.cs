using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteLine : MonoBehaviour
{
    [SerializeField] private Transform points;                          //控制点父对象
    private List<Vector3> point_tranList = new List<Vector3>(3);     //控制点列表
    [SerializeField] private int pointCount = 100;                      //曲线点的个数
    private List<Vector3> line_pointList;                               //曲线点列表

    [SerializeField] private GameObject ball;                           //运动物体
    [SerializeField] private float time0 = 0;                           //曲线点之间移动时间间隔
    private float timer = 0;                    //计时器
    private int item = 1;                       //曲线点的索引
    private bool isTrue = false;
    public PathFinders finder;
    //使小球沿曲线运动
    //这里不能直接在for里以Point使用差值运算，看不到小球运算效果
    //定义一个计时器，在相隔时间内进行一次差值运算。
    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            point_tranList.Add(new Vector3(0, 0, 0));
        }
        ball.transform.position = finder.FindNextRoute();
        point_tranList[0] = ball.transform.position;
        point_tranList[2] = finder.FindNextRoute();
        Init();
    }
    void Init()
    {
        line_pointList = new List<Vector3>();
        Vector3 offset = Vector3.Cross((point_tranList[2] - point_tranList[0]).normalized, Vector3.up) * 3;
        point_tranList[1] = (point_tranList[0] + point_tranList[2]) / 2;
        
        for (int i = 0; i < 30; i++)
        {
            Ray ray = new Ray(point_tranList[0], Quaternion.AngleAxis(15 - i, Vector3.up) * (point_tranList[2] - point_tranList[0]));
            RaycastHit hit;
            if ((Physics.Raycast(ray, out hit, Vector3.Distance(point_tranList[0], point_tranList[2]))))
            {
                if (hit.transform.tag == "Ground")
                {
                        point_tranList[1] -= offset;
                        break;
                }
            }
        }
        for (int i = 0; point_tranList.Count != 0 && i < pointCount; i++)
        {
            //一
            Vector3 pos1 = Vector3.Lerp(point_tranList[0], point_tranList[1], i / (float)pointCount);
            Vector3 pos2 = Vector3.Lerp(point_tranList[1], point_tranList[2], i / (float)pointCount);
            Vector3 find = Vector3.Lerp(pos1, pos2, i / (float)pointCount);

            line_pointList.Add(find);
        }
        if (line_pointList.Count == pointCount)
            isTrue = true;
    }

    void Update()
    {
        if (!isTrue)
            return;
        timer += Time.deltaTime;
        if (timer > time0)
        {
            timer = 0;
            if (item >= line_pointList.Count - 1)
            {
                ball.transform.LookAt(line_pointList[item]);
            }
            else
            {
                    ball.transform.LookAt(line_pointList[item]);
            }
            if (item == 0)
            {
                ball.transform.localPosition = Vector3.Lerp(line_pointList[line_pointList.Count - 1], line_pointList[item], 1f);
            }
            else
            {
                ball.transform.localPosition = Vector3.Lerp(line_pointList[item - 1], line_pointList[item], 1f);
            }
            item++;
            if (item >= line_pointList.Count)
            {
                item = 0;
                while (true)
                {
                    point_tranList[0] = ball.transform.position;
                    point_tranList[2] = finder.FindNextRoute();
                    if(Vector3.Distance(point_tranList[0],point_tranList[2])>3)
                    {
                        break;
                    }
                }
                Init();
            }
        }
    }

    //------------------------------------------------------------------------------
    //在scene视图显示
    void OnDrawGizmos()//画线
    {
        //Init();
        Gizmos.color = Color.yellow;
        for (int i = 0; i < line_pointList.Count; i++)
        {
            Gizmos.DrawLine(line_pointList[i], line_pointList[i + 1]);
        }
    }

}