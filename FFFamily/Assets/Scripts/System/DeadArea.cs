using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //other.gameObject.transform.localPosition = new Vector3(0, 15, 0);
            other.gameObject.GetComponent<Players>().Fall();
        }
    }
}
