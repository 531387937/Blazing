using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RagdollMecanimMixer;

public class RagdollController : MonoBehaviour
{
    //眩晕时长
    public float stunTime = 2f;
    //死亡时长
    public float deathTime = 3f;
    //眩晕槽
    public float Stun
    {
        get { return stun; }
        set
        {
            if(stunned)
            {
                return;
            }
            stun = value;
            if(stun>=10)
            {
                Ragdoll2Stunned();
                stun = 0;
            }
        }
    }
    public HitManager hitManager;
    private float stun;
    private RamecanMixer ramecanMixer;
    private Animator anim;
    private Rigidbody rb;

    private bool dead = false;
    private bool stunned = false;
    //private Collider col;
    // Start is called before the first frame update
    void Start()
    {
        ramecanMixer = GetComponent<RamecanMixer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ChangeRagdollState("normal");
    }

    // Update is called once per frame
    void Update()
    {
        if(!dead && !stunned)
        {
            anim.SetBool("block", Input.GetKeyDown(KeyCode.LeftShift));
            if(Input.GetMouseButtonDown(0))
            {
                anim.SetTrigger("attack");
            }
            if(Input.GetMouseButtonDown(1))
            {
                anim.SetTrigger("grab");
            }
            //To Do
            //移动逻辑 动画机里对应float变量velocity 
        }
    }

    /// <summary>
    /// 将布娃娃转换为正常状态
    /// </summary>
    private void Ragdoll2Normal()
    {
        ChangeRagdollState("normal");
    }
    /// <summary>
    /// 将布娃娃动画切换为纯布娃娃状态
    /// </summary>
    private void Ragdoll2Die()
    {
        if (!dead)
        {
            ChangeRagdollState("die");
            anim.SetBool("dead", true);
            anim.SetBool("stun", false);
            anim.SetBool("block", false);
            dead = true;
            stunned = false;
            StartCoroutine(DeathTimer());
        }
        //TO DO 切换动画为死亡
    }
    /// <summary>
    /// 布娃娃切换为眩晕状态
    /// </summary>
    private void Ragdoll2Stunned()
    {
        ChangeRagdollState("stunned");
        anim.SetBool("stun", true);
        stunned = true;
        StartCoroutine(StunnedTimer());
    }
    /// <summary>
    /// 修改布娃娃的状态接口
    /// </summary>
    /// <param name="state">状态名</param>
    private void ChangeRagdollState(string state)
    {
        ramecanMixer.BeginStateTransition(state);
    }
    //死亡倒计时
    IEnumerator DeathTimer()
    {
        yield return new WaitForSeconds(deathTime);
        Vector3 reviveDir = ramecanMixer.RootBoneTr.forward;
        Quaternion reviveRot = Quaternion.LookRotation(-reviveDir, Vector3.up);
        rb.rotation = Quaternion.Euler(0, reviveRot.eulerAngles.y, 0);
        //Time.timeScale = 1;
        anim.SetBool("death", false);

        dead = false;
        Ragdoll2Normal();
    }
    //眩晕倒计时
    IEnumerator StunnedTimer()
    {
        anim.ResetTrigger("attack");
        yield return new WaitForSeconds(stunTime);
        anim.SetBool("stun", false);
        stunned = false;
        Ragdoll2Normal();
    }
    public void BeginHit(int mode)
    {
        hitManager.BeginHit((hitMode)mode);
    }
    public void StopHit()
    {
        hitManager.StopHit();
    }
}
