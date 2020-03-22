using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blood : MonoBehaviour
{
    public RagdollController ragCtr;

    private Image hp;
    // Start is called before the first frame update
    void Start()
    {
        hp = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ragCtr != null)
            hp.fillAmount = (10 - ragCtr.Stun) / 10;
    }
}
