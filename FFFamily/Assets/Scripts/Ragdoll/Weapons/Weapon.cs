using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Vector3 posOffset;
    public Vector3 rotOffset;
    public string idleAnim;
    public string attackAnim;
    private bool weaponed = false;
    private Rigidbody rig;
    private bool _throw;
    private RagdollAnim attack;
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_throw)
        {
            timer += Time.deltaTime;
            if(rig.velocity.magnitude<=0.2f&&timer>1)
            {
                _throw = false;
                weaponed = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {

    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!weaponed && collision.gameObject.tag == "Player")
        {
             var ragdoll = collision.transform.root.GetComponent<APRController>();
            if (ragdoll.OnGetWeapon(this))
            {
                var rightHand = ragdoll.RightHand;
                rightHand.GetComponent<Collider>().isTrigger = true;
                var joint = gameObject.AddComponent<FixedJoint>();
                weaponed = true;
                transform.SetParent(rightHand.transform);
                transform.localPosition = posOffset;
                transform.localEulerAngles = rotOffset;
                transform.SetParent(transform.root);
                joint.connectedBody = rightHand;
            }
        }
    }
    public void OnThrow()
    {
        _throw = true;
    }
}
