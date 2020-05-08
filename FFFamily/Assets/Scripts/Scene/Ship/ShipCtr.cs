using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipCtr : MonoBehaviour
{
    bool operated = false;
    [HideInInspector]
    public Transform operatePos;
    [HideInInspector]
    public bool drifting;

    public GameObject cannon;
    public GameObject target;
    public Transform cannonPos;
    public float targetSpeed;
    private Rigidbody rig;
    private APRController rag;
    private float timer = 0;

    public int cannonNum;
    [Header("驾驶时间")]
    public float time;
    private int lastCannon;
    private LayerMask mask = 1 << 13;
    float zMax = 0; float zMin = 0; float xMax = 0; float xMin = 0;
    private bool sink = false;
    FloatingObject floatObj;
    // Start is called before the first frame update
    void Start()
    {
        lastCannon = cannonNum;
        floatObj = GetComponentInChildren<FloatingObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!drifting&&operated&&rig!=null)
        {
            timer += Time.deltaTime;
            if(timer>1)
            {
                StartDrift();
                timer = 0;
            }
        }
        if(drifting&&operated)
        {
            timer += Time.deltaTime;
            if(timer>time||lastCannon<=0)
            {
                Sink();
            }
        }
        if(sink)
        {
            floatObj.MaterialDensity += 100 * Time.deltaTime;
        }
    }
    private void FixedUpdate()
    {
        if(rig!=null)
        {
            if(Vector3.Distance(operatePos.position,rig.transform.position)>=2f)
            {
                rig.velocity = (operatePos.position - rig.transform.position)*3;
            }
        }
    }
    public void Fire()
    {
        lastCannon--;
        var c = GameObject.Instantiate(cannon,cannonPos.position,Quaternion.identity,null);
        c.GetComponent<Cannon>().Fire(target.transform.position);
    }

    public void StartDrift()
    {
        GetComponent<Collider>().isTrigger = false;
        drifting = true;
    }

    public void EndDrift()
    {
        GetComponent<Collider>().isTrigger = true;
        drifting = false;
    }

    public void Sink()
    {
        rig.velocity = -transform.right * 50 + new Vector3(0, 150, 0);
        rig = null;
        rag.Disembark();
        rag = null;
        EndDrift();
        sink = true;
    }

    public void CannonCtr(Vector3 dir)
    {
        
        Ray ray1 = new Ray(target.transform.position, target.transform.forward);
        RaycastHit hit1;
        if (Physics.Raycast(ray1, out hit1, 5000, mask))
        {
            zMax = hit1.point.z;
        }
        Ray ray2 = new Ray(target.transform.position, -target.transform.forward);
        RaycastHit hit2;
        if (Physics.Raycast(ray2, out hit2, 5000, mask))
        {
            zMin = hit2.point.z;
        }
        Ray ray3 = new Ray(target.transform.position, target.transform.right);
        RaycastHit hit3;
        if (Physics.Raycast(ray3, out hit3, 5000, mask))
        {
            xMax = hit3.point.x;
        }
        Ray ray4 = new Ray(target.transform.position, -target.transform.right);
        RaycastHit hit4;
        if (Physics.Raycast(ray4, out hit4, 5000, mask))
        {
            xMin = hit4.point.x;
        }
        target.transform.Translate(targetSpeed*dir * Time.deltaTime);
        float fx = Mathf.Clamp(target.transform.position.x, xMin, xMax);
        float fz = Mathf.Clamp(target.transform.position.z, zMin, zMax);
        target.transform.position = new Vector3(fx, 0, fz);
    }


    private void OnTriggerEnter(Collider other)
    {
        rag = other.transform.root.GetComponent<APRController>();
        if (!operated && rag != null)
        {
            operated = true;
            rag.GetBoat(this.gameObject);
            GetComponent<Collider>().isTrigger = false;
            rig = rag.Root.GetComponent<Rigidbody>();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        var ragdoll = collision.transform.root.GetComponent<APRController>();
        if (ragdoll != null)
        {
            var rig = ragdoll.Root.GetComponent<Rigidbody>();
            rig.velocity = new Vector3(-rig.velocity.x, 0, -rig.velocity.z).normalized * 100 + new Vector3(0, 40, 0);
            ragdoll.ActivateRagdoll(1);
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(operatePos.position, 0.5f);
    }
}
