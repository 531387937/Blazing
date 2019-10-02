using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public PlayerBase _base;

    public Transform _instanPos;

    public List<PlayerBase> _playersCtr;

    public GameObject prefab_Player;

    public Dictionary<GameObject, PlayerBase> _players = new Dictionary<GameObject, PlayerBase>();
    // Start is called before the first frame update
    private void Awake()
    {
        
    }
    void Start()
    {
        for( int i = 0; i<_playersCtr.Count; i++)
        {
            GameObject _player = Instantiate(prefab_Player, _instanPos);
            _playersCtr[i] = new PlayerBase(_base,_player);
            _playersCtr[i].playerNum = i+1;
            _playersCtr[i]._player = _player;
            _players.Add(_player,_playersCtr[i]);
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
        foreach(var playerCtr in _playersCtr)
        {
            playerCtr.playerUpdate();
        }
    }

    public void cdCount(float seconds,Action callback)
    {
        StartCoroutine(cdCounts(seconds, callback));
    }

    IEnumerator cdCounts(float seconds, Action callback)
    {
        yield return new WaitForSeconds(seconds);
        callback();
    }
}
