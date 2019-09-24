using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlateControl : MonoBehaviour
{
    public float MaxAngle;
    public float test;

    void Awake() 
    {
        test = -10;
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        WeightCore();
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
            Wdirection += new Vector3(players[i].gameObject.transform.localPosition.x, players[i].transform.localPosition.z, 0);
            //Debug.Log(players[i].gameObject + "(" + players[i].gameObject.transform.position.x+"," +players[i].gameObject.transform.position.z+ ")");
        }
        angle = (float)System.Math.Sqrt(Wdirection.x * Wdirection.x + Wdirection.y * Wdirection.y);
        Vector3 from = Vector3.up;
        Vector3 to = new Vector3(-Wdirection.x, test, Wdirection.y);
        //transform.rotation = Quaternion.FromToRotation(from, to);
        Vector3 Newrotation = Quaternion.FromToRotation(from, to).eulerAngles;
        Newrotation += new Vector3(0, 0, 180);
        Newrotation.y = 0;
        transform.rotation = Quaternion.Euler(Newrotation);
    }
}
