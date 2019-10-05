using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbNormal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if ( collision.GetContact(0).point.y>gameObject.transform.position.y+0.4f && PlayerManager.Instance._players[collision.gameObject].state==playerState.Jump)
            {
                PlayerManager.Instance._players[this.gameObject].Dizzy();
                PlayerManager.Instance._players[collision.gameObject].state = playerState.OnGround;
            }
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        //if (collision.gameObject.CompareTag("Player"))
        //{
        //    PlayerManager.Instance._players[collision.gameObject].state = playerState.Jump;
        //}
    }
}
