using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public RecoverManager recoverManager;
    public AudioManager audioManager;
    public List<int> deadCount = new List<int>(4);
    public List<RagdollController> players = new List<RagdollController>(4);
    public float gameTime;
    private bool gameOver;
    [SerializeField]
    private bool gameStart;
    public void PlayerDead(int i)
    {
        deadCount[i]++;
        recoverManager.RecoverPlayer(i);
    }
    private void Start()
    {
        
    }
    private void Update()
    {
        if (!gameOver && gameStart)
            gameTime -= Time.deltaTime;
        if (gameTime <= 0&&!gameOver)
        {
            gameOver = true;
        }
    }
    private void GameOver()
    {
        int winner = 0;
        for(int i = 0;i<deadCount.Count;i++)
        {
            if (deadCount[winner] > deadCount[i])
            {
                winner = i;
            }
        }
    }
}
