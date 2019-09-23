using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateControl : MonoBehaviour
{
    public float MaxAngle;

    void Start()
    {
        WeightCore();
    }

    
    void Update()
    {
        
    }

    void WeightCore()
    {
        float angle;
        Vector3 Wdirection = new Vector3(0, 0, 1);
        List<Transform> players = new List<Transform>();
        GameObject p = gameObject.transform.Find("Players").gameObject;
        foreach(Transform child in p.transform)
        {
            players.Add(child);
            //Debug.Log(child.gameObject.name);
        }

        for (int i = 0; i < players.Count; i++)
        {
            Wdirection += new Vector3(players[i].gameObject.transform.position.x, players[i].transform.position.z, 0);
            //Debug.Log(players[i].gameObject + "(" + players[i].gameObject.transform.position.x+"," +players[i].gameObject.transform.position.z+ ")");
        }
        angle = (float)System.Math.Sqrt(Wdirection.x * Wdirection.x + Wdirection.y * Wdirection.y);
    }
}
