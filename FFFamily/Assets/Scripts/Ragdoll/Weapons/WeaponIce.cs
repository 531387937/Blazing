using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponIce : MonoBehaviour
{
    private GameObject ice;
    private Material ma;
    public float cutTime;
    bool beginCutOff;
    // Start is called before the first frame update
    void Start()
    {
        ice = transform.GetChild(0).gameObject;
        ma =new Material(ice.GetComponent<MeshRenderer>().material);
        ice.GetComponent<MeshRenderer>().material = ma;
    }

    // Update is called once per frame
    void Update()
    {
        if(beginCutOff)
        {
            float cutoff = ma.GetFloat("_Cutoff");
            cutoff -= Time.deltaTime/cutTime;
            ma.SetFloat("_Cutoff", cutoff);
            if(cutoff<=0.05f)
            {
                beginCutOff = false;
                var weapon = ice.transform.GetChild(0);
                GetComponent<BoxCollider>().isTrigger = true;
                weapon.SetParent(null);
                foreach(var collider in weapon.GetComponents<Collider>())
                {
                    collider.isTrigger = false;
                }
                weapon.GetComponent<Rigidbody>().isKinematic = false;
                Destroy(this.gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        beginCutOff = true;
    }
}
