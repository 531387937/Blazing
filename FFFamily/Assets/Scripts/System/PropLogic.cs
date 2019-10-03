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
    public void EffectRecover(PropType type, PlayerBase _pb, float seconds)
    {
        StartCoroutine(Recover(PropType.FAT, _pb, 3));
    }

    IEnumerator Recover(PropType type,PlayerBase _pb,float seconds)
    {
        print("????????????????");
        yield return new WaitForSeconds(seconds);
        print("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");
        switch (type)
        {
            case PropType.FAT:
                print("!!!!!!!!!!!!!!!!!!!!");
                _pb.weight /= Global.fatEffect;
                _pb._player.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                break;
        }
    }
}

public class FatProp : MonoBehaviour,Prop
{
    private PlayerBase _pb;
    public void Effect(PlayerBase Pb)
    {
        _pb.weight *= Global.fatEffect;
        PropLogic.Instance.EffectRecover(PropType.FAT, _pb, 3);
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
