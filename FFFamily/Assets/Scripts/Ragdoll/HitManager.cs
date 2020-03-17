﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum hitMode
{
    hand = 0,
    foot = 1,
}
public class HitManager : MonoBehaviour
{
    public List<GameObject> hitBody;
    public float[] damages;

    public List<HitSensor> hand;
    public List<HitSensor> foot;
    //public List<HitSensor> special;

    private Dictionary<GameObject, float> dic = new Dictionary<GameObject, float>();
    public RagdollController ragCtr;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < hitBody.Count; i++)
        {
            dic.Add(hitBody[i], damages[i]);
        }
    }

    public void GetHurt(GameObject go, Vector3 point, Vector3 impulse)
    {
        float dam;
        if (dic.TryGetValue(go, out dam))
        {
            ragCtr.Stun += dam;
            Rigidbody boneRb = go.GetComponent<Rigidbody>();
            boneRb.AddForceAtPosition(impulse.normalized * 400, point, ForceMode.Impulse);
            Vector3 dir = new Vector3(impulse.x, 0, impulse.z);
            //rb.AddForce(dir.normalized * 400, ForceMode.Impulse);
            //Instantiate(hitParticle, point, Quaternion.identity);
        }
    }

    List<HitSensor> temp;
    public void HitMode(hitMode mode)
    {
        switch (mode)
        {
            case hitMode.hand:
                temp = hand;
                break;
            case hitMode.foot:
                temp = foot;
                break;
        }
        foreach (HitSensor sensor in temp)
        {
            sensor.canHit = true;
        }
    }
    public void StopHit()
    {
        foreach (HitSensor sensor in temp)
        {
            sensor.canHit = false;
        }
    }
}
