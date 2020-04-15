using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
//玩家输入类
public class PlayerInput
{
    private int playerNum;
    public PlayerInput(int playerNumber)
    {
        playerNum = playerNumber;
        int keycode = 0;
        button = new KeyCode[6];
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
            button[i] = (KeyCode)keycode;
            keycode++;
        }
        horizontal = "Horizontal" + playerNum.ToString();
        vertical = "Vertical" + playerNum.ToString();
        trigger = "Trigger" + playerNum.ToString();
    }
    /// <summary>
    /// 0--A
    /// 1--B
    /// 2--X
    /// 3--Y
    /// 4--LB
    /// 5--RB
    /// </summary>
    public KeyCode[] button = null;
    public string horizontal;
    public string vertical;
    public string trigger;
}
