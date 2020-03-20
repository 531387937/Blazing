using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSensor : MonoBehaviour
{
    public bool canHit = false;
    public bool specialGrab = false;
    public Transform specialTr;

    private Vector3 velocity
    {
        set
        {
            for (int i = 0; i < velocities.Length - 1; i++)
            {
                velocities[i + 1] = velocities[i];
            }
            velocities[0] = value;
        }
        get
        {
            Vector3 avg = Vector3.zero;
            foreach (Vector3 vel in velocities)
            {
                avg += vel;
            }
            avg /= velocities.Length;
            return avg;
        }
    }
    private Vector3[] velocities = new Vector3[2];
    private Vector3 oldPos;
    private Rigidbody grabRig;
    private Rigidbody r;

    private void OnCollisionEnter(Collision collision)
    {
        if (canHit && collision.gameObject.layer == 11)
        {
            canHit = false;
            collision.gameObject.transform.root.GetComponent<HitManager>().GetHurt(collision.gameObject, collision.contacts[0].point, velocity);
        }
        if(specialGrab&& collision.gameObject.layer == 11&&collision.gameObject.transform.root!=transform.root)
        {
            specialGrab = false;
            collision.gameObject.transform.root.GetComponent<HitManager>().BeGrabed();
            grabRig = collision.gameObject.transform.root.GetComponent<HitManager>().ragCtr.GetComponent<RagdollMecanimMixer.RamecanMixer>().RootBoneRb;
            r = collision.gameObject.transform.root.GetChild(1).GetComponent<Rigidbody>();

            r.transform.root.SetParent(specialTr);
        }
    }

    void FixedUpdate()
    {
        velocity = (transform.position - oldPos) / Time.fixedDeltaTime;
        oldPos = transform.position;
        if(grabRig)
        {
            //grabRig.velocity = velocity*2.3f;
            grabRig.AddForce(velocity.normalized*70,ForceMode.Acceleration);
            //r.velocity = grabRig.velocity;
        }
    }
    public void StopGrabing()
    {
        if (grabRig)
        {
            //grabRig.velocity = velocity*20;
            r.transform.parent.SetParent(null);
            r.transform.localRotation = r.transform.parent.rotation;
            Transform tr = r.transform.parent;
            r.transform.SetParent(null,true);
            grabRig.transform.parent.SetParent(null,true);
            tr.rotation = new Quaternion();
            grabRig.transform.parent.SetParent(tr, true);
            r.transform.SetParent(tr, true);           
            grabRig = null;
        }
    }
}
