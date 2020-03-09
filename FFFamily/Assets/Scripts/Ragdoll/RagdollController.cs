using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RagdollMecanimMixer;

public class RagdollController : MonoBehaviour
{
    private RamecanMixer ramecanMixer;
    private Animator anim;
    private Rigidbody rb;

    private bool dead = false;
    //private Collider col;
    // Start is called before the first frame update
    void Start()
    {
        ramecanMixer = GetComponent<RamecanMixer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        //col = GetComponent<Collider>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            RagdollReceive();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Ragdoll2Die();
        }
    }

    public void RagdollReceive()
    {
        Vector3 reviveDir = ramecanMixer.RootBoneTr.forward;
        Quaternion reviveRot = Quaternion.LookRotation(-reviveDir, Vector3.up);
        rb.rotation = Quaternion.Euler(0, reviveRot.eulerAngles.y, 0);
        //Time.timeScale = 1;
        anim.SetTrigger("reviveUp");

        anim.SetBool("dead", false);
        dead = false;
        //rb.isKinematic = false;
        //col.enabled = true;
        ramecanMixer.BeginStateTransition("default");
    }
    /// <summary>
    /// 将布娃娃转换为正常状态
    /// </summary>
    private void Ragdoll2Normal()
    {
        ChangeRagdollState("default");
    }
    /// <summary>
    /// 将布娃娃动画切换为纯布娃娃状态
    /// </summary>
    private void Ragdoll2Die()
    {
        ChangeRagdollState("die");
        anim.SetBool("dead", true);
        //TO DO 切换动画为死亡
    }

    private void ChangeRagdollState(string state)
    {
        ramecanMixer.BeginStateTransition(state);
    }
}
