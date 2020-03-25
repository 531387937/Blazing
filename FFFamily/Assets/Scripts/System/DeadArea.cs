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
            //other.gameObject.GetComponent<RagdollController>().Ragdoll2OnlyAnim();
            other.gameObject.GetComponent<Rigidbody>().Sleep();
            PlayerRecover(other.gameObject.GetComponent<RagdollController>().playerNum);


            //other.gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
            //other.gameObject.GetComponent<Rigidbody>().Sleep();
            //other.gameObject.SetActive(false);
        }
    }
    private void PlayerRecover(int player)
    {
        GameManager.Instance.PlayerDead(player - 1);
    }
}
