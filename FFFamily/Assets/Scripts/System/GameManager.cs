using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public enum GameState
{
    Begin,
    Choose,
    Fight,
    End,
}
public class GameManager : Singleton<GameManager>
{
    public GameState gameState;
    private GameState curState;
    public AudioManager audioManager;
    public List<APRController> players = new List<APRController>(4);
    public MapManager mapManager;
    public PlayableDirector timeline;
    public float aliveNum = 4;
    private bool canChoose;
    private void Awake()
    {
        gameState = GameState.Begin;

        curState = GameState.Begin;
        var map = GameObject.Find("map");
        mapManager = new MapManager(map);
    }
    private void OnEnable()
    {
        EventManager.Instance.AddListener("PlayerDead", PlayerDead);
        EventManager.Instance.AddListener("EnterGame", OnEnterGame);
    }
    private void Update()
    {
        if(aliveNum==1)
        {
            GameOver();
        }
        if(gameState!=curState)
        {
            curState = gameState;
            switch(curState)
            {
                case GameState.Choose:
                    timeline.Play();
                    StartCoroutine(ChoosePlayer());
                    break;
                case GameState.Fight:

                    StartCoroutine(FightTimer());
                    
                    break;
            }
        }
        switch(curState)
        {
            case GameState.Choose:
                if (canChoose)
                {
                    int num = 0;
                    if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Q))
                    {
                        num = 1;
                    }
                    if (Input.GetKeyDown(KeyCode.Joystick2Button0) || Input.GetKeyDown(KeyCode.W))
                    {
                        num = 2;
                    }
                    if (Input.GetKeyDown(KeyCode.Joystick3Button0) || Input.GetKeyDown(KeyCode.E))
                    {
                        num = 3;
                    }
                    if (Input.GetKeyDown(KeyCode.Joystick4Button0) || Input.GetKeyDown(KeyCode.R))
                    {
                        num = 4;
                    }
                    if (num != 0)
                    {
                        for (int i = 0; i < players.Count; i++)
                        {
                            if (num == players[i].PlayerNum)
                            {
                                return;
                            }
                            else if (players[i].PlayerNum == 0)
                            {
                                players[i].PlayerNum = num;
                                players[i].input = new PlayerInput(num);
                                canChoose = false;
                                return;
                            }
                        }
                    }
                }
                break;
        }
    }
    private void GameOver()
    {
        //To Do胜者是最后的一个人
        Time.timeScale = 0;
        EventManager.Instance.TriggerEvent("GameOver", players[0].PlayerNum);
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
                break;
            }
        }
    }
    private void OnEnterGame(params object[] arg)
    {
        gameState = GameState.Choose;
        //EventManager.Instance.RemoveListener("EnterGame", OnEnterGame);
    }
    private void ChoosePlayer(params object[] arg)
    {
        canChoose = true;
    }
    IEnumerator ChoosePlayer()
    {
        EventManager.Instance.AddListener("ChoosePlayer", ChoosePlayer);
        int index = 0;
        while(index<4)
        {
            if(players[index].PlayerNum!=0)
            {
                EventManager.Instance.TriggerEvent("ChooseNext");
                index++;
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
        EventManager.Instance.RemoveListener("ChoosePlayer", ChoosePlayer);
        gameState = GameState.Fight;
    }
    IEnumerator FightTimer()
    {
        yield return new WaitForSeconds(3);
        timeline.enabled = false;
        foreach (var play in players)
        {
            play.useControls = true;
        }
        EventManager.Instance.TriggerEvent("GameStart");
    }
}
