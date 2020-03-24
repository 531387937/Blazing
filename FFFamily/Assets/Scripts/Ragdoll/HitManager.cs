using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum hitMode
{
    hand = 0,
    foot = 1,
    special = 2,
}
public class HitManager : MonoBehaviour
{
    public List<GameObject> hitBody;
    public float[] damages;
    public bool fighting = false;
    public List<HitSensor> hand;
    public List<HitSensor> foot;
    public HitSensor special;

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
            fighting = true;
            ragCtr.Stun += dam;
            Rigidbody boneRb = go.GetComponent<Rigidbody>();
            boneRb.AddForceAtPosition(impulse.normalized * 100, point, ForceMode.Impulse);
            Vector3 dir = new Vector3(impulse.x, 0, impulse.z);
            GameManager.Instance.audioManager.PlayHit();
            //rb.AddForce(dir.normalized * 400, ForceMode.Impulse);
            //Instantiate(hitParticle, point, Quaternion.identity);
        }
    }

    public void BeGrabed()
    {
        ragCtr.Ragdoll2Die();
    }

    List<HitSensor> temp;
    public void BeginHit(hitMode mode)
    {
        switch (mode)
        {
            case hitMode.hand:
                temp = hand;
                break;
            case hitMode.foot:
                temp = foot;
                break;
            case hitMode.special:
                special.specialGrab = true;
                break;
        }
        if (temp!=null)
        {
            foreach (HitSensor sensor in temp)
            {
                sensor.canHit = true;
            }
        }
    }
    public void StopHit()
    {
        if (temp != null)
        {
            foreach (HitSensor sensor in temp)
            {
                sensor.canHit = false;
            }
        }
        special.specialGrab = false;
        special.StopGrabing();
    }
    private void LateUpdate()
    {
        fighting = false;
    }
}
