using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MapEditor : EditorWindow
{
    private Map map;
    private GameObject wood;
    private GameObject edge;
    private GameObject ring;
    private string path = "Assets/Resources/"+"map.asset";
    Rect[] rects = null;
    //利用构造函数来设置窗口名称
    MapEditor()
    {
        this.titleContent = new GUIContent("Map Editor");
    }

    //添加菜单栏用于打开窗口
    [MenuItem("Tools/MapEditor")]
    static void showWindow()
    {
        EditorWindow.GetWindow(typeof(MapEditor));
    }

    private void OnGUI()
    {
        map = (Map)EditorGUILayout.ObjectField("地图文件", map, typeof(Map), true);
        wood = (GameObject)EditorGUILayout.ObjectField("木块", wood, typeof(GameObject), true);
        edge = (GameObject)EditorGUILayout.ObjectField("边界", edge, typeof(GameObject), true);
        ring = (GameObject)EditorGUILayout.ObjectField("擂台柱子", ring, typeof(GameObject), true);
        if (map == null)
        {
            if (GUILayout.Button("创建新的地图文件"))
            {
                map = new Map();
            }
        }
        else
        {
            if (rects == null)
            {
                rects = new Rect[map.mapData.Length];
            }
            for (int i = 0;i<Mathf.Sqrt(map.mapData.Length);i++)
            {
                GUILayout.BeginHorizontal();
                for(int j = 0;j< Mathf.Sqrt(map.mapData.Length); j++)
                {
                    if(map.mapData[(int)(i*Mathf.Sqrt(map.mapData.Length)+j)]==1)
                    {
                        GUI.backgroundColor = Color.green;
                    }
                    else if (map.mapData[(int)(i * Mathf.Sqrt(map.mapData.Length) + j)] == 2)
                    {
                        GUI.backgroundColor = Color.gray;
                    }
                    else if (map.mapData[(int)(i * Mathf.Sqrt(map.mapData.Length) + j)] == 3)
                    {
                        GUI.backgroundColor = Color.red;
                    }
                    rects[(int)(i * Mathf.Sqrt(map.mapData.Length) + j)] = GUILayoutUtility.GetRect(new GUIContent(map.mapData[(int)(i * Mathf.Sqrt(map.mapData.Length) + j)].ToString()), "HelpBox");
                    GUI.Box(rects[(int)(i * Mathf.Sqrt(map.mapData.Length) + j)], map.mapData[(int)(i * Mathf.Sqrt(map.mapData.Length) + j)].ToString(), "HelpBox");
                    GUI.backgroundColor = Color.white;
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            if (GUILayout.Button("根据边界填充地图(测试)"))
            {
                if (map.FillMap())
                {
                    Repaint();
                    return;
                }
            }
            if (GUILayout.Button("生成地图格"))
            {
                map.InitMap(wood, edge, ring);
            }
        }
        if (ClickMap())
        {
            Repaint();
            return;
        }
    }
    private bool ClickMap()
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].Contains(Event.current.mousePosition))
                {
                    EditorUtility.SetDirty(map);
                    map.mapData[i]++;
                    map.mapData[i] %= System.Enum.GetNames(typeof(GridType)).Length;
                    return true;
                }
            }
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
        {
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i].Contains(Event.current.mousePosition))
                {
                    EditorUtility.SetDirty(map);
                    map.mapData[i] = 0;
                    return true;
                }
            }
        }
        return false;
    }
}
