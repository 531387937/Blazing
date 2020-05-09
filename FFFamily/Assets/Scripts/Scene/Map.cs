using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum GridType
{
    None = 0,
    Wood = 1,
    Edge = 2,
    Ring = 3,
}
public class Map : ScriptableObject
{
    public int[] mapData = new int[625];

    public void InitMap(GameObject wood,GameObject edge,GameObject ring,GameObject rope)
    {
        GameObject map = new GameObject("map");
        int count =(int)Mathf.Sqrt(mapData.Length);
        float size = wood.transform.lossyScale.x;
        for(int i = 0;i<count;i++)
        {
            for(int j = 0;j<count;j++)
            {
                if(mapData[i*count+j]==1)
                {
                    var grid = Instantiate(wood, map.transform);
                    grid.transform.localPosition = new Vector3(size * j, 0, size * i);
                    grid.GetComponent<Grid>().index = i * count + j;
                }
                else if (mapData[i * count + j] == 2)
                {
                    var grid = Instantiate(edge, map.transform);
                    grid.transform.localPosition = new Vector3(size * j, 0, size * i);
                    grid.GetComponent<Grid>().index = i * count + j;
                }
                else if (mapData[i * count + j] == 3)
                {
                    var grid = Instantiate(ring, map.transform);
                    grid.transform.localPosition = new Vector3(size * j, 0, size * i);
                    GameObject preRope = null;
                    int tempI = i;
                    int tempJ = j;
                    //向右造绳子
                    while (mapData[i * count + tempJ + 1] == 2)
                    {
                        var ropes = Instantiate(rope, map.transform);
                        ropes.transform.localPosition = new Vector3(size * (tempJ + 1), 1.7f, size * i);
                        ropes.transform.rotation = rope.transform.rotation;
                        ropes.name = "Hrope" + (tempJ - j);
                        ropes.transform.SetParent(grid.transform);
                        if (preRope != null)
                        {
                            ropes.GetComponent<ConfigurableJoint>().connectedBody = preRope.GetComponent<Rigidbody>();
                        }
                        preRope = ropes;
                        tempJ++;
                    }
                    preRope = null;
                    //向下造绳子
                    while (mapData[(tempI + 1) * count + j] == 2)
                    {
                        var ropes = Instantiate(rope, map.transform);
                        ropes.transform.localPosition = new Vector3(size * j, 1.7f, size * (tempI + 1));
                        ropes.transform.rotation = Quaternion.Euler(0, 90, 90);
                        ropes.name = "Vrope"+(tempI - i);
                        ropes.transform.SetParent(grid.transform);
                        if (preRope != null)
                        {
                            ropes.GetComponent<ConfigurableJoint>().connectedBody = preRope.GetComponent<Rigidbody>();
                        }
                        else
                        {
                            ropes.GetComponent<ConfigurableJoint>().connectedBody = grid.transform.GetChild(0).GetComponent<Rigidbody>();
                        }
                        preRope = ropes;
                        tempI++;
                    }
                }
            }
        }
        //GameObject preRope = null;
        //for(int i = 0;i<count;i++)
        //{
        //    for(int j = 0;j<count;j++)
        //    {
        //        if (mapData[i * count + j] == 3)
        //        {
        //            int tempI = i;
        //            int tempJ = j;
        //            //向右造绳子
        //            while(mapData[i*count+tempJ+1]==2)
        //            {
        //                var ropes = Instantiate(rope, map.transform);
        //                ropes.transform.localPosition = new Vector3(size * (tempJ+1), 1.7f, size * i);
        //                ropes.transform.rotation = rope.transform.rotation;
        //                ropes.name = i + (",") + j;
        //                if(preRope!=null)
        //                {
        //                    ropes.GetComponent<ConfigurableJoint>().connectedBody = preRope.GetComponent<Rigidbody>();
        //                }
        //                preRope = ropes;
        //                tempJ++;
        //            }
        //            preRope = null;
        //            //向下造绳子
        //            while (mapData[(tempI + 1) * count + j] == 2)
        //            {
        //                var ropes = Instantiate(rope, map.transform);
        //                ropes.transform.localPosition = new Vector3(size * j, 1.7f, size * (tempI + 1));
        //                ropes.transform.rotation = Quaternion.Euler(0, 90, 90);
        //                ropes.name = i + (",") + j;
        //                if (preRope != null)
        //                {
        //                    ropes.GetComponent<ConfigurableJoint>().connectedBody = preRope.GetComponent<Rigidbody>();
        //                }
        //                preRope = ropes;
        //                tempI++;
        //            }
        //        }
        //    }
        //}
    }

    public bool FillMap()
    {
        int count = (int)Mathf.Sqrt(mapData.Length);
        for(int i = 0;i<count;i++)
        {
            bool filling = false;
            for(int j = 0;j<count;j++)
            {
                if(mapData[i*count+j]==2&&!filling)
                {
                    filling = true;
                    continue;
                }
                else if(mapData[i * count + j] == 2 && filling)
                {
                    filling = false;
                }
                if(filling)
                {
                    mapData[i * count + j] = 1;
                }
            }
            if(filling)
            {
                Debug.LogError("地图没有封闭");
                return false;
            }
        }
        return true;
    }
}
[System.Serializable]
public class MapManager
{
    public Grid[] mapGrid;
    public MapManager(GameObject map)
    {
        mapGrid = new Grid[625]; 
        Grid[] children = map.GetComponentsInChildren<Grid>();
        foreach(var child in children)
        {
            int index = child.index;
            mapGrid[index] = child;
        }
    }

    public bool DestoryGrid(int index,float time)
    {
        if(index<0||index>625)
        {
            return false;
        }
        if (mapGrid[index]!=null&&mapGrid[index].type == GridType.Wood)
        {
            mapGrid[index].DestroyGrid(time);
            mapGrid[index] = null;
        }
        return true;
    }
}
