using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteLine : MonoBehaviour
{
    private List<Vector3> point_tranList = new List<Vector3>(3);     //控制点列表
    [SerializeField] private int pointCount = 100;                      //曲线点的个数
    private List<Vector3> line_pointList;                               //曲线点列表

    [SerializeField] private GameObject boat;                           //运动物体
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
        boat.transform.position = finder.FindNextRoute();
        line_pointList = new List<Vector3>();
        point_tranList[0] = boat.transform.position;
        point_tranList[2] = finder.FindNextRoute();
        Init();
        for (int i = 0; i < finder.pathFinders.Count - 1; i++)
        {
            point_tranList[0] = point_tranList[2];
            point_tranList[2] = finder.FindNextRoute();
            Init();
        }
    }
    void Init()
    {

        Vector3 offset = Vector3.Cross((point_tranList[2] - point_tranList[0]).normalized, Vector3.up) * 6;
        point_tranList[1] = (point_tranList[0] + point_tranList[2]) / 2;
        float a = pointCount;
        if (Mathf.Abs(point_tranList[2].x - point_tranList[0].x) * Mathf.Abs(point_tranList[2].z - point_tranList[0].z) > 5f)
        {
            point_tranList[1] -= offset;
            a *= 1.5f;
        }
        for (int i = 0; point_tranList.Count != 0 && i < a; i++)
        {
            //一
            Vector3 pos1 = Vector3.Lerp(point_tranList[0], point_tranList[1], i / (float)a);
            Vector3 pos2 = Vector3.Lerp(point_tranList[1], point_tranList[2], i / (float)a);
            Vector3 find = Vector3.Lerp(pos1, pos2, i / (float)a);

            line_pointList.Add(find);
        }
        if (line_pointList.Count == a)
            isTrue = true;
    }

    void Update()
    {
        if (!isTrue||!boat.GetComponent<ShipCtr>().drifting)
            return;
        timer += Time.deltaTime;
        if (timer > time0)
        {
            timer = 0;

            boat.transform.LookAt(line_pointList[item]);
            boat.transform.localPosition = Vector3.Lerp(line_pointList[item - 1], line_pointList[item], 1f);
            item++;
            if (item >= line_pointList.Count)
            {
                item = 1;
            }
        }
    }

    //------------------------------------------------------------------------------
    //在scene视图显示
    void OnDrawGizmos()//画线
    {
        Gizmos.color = Color.yellow;
        for (int i = 0; i < line_pointList.Count - 1; i++)
        {
            Gizmos.DrawLine(line_pointList[i], line_pointList[i + 1]);
        }
    }

}