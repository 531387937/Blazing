using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class DeadArea : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.transform.root.GetComponent<APRController>())
            {
                int num = other.transform.root.GetComponent<APRController>().PlayerNum;
                EventManager.Instance.TriggerEvent("PlayerDead", num);
            }
        }
        else if(!other.CompareTag("Ground"))
        {
            GameManager.Instance.audioManager.PlaySound("落水声");
        }
    }
}
