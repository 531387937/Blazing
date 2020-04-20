using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Vector3 posOffset;
    public Vector3 rotOffset;
    public RagdollAnim idleAnim;
    private bool weaponed = false;
    private Rigidbody rig;
    private Collider col;
    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
       Quaternion q1 = Quaternion.Euler(new Vector3(30, 42, 22));
       Quaternion q2 = Quaternion.Euler(new Vector3(-27, 3, 90));
        Debug.Log((q1 * q2).eulerAngles);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if(!weaponed&&other.gameObject.tag=="Player")
        {
            var ragdoll = other.transform.root.GetComponent<APRController>();
            var rightHand = ragdoll.RightHand;
            var joint = gameObject.AddComponent<FixedJoint>();
            weaponed = true;
            col.isTrigger = false;
            rig.isKinematic = false;
            rig.useGravity = false;
            transform.SetParent(rightHand.transform);
            transform.localPosition = posOffset;
            transform.localEulerAngles = rotOffset;
            transform.SetParent(transform.root);
            joint.connectedBody = rightHand;
            
        }
    }
}
