using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerRecover(other.gameObject.GetComponent<RagdollController>().playerNum);
            other.gameObject.transform.parent.GetChild(0).gameObject.SetActive(false);
            other.gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
            other.gameObject.SetActive(false);
        }
    }
    private void PlayerRecover(int player)
    {
        GameManager.Instance.PlayerDead(player - 1);
    }
}
