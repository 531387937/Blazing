using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartManager : MonoBehaviour
{

    private void OnEnable()
    {
        EventManager.Instance.TriggerEvent("GameStart");
    }
    // Update is called once per frame
}
