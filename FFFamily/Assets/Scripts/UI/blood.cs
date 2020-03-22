using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class blood : MonoBehaviour
{
    Image HP;
    // Start is called before the first frame update
    void Start()
    {
        HP = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown ("x"))
            HP.fillAmount = HP.fillAmount - 0.1f;
    }
}
