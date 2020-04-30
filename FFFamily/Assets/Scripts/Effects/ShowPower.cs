using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowPower : MonoBehaviour
{
    public APRController controller;
    private Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        material.SetFloat("_OutlineSize", controller.Power*2);
    }
}
