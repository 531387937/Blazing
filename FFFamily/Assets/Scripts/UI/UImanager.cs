using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class UImanager : MonoBehaviour
{
    public GameObject PlayersState;
    public Animator gameOver;
    private DOTweenAnimation dotween;
    private void OnEnable()
    {
        EventManager.Instance.AddListener("GameStart", OnGameStart);
        EventManager.Instance.AddListener("GameOver", OnGameOver);
    }
    // Start is called before the first frame update
    void Start()
    {
        gameOver.speed = 0;
        dotween = gameOver.gameObject.GetComponent<DOTweenAnimation>();
        dotween.onComplete.AddListener(() =>
        {
            gameOver.speed = 1;
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnGameStart(params object[] arg)
    {
        PlayersState.SetActive(true);
    }

    void OnGameOver(params object[] arg)
    {
        gameOver.gameObject.SetActive(true);
        dotween.DOPlay();
        }
}
