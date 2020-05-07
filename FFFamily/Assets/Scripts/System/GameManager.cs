using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public AudioManager audioManager;
    public List<APRController> players = new List<APRController>(4);
    public MapManager mapManager;
    public void PlayerDead(int i)
    {
    }
    private void Awake()
    {
        var map = GameObject.Find("map");
        mapManager = new MapManager(map);
    }
    private void Update()
    {

    }
    private void GameOver()
    {
    }
}
