using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UImanager : MonoBehaviour
{
    public GameObject PlayersState;
    private void OnEnable()
    {
        EventManager.Instance.AddListener("GameStart", GameStart);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GameStart(params object[] arg)
    {
        PlayersState.SetActive(true);
    }
}
