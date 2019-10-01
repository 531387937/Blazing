using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum playerState
{
    OnGround = 1,
    Jump = 2,
}
[Serializable]
public class PlayerBase
{
    public float weight;

    public float jumpForce;

    public float moveSpeed;

    public int playerNum;
    /// <summary>
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

    private Transform m_parent;

    public GameObject _player;
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
    private Vector3 m_input;

    private Rigidbody rig
    {
        get { return _player.GetComponent<Rigidbody>(); }
    }
    public PlayerBase( int _playerNum,float speed, GameObject player )
    {
        playerNum = _playerNum;

        moveSpeed = speed;

        _player = player;

        m_parent = _player.transform.parent;
    }
    public PlayerBase(PlayerBase _base,GameObject player)
    {
        weight = _base.weight;
        jumpForce = _base.jumpForce;
        moveSpeed = _base.moveSpeed;
        _player = player;
        m_parent = _player.transform.parent;
    }
    public playerState state = playerState.OnGround;

    private KeyCode[] _button = null;

    public virtual Vector3 Move()
    {
        Vector3 _moveDir = Vector3.zero;

        return _moveDir;
    }

    private void inputListener()
    {
        foreach (KeyCode key in Buttons)
        {
            if (Input.GetKeyDown(key))
            {
                Debug.Log("按下了" + key);
            }
        }
        if(Input.GetAxis(_horizontal)!=0)
        {
            Debug.Log("摇杆" + playerNum + "的水平方向输入为"+Input.GetAxis(_horizontal));
        }
        if (Input.GetAxis(_vertical) != 0)
        {
            Debug.Log("摇杆" + playerNum + "的竖直方向输入为" + Input.GetAxis(_vertical));
        }
        m_input = SquareToCircle(new Vector2(Input.GetAxis(_horizontal), Input.GetAxis(_vertical)));
        Debug.Log(m_input);
        _player.transform.localPosition += m_input * moveSpeed * Time.deltaTime;
        jump();
    }
    
    public void playerUpdate()
    {
        inputListener();

        if(state == playerState.OnGround)
        {
            _player.transform.SetParent(m_parent);
        }
        else
        {
            _player.transform.SetParent(m_parent.parent);
        }
    }


    private Vector3 SquareToCircle(Vector2 input)
    {
        Vector3 output = Vector3.zero;
        output.x = input.x * Mathf.Sqrt(1 - (input.y * input.y) / 2.0f);
        output.z = -input.y * Mathf.Sqrt(1 - (input.x * input.x) / 2.0f);
        return output;
    }

    void jump()
    {
        if(Input.GetKeyDown(Buttons[0])&&state == playerState.OnGround)
        {
            rig.velocity = new Vector3(0, jumpForce, 0);
           
        }
    }
}
