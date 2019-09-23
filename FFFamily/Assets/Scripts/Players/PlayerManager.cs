using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public PlayerBase[] _players;
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0;i<_players.Length;i++)
        {
            _players[i] = new PlayerBase(i + 1);
        }
    }

    // Update is called once per frame
    void Update()
    {
        PlayerInputListener();
    }
    //监听玩家的输入
    void PlayerInputListener()
    {
        foreach(var player in _players)
        {
            player.inputListener();
        }
    }
}
