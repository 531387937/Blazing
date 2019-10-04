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
    void Effect(PlayerBase Pb);
}


public class PropLogic : Singleton<PropLogic>
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
    public void EffectRecover(Action recover, PlayerBase _pb, float seconds)
    {
        StartCoroutine(Recover(recover, _pb, 3));
    }

    IEnumerator Recover(Action recover, PlayerBase _pb,float seconds)
    {
        yield return new WaitForSeconds(seconds);
        recover();
    }
}

public class FatProp : MonoBehaviour,Prop
{
    private PlayerBase _pb;
    public void Effect(PlayerBase Pb)
    {
        _pb.weight *= Global.fatEffect;
        PropLogic.Instance.EffectRecover(()=> { _pb.weight /= Global.fatEffect;
            _pb._player.GetComponent<MeshRenderer>().material.color = Color.white;
        }, _pb, 3);
        Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {        
        if(PlayerManager.Instance._players.TryGetValue(other.gameObject,out _pb))
        {
            Effect(_pb);
            other.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
        }
    }
}
