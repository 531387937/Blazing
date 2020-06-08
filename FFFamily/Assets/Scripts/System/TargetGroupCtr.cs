using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class TargetGroupCtr : MonoBehaviour
{
    private CinemachineTargetGroup group;
    private void OnEnable()
    {
        EventManager.Instance.AddListener("PlayerDead", OnPlayerDead);
        EventManager.Instance.AddListener("GameOver", OnGameOver);
    }
    // Start is called before the first frame update
    void Start()
    {
        group = GetComponent<CinemachineTargetGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnPlayerDead(params object[] arg)
    {
        int index = (int)arg[0];
        for(int i = 0;i<group.m_Targets.Length;i++)
        {
            if (group.m_Targets[i].target.root.gameObject.GetComponent<APRController>())
            {
                if (group.m_Targets[i].target.root.gameObject.GetComponent<APRController>().PlayerNum == index)
                {
                    group.m_Targets[i].weight = 0;
                    return;
                }
            }
        }
    }
    void OnGameOver(params object[] arg)
    {
        int winner = (int)arg[0];
        for (int i = 0; i < group.m_Targets.Length; i++)
        {
            if (group.m_Targets[i].target.root.GetComponent<APRController>().PlayerNum == winner)
            {
                group.m_Targets[i].radius = 1f;
                return;
            }
        }
    }
}
