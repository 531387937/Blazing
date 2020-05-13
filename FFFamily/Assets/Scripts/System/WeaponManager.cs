using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public List<GameObject> weapon;
    private bool drowpDown = false;
    public float dropTime = 15f;
    private float timer;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void OnEnable()
    {
        EventManager.Instance.AddListener("GameStart", OnGameStart);
    }
    // Update is called once per frame
    void Update()
    {
        if(drowpDown)
        {
            timer += Time.deltaTime;
            if(timer>dropTime)
            {
                timer = 0;
                WeaponDropDown();
            }
        }
    }

    void WeaponDropDown()
    {
        int w = Random.Range(0, weapon.Count - 1);
        Vector3 pos = GameManager.Instance.mapManager.GetFreeGrid();
        Instantiate(weapon[w], pos, weapon[w].transform.rotation);
    }

    void OnGameStart(params object[] arg)
    {
        drowpDown = true;
    }
}
