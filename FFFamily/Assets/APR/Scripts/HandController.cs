using System.Collections;
using UnityEngine;

public class HandController : MonoBehaviour
{
    public APRController APR_Player;
	public bool Left;
	public bool hasJoint;
    private bool hasWaitedAfterThrow = true;
	

    void Update()
    {
        if(APR_Player.useControls)
        {
            //Left Hand
            //On key release destroy joint
            if(Left)
            {
                if(hasJoint && Input.GetKeyUp(APR_Player.reachLeft))
                {
                    this.gameObject.GetComponent<FixedJoint>().breakForce = 0;
                    hasJoint = false;
                    hasWaitedAfterThrow = false;
                }

                if(hasJoint && this.gameObject.GetComponent<FixedJoint>() == null)
                {
                    hasJoint = false;
                    hasWaitedAfterThrow = false;
                }
            }

            //Right Hand
            //On key release destroy joint
            if(!Left)
            {
                if(hasJoint && Input.GetKeyUp(APR_Player.reachRight))
                {
                    this.gameObject.GetComponent<FixedJoint>().breakForce = 0;
                    hasJoint = false;
                    hasWaitedAfterThrow = false;
                }

                if(hasJoint && this.gameObject.GetComponent<FixedJoint>() == null)
                {
                    hasJoint = false;
                    hasWaitedAfterThrow = false;
                }
            }
        }
    }

    //Grab on collision
    void OnCollisionEnter(Collision col)
    {
        if(APR_Player.useControls && hasWaitedAfterThrow)
        {
            //Left Hand
            if(Left)
            {
                if(col.gameObject.tag == "Object" && !hasJoint)
                {
                    if(Input.GetKey(APR_Player.reachLeft) && !hasJoint)
                    {
                        hasJoint = true;
                        hasWaitedAfterThrow = false;
                        this.gameObject.AddComponent<FixedJoint>();
                        this.gameObject.GetComponent<FixedJoint>().breakForce = 100000;
                        this.gameObject.GetComponent<FixedJoint>().connectedBody = col.gameObject.GetComponent<Rigidbody>();
                    }
                }
                
                else if(col.gameObject.tag == "Player" && col.gameObject.layer != LayerMask.NameToLayer(APR_Player.thisPlayerLayer) && !hasJoint)
                {
                    if(Input.GetKey(APR_Player.reachLeft) && !hasJoint)
                    {
                        hasJoint = true;
                        hasWaitedAfterThrow = false;
                        this.gameObject.AddComponent<FixedJoint>();
                        this.gameObject.GetComponent<FixedJoint>().breakForce = 100000;
                        this.gameObject.GetComponent<FixedJoint>().connectedBody = col.gameObject.GetComponent<Rigidbody>();
                    }
                }
            }

            //Right Hand
            if(!Left)
            {
                if(col.gameObject.tag == "Object" && !hasJoint)
                {
                    if(Input.GetKey(APR_Player.reachRight) && !hasJoint)
                    {
                        hasJoint = true;
                        hasWaitedAfterThrow = false;
                        this.gameObject.AddComponent<FixedJoint>();
                        this.gameObject.GetComponent<FixedJoint>().breakForce = 100000;
                        this.gameObject.GetComponent<FixedJoint>().connectedBody = col.gameObject.GetComponent<Rigidbody>();
                    }
                }
                
                if(col.gameObject.tag == "Player" && col.gameObject.layer != LayerMask.NameToLayer(APR_Player.thisPlayerLayer) && !hasJoint)
                {
                    if(Input.GetKey(APR_Player.reachRight) && !hasJoint)
                    {
                        hasJoint = true;
                        hasWaitedAfterThrow = false;
                        this.gameObject.AddComponent<FixedJoint>();
                        this.gameObject.GetComponent<FixedJoint>().breakForce = 100000;
                        this.gameObject.GetComponent<FixedJoint>().connectedBody = col.gameObject.GetComponent<Rigidbody>();
                    }
                }
            }
        }
    }
    
    void OnJointBreak()
    {
        StartCoroutine (DelayCoroutine());
        IEnumerator DelayCoroutine()
        {
            yield return new WaitForSeconds(1f);
            hasWaitedAfterThrow = true;
        }
    }
}
