using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CamCtr : MonoBehaviour
{
    public CinemachineFreeLook freeLook;
    public CinemachineVirtualCamera follow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void changeCam(bool isFreeLook)
    {
        if(isFreeLook)
        {
            freeLook.Priority = 11;
            follow.Priority = 9;
        }
        else
        {
            freeLook.Priority = 9;
            follow.Priority = 11;
        }
    }
}
