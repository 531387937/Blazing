using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum playerState
{
    OnGround = 1,
    Jump = 2,
    Falling = 3,
}
[RequireComponent(typeof(Rigidbody))]
public class Players : MonoBehaviour
{
    //重量
    public float weight;

    //跳跃力量
    public float jumpForce;

    //移动加速度
    public float moveSpeed;

    public int playerNum;

    public float maxSpeed;
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
    public playerState state = playerState.OnGround;
    private string _horizontal;
    private string _vertical;
    private KeyCode[] _button = null;
    private Rigidbody rig;
    private Vector3 _input;

    private bool canJump = true;
    private bool canMove = true;
    private GameObject plate;
    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody>();
        plate = GameObject.Find("Plate");
        _horizontal = "Horizontal" + playerNum.ToString();
        _vertical = "Vertical" + playerNum.ToString();
    }

    private void Update()
    {
        
        StateListener();
        transform.rotation = plate.transform.rotation;
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        if(canMove)
        inputListener();
    }

    //输入监听
    private void inputListener()
    {
        _input = SquareToCircle(new Vector2(Input.GetAxis(_horizontal), Input.GetAxis(_vertical)))*moveSpeed*Time.deltaTime;
        _input = changeByAngle(_input);
        Vector3 targetPos = _input * Time.deltaTime + transform.localPosition;
        rig.MovePosition(targetPos);
        //跳跃
        if (Input.GetKeyDown(Buttons[0]) && state == playerState.OnGround &&canJump)
        {
            canJump = false;
            rig.velocity += new Vector3(0, jumpForce, 0);
            state = playerState.Jump;
        }
    }

    private void StateListener()
    {
        if (state != playerState.OnGround)
        {
            canJump = false;
        }
        else
            canJump = true;
    }

    //将输入化为一个圆，保证各方向行走相等
    private Vector3 SquareToCircle(Vector2 input)
    {
        Vector3 output = Vector3.zero;
        output.x = input.x * Mathf.Sqrt(1 - (input.y * input.y) / 2.0f);
        output.z = -input.y * Mathf.Sqrt(1 - (input.x * input.x) / 2.0f);
        return output;
    }
    //根据角度修改行进方向
    private Vector3 changeByAngle(Vector3 orginal)
    {
        Vector3 a;
        float sinX = Mathf.Sin(plate.transform.rotation.x * 2 / Mathf.PI);
        float cosX = Mathf.Cos(plate.transform.rotation.x * 2 / Mathf.PI);
        float sinZ = Mathf.Sin(plate.transform.rotation.z * 2 / Mathf.PI);
        float cosZ = Mathf.Cos(plate.transform.rotation.z * 2 / Mathf.PI);
        a = new Vector3(orginal.x * cosX, orginal.x * sinX + orginal.z * sinZ, orginal.z * cosZ);
        return a;
    }

    public void Dizzy()
    {
        GetComponent<MeshRenderer>().material.color = Color.blue;
        canMove = false;
        BuffLogic.Instance.EffectRecover(() =>
        {
            GetComponent<MeshRenderer>().material.color = Color.white;
            canMove = true;
        }, Global.dizzyEffect);
    }

    public void Fall()
    {
        state = playerState.Falling;
        transform.position = new Vector3(0, 15, 0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.GetContact(0).point.y > gameObject.transform.position.y + 0.4f && collision.gameObject.GetComponent<Players>().state == playerState.Jump)
            {
                Dizzy();
                collision.gameObject.GetComponent<Players>().state = playerState.OnGround;
            }
        }
    }
}
