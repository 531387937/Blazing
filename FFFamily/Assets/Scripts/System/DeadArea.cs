using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (PlayerManager.Instance._players.ContainsKey(other.gameObject))
        {
            other.gameObject.transform.localPosition = new Vector3(0, 15, 0);
            PlayerManager.Instance._players[other.gameObject].Fall();
        }
    }
}
