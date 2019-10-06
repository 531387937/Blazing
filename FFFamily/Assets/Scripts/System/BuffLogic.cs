using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropType
{
    None = 0,
    FAT = 1,
}

public interface Prop
{
    void Effect(Players _player);
}


public class BuffLogic : Singleton<BuffLogic>
{
    public GameObject test;
    // Start is called before the first frame update
    void Start()
    {
        test.AddComponent<FatProp>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EffectRecover(Action recover, float seconds)
    {
        StartCoroutine(Recover(recover, 3));
    }

    IEnumerator Recover(Action recover,float seconds)
    {
        yield return new WaitForSeconds(seconds);
        recover();
    }
}

public class FatProp : MonoBehaviour,Prop
{
    public void Effect(Players _player)
    {
        _player.weight *= Global.fatEffect;
        BuffLogic.Instance.EffectRecover(()=> { _player.weight /= Global.fatEffect;
            _player.GetComponent<MeshRenderer>().material.color = Color.white;
        }, 3);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(other.CompareTag("Player"))
        {
            Effect(other.GetComponent<Players>());
            other.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }
}
