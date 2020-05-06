using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinders : MonoBehaviour
{
    [SerializeField]
    private List<Transform> pathFinders = new List<Transform>();
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
        float shortest = 10000;
        RaycastHit shortestHit = new RaycastHit();
        for(int i = 0;i<30;i++)
        {
            Ray ray = new Ray(tr.position, Quaternion.AngleAxis(15 - i, tr.up) * tr.right);
            RaycastHit hit;
            if ((Physics.Raycast(ray, out hit, 100)))
            {
                if (hit.transform.tag == "Ground")
                {
                    if (hit.distance < shortest)
                    {
                        shortest = hit.distance;
                        shortestHit = hit;
                    }
                }
            }
        }
        nextRoute = shortestHit.point + (tr.position - shortestHit.point).normalized * 5;
        return nextRoute;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        for(int i = 0;i<transform.childCount;i++)
        {
            Gizmos.DrawWireSphere(transform.GetChild(i).position, 0.5f);
        }
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(nextRoute,0.5f);
    }
}
