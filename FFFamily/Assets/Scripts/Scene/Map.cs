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
    public void InitMap(GameObject wood,GameObject edge,GameObject ring)
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
                }
                else if (mapData[i * count + j] == 2)
                {
                    var grid = Instantiate(edge, map.transform);
                    grid.transform.localPosition = new Vector3(size * j, 0, size * i);
                }
                else if (mapData[i * count + j] == 3)
                {
                    var grid = Instantiate(ring, map.transform);
                    grid.transform.localPosition = new Vector3(size * j, 0, size * i);
                }
            }
        }
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
