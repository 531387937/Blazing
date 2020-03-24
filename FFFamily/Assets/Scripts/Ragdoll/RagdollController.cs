using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RagdollMecanimMixer;

public class RagdollController : MonoBehaviour
{
    //移动
    private Vector3 inputDirection;
    private float inputVelocity;
    private Camera cam;
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
            if (stunned)
            {
                return;
            }
            if (blocking)
            {
                stun = value * 0.2f;
            }
            else
                stun = value;
            if (stun >= 10)
            {
                Ragdoll2Stunned();
            }
        }
    }
    public HitManager hitManager;

    [HideInInspector]
    public Animator anim;

    [Range(0.01f, 10)]
    private float stun;
    private RamecanMixer ramecanMixer;
    private Rigidbody rb;
    //是否在防御
    private bool blocking = false;

    private bool dead = false;
    private bool stunned = false;
    //脱战时间
    private float peaceTimer = 0;
    //private Collider col;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        ramecanMixer = GetComponent<RamecanMixer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ChangeRagdollState("normal");
    }

    // Update is called once per frame
    void Update()
    {
        inputDirection = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0) * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized;
        inputVelocity = Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical")));

        float angle = Vector3.SignedAngle(transform.forward, inputDirection, Vector3.up);
        //anim.SetFloat("direction", angle / 180);
        anim.SetFloat("velocity", inputVelocity);

        if (!dead && !stunned)
        {
            anim.SetBool("block", Input.GetKeyDown(KeyCode.LeftShift));
            if (Input.GetMouseButtonDown(0))
            {
                anim.SetTrigger("attack");
            }
            if (Input.GetMouseButtonDown(1))
            {
                anim.SetTrigger("grab");
                Ragdoll2Grab();
            }
            //To Do
            //移动逻辑 动画机里对应float变量velocity 

        }
        if (!hitManager.fighting)
        {
            peaceTimer += Time.deltaTime;
        }
        else
        {
            peaceTimer = 0;
        }
        if (peaceTimer >= 5 && stun >= 0)
        {
            stun -= 0.8f * Time.deltaTime;
        }
    }

    void LateUpdate()
    {
        Vector3 directionToTarget = transform.forward;
        if (inputVelocity > 0)
            directionToTarget = inputDirection;
        Quaternion rotation = Quaternion.LookRotation(directionToTarget.normalized, Vector3.up);
        rb.rotation = Quaternion.Slerp(rb.rotation, Quaternion.Euler(0, rotation.eulerAngles.y, 0), Time.deltaTime * 10);
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
    public void Ragdoll2Die()
    {
        if (!dead)
        {
            ChangeRagdollState("die");
            anim.SetBool("death", true);
            anim.SetBool("stun", false);
            anim.SetBool("block", false);
            dead = true;
            stunned = false;
            hitManager.StopHit();
            anim.applyRootMotion = false;
            StartCoroutine(DeathTimer());
        }
        //TO DO 切换动画为死亡
    }
    private void Ragdoll2Grab()
    {
        ChangeRagdollState("grabing");
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
        anim.applyRootMotion = true;
        Vector3 reviveDir = ramecanMixer.RootBoneTr.forward;
        Quaternion reviveRot = Quaternion.LookRotation(-reviveDir, Vector3.up);
        rb.rotation = Quaternion.Euler(0, reviveRot.eulerAngles.y, 0);
        rb.transform.rotation = Quaternion.Euler(0, 0, 0);
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
        stun = 0.01f;
        stunned = false;
        Ragdoll2Normal();
    }
    //开始有击打判定
    public void BeginHit(int mode)
    {
        hitManager.BeginHit((hitMode)mode);
    }
    //取消击打判定
    public void StopHit()
    {
        hitManager.StopHit();
    }
}
