﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RagdollMecanimMixer;

public class RagdollController : MonoBehaviour
{
    public bool canHit = true;
    public int playerNum;
    /// <summary>
    /// 手柄映射
    /// 0--A
    /// 1--B
    /// 2--X
    /// 3--Y
    /// 4--LB
    /// 5--RB
    /// </summary>
    public KeyCode[] Buttons
    {
        get
        {
            if (_button == null)
            {
                _button = new KeyCode[6];
                int keycode = 0;
                switch (playerNum)
                {
                    case 1:
                        keycode = (int)KeyCode.Joystick1Button0;
                        break;
                    case 2:
                        keycode = (int)KeyCode.Joystick2Button0;
                        break;
                    case 3:
                        keycode = (int)KeyCode.Joystick3Button0;
                        break;
                    case 4:
                        keycode = (int)KeyCode.Joystick4Button0;
                        break;
                }
                for (int i = 0; i < 6; i++)
                {
                    _button[i] = (KeyCode)keycode;
                    keycode++;
                }
            }
            return _button;
        }
    }
    private KeyCode[] _button = null;
    private string _horizontal
    {
        get
        {
            return "Horizontal" + playerNum.ToString();
        }
    }
    private string _vertical
    {
        get
        {
            return "Vertical" + playerNum.ToString();
        }
    }
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
    [HideInInspector]
    public bool recoving = false;

    public ParticleSystem stunFX;

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

        if (!dead && !stunned&&canHit)
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
            inputDirection = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0) * new Vector3(Input.GetAxis(_horizontal), 0, Input.GetAxis(_vertical)).normalized;
            inputVelocity = Mathf.Max(Mathf.Abs(Input.GetAxis(_horizontal)), Mathf.Abs(Input.GetAxis(_vertical)));

            float angle = Vector3.SignedAngle(transform.forward, inputDirection, Vector3.up);
            //anim.SetFloat("direction", angle / 180);
            anim.SetFloat("velocity", inputVelocity);

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
    public void Ragdoll2Normal()
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
        hitManager.StopHit();
        StartCoroutine(StunnedTimer());
    }
    public void Ragdoll2OnlyAnim()
    {
        ChangeRagdollState("animOnly");
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
        stunFX.gameObject.SetActive(true);
        stunFX.Play();
        yield return new WaitForSeconds(stunTime);
        stunFX.gameObject.SetActive(false);
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
