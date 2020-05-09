using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blood : MonoBehaviour
{
    public APRController ragCtr;

    private Image power;
    public Sprite deadSprite;
    private void OnEnable()
    {
        EventManager.Instance.AddListener("PlayerDead", Dead);
    }
    // Start is called before the first frame update
    void Start()
    {
        power = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ragCtr != null)
        {
            power.fillAmount = ragCtr.Power-1;
            if(power.fillAmount==1)
            {
                //燃烧
            }
        }
    }

    private void Dead(params object[] arg)
    {
        if ((int)arg[0] == ragCtr.PlayerNum)
        {
            EventManager.Instance.RemoveListener("PlayerDead", Dead);
            power.fillAmount = 0;
            transform.parent.GetComponent<Image>().sprite = deadSprite;
        }
    }
}
