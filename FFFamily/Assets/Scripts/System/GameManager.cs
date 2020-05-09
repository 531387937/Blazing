using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public AudioManager audioManager;
    public List<APRController> players = new List<APRController>(4);
    public MapManager mapManager;

    private Event_CallBack playerDeadCallback;
    public float aliveNum = 4;
    private void Awake()
    {
        var map = GameObject.Find("map");
        mapManager = new MapManager(map);
        playerDeadCallback += PlayerDead;
        EventManager.Instance.AddListener("PlayerDead", playerDeadCallback);
    }

    private void Update()
    {
        if(aliveNum==1)
        {
            GameOver();
        }
    }
    private void GameOver()
    {
        //To Do胜者是最后的一个人
    }

    private void PlayerDead(params object[] arg)
    {
        int num = (int)arg[0];
        foreach(var player in players)
        {
            if(player.PlayerNum==num)
            {
                aliveNum--;
                players.Remove(player);
                player.gameObject.SetActive(false);
                break;
            }
        }
    }
}
