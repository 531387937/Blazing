using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitSensor : MonoBehaviour
{
    public bool canHit = false;

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

    private void OnCollisionEnter(Collision collision)
    {
        if (canHit && collision.gameObject.layer == 11)
        {
            canHit = false;
            collision.gameObject.transform.root.GetComponent<HitManager>().GetHurt(collision.gameObject, collision.contacts[0].point, velocity);
        }
    }

    void FixedUpdate()
    {
        velocity = (transform.position - oldPos) / Time.fixedDeltaTime;
        oldPos = transform.position;
    }
}
