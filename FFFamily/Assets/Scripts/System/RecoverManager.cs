using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverManager : MonoBehaviour
{
    public float recoverTime = 5;
    public float posSpeed = 2f;
    public CinemachineTargetGroup target;
    private Camera cam;
    private bool survie = false;
    public List<GameObject> recoverPos = new List<GameObject>(4);
    private void Awake()
    {
        cam = Camera.main;
    }
    public void RecoverPlayer(int a)
    {
        for (int i = 0; i < target.m_Targets.Length; i++)
        {
            if (target.m_Targets[i].target.gameObject == GameManager.Instance.players[a].gameObject)
            {
                target.m_Targets[i].target = recoverPos[a].transform;
                StartCoroutine(surving(a,i));
            }
        }
        recoverPos[a].SetActive(true);
        survie = true;
        
    }
    private void Update()
    {
        if(survie)
        {
            for(int i = 0;i<4;i++)
            {
                if(recoverPos[i].activeInHierarchy)
                {
                    survie = true;
                    string _horizontal = "Horizontal" + (i + 1).ToString();
                    string _vertical = "Vertical" + (i + 1).ToString();
                    Vector3 inputDirection = Quaternion.Euler(0, cam.transform.rotation.eulerAngles.y, 0) * new Vector3(Input.GetAxis(_horizontal), 0, Input.GetAxis(_vertical)).normalized;
                    float inputVelocity = Mathf.Max(Mathf.Abs(Input.GetAxis(_horizontal)), Mathf.Abs(Input.GetAxis(_vertical)));
                    recoverPos[i].transform.position += inputDirection * inputVelocity * posSpeed * Time.deltaTime;
                }
            }
        }
    }
    IEnumerator surving(int i,int a)
    {
        //Transform t = GameManager.Instance.players[i].gameObject.transform.parent.GetChild(0).gameObject.transform;
        yield return new WaitForSeconds(recoverTime);
        recoverPos[i].SetActive(false);
        //GameManager.Instance.players[i].gameObject.transform.position = recoverPos[i].transform.position + new Vector3(0, 5, 0);
        GameManager.Instance.players[i].gameObject.transform.parent.position += recoverPos[i].transform.position - GameManager.Instance.players[i].gameObject.transform.position + new Vector3(0, 10, 0);
        //GameManager.Instance.players[i].gameObject.SetActive(true);
        //GameManager.Instance.players[i].gameObject.transform.parent.GetChild(0).gameObject.SetActive(true);
        GameManager.Instance.players[i].gameObject.GetComponent<Rigidbody>().WakeUp();
        GameManager.Instance.players[i].gameObject.GetComponent<Rigidbody>().velocity = Vector3.zero;
        GameManager.Instance.players[i].gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        
        yield return new WaitForSeconds(0.5f);
        target.m_Targets[a].target = GameManager.Instance.players[i].gameObject.transform;
        GameManager.Instance.players[i].Ragdoll2Normal();
    }
}
