using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCtr : MonoBehaviour
{
    bool operated = false;
    List<APRController> failer;
    public Transform operatePos;
    private Rigidbody rig;
    // Start is called before the first frame update
    void Start()
    {
        failer = new List<APRController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if(rig!=null)
        {
            if(Vector3.Distance(operatePos.position,rig.transform.position)>=1f)
            {
                rig.velocity = (operatePos.position - rig.transform.position)*3;
                print("吸！！！！");
            }
            else
            {
                rig = null;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        var ragdoll = other.transform.root.GetComponent<APRController>();
        if(!operated&&ragdoll!=null)
        {
            operated = true;
            ragdoll.GetBoat(this.gameObject);
            GetComponent<Collider>().isTrigger = false;
            rig = ragdoll.Root.GetComponent<Rigidbody>();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        var ragdoll = collision.transform.root.GetComponent<APRController>();
        if(ragdoll!=null&&!failer.Contains(ragdoll))
        {
            var rig = ragdoll.Root.GetComponent<Rigidbody>();
            rig.velocity = new Vector3(-rig.velocity.x, 0, -rig.velocity.z).normalized * 100 + new Vector3(0, 40, 0);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(operatePos.position, 0.5f);
    }
}
