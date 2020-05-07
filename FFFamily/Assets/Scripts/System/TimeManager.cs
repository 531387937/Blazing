using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TimeManager : MonoBehaviour
{
    private Text timer;
    private string sec;
    // Start is called before the first frame update
    void Start()
    {
        timer = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
