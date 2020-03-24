using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blood : MonoBehaviour
{
    public RagdollController ragCtr;

    private Image hp;
    private Text text;
    float recoverTimer = 3;
    private void Awake()
    {
        text = GetComponentInChildren<Text>();
    }
    // Start is called before the first frame update
    void Start()
    {
        hp = GetComponent<Image>();
        text.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (ragCtr != null)
        {
            hp.fillAmount = (10 - ragCtr.Stun) / 10;
            if (!ragCtr.gameObject.activeInHierarchy)
            {
                text.gameObject.SetActive(true);
                recoverTimer -= Time.deltaTime;
                text.text = ((int)recoverTimer).ToString();
            }
            else
            {
                recoverTimer = 3;
                text.gameObject.SetActive(false);
            }
        }
    }
}
