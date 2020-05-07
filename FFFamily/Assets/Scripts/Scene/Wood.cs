using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : Grid
{
    public float destoryTime;
    private Material material;
    private float timer;
    private bool des;
    private void Awake()
    {
        type = GridType.Wood;
    }
    private void Update()
    {
        if (des)
        {
            material.color = Color.Lerp(material.color, Color.red, 0.5f * Time.deltaTime);
            timer += Time.deltaTime;
            if (timer >= destoryTime)
            {
                Destroy(this.gameObject);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.root.GetComponent<APRController>() != null)
        {
            var obj = collision.gameObject;
            var ctr = collision.transform.root.GetComponent<APRController>();
            if (ctr.down)
            {
                ctr.down = false;
                material = new Material(GetComponent<Renderer>().material);
                GetComponent<Renderer>().material = material;
                des = true;
            }
        }
    }
}
