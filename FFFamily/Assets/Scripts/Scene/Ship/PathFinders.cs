using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinders : MonoBehaviour
{
    public List<Transform> pathFinders = new List<Transform>();
    // Start is called before the first frame update
    private int index;
    Vector3 nextRoute;
    void Awake()
    {
        int count = transform.childCount;
        for(int i = 0;i<count;i++)
        {
            pathFinders.Add(transform.GetChild(i));
        }
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.B))
        {
            FindNextRoute();
        }
    }
    public Vector3 FindNextRoute()
    {
        index = (index + 1) % transform.childCount;
        Transform tr = pathFinders[index];


        nextRoute = tr.position;
        return nextRoute;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for(int i = 0;i<transform.childCount;i++)
        {
            Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.5f);
        }
    }
}
