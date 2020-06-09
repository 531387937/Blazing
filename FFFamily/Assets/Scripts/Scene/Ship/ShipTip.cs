using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShipTip : MonoBehaviour
{
    bool visuable = false;
    private GameObject map;
    public GameObject shipUI;
    private RectTransform rectTr;
    public GameObject[] dir;
    int state = 1;
    // Start is called before the first frame update
    void Start()
    {
        map = GameObject.Find("map");
        rectTr = shipUI.GetComponent<RectTransform>();
    }
    private void OnEnable()
    {
        EventManager.Instance.AddListener("GameStart", OnGameStart);
    }
    // Update is called once per frame
    void Update()
    {
        if(!IsInView(transform.position))
        {
            if(visuable)
            {
                visuable = false;
                shipUI.SetActive(true);
            }
        }
        else
        {
            if(!visuable)
            {
                visuable = true;
                shipUI.SetActive(false);
            }
        }
        if (!visuable)
        {
            CaculateUIPos();
        }
    }


    void CaculateUIPos()
    {
        float x = (transform.root.position.x - 53) / 55;
        float y = (transform.root.position.z - 60) / 70;
        if(state!=1&&transform.root.position.x>=74)
        {
            state = 1;
            dir[0].SetActive(true);
            dir[3].SetActive(false);
        }
        if (state != 2 && transform.root.position.z >= 83f)
        {
            state = 2;
            dir[1].SetActive(true);
            dir[0].SetActive(false);
        }
        if (state != 3 && transform.root.position.x <= 33)
        {
            state = 3;
            dir[2].SetActive(true);
            dir[1].SetActive(false);
        }
        if (state != 4 && transform.root.position.z <= 36f)
        {
            state = 4;
            dir[3].SetActive(true);
            dir[2].SetActive(false);
        }
        rectTr.anchoredPosition = new Vector3(x * 1920, y * 1080);
    }

    public bool IsInView(Vector3 worldPos)
    {
        Transform camTransform = Camera.main.transform;
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos);
        Vector3 dir = (worldPos - camTransform.position).normalized;
        float dot = Vector3.Dot(camTransform.forward, dir);     //判断物体是否在相机前面


        if (dot > 0 && viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
            return false;
    }
    void OnGameStart(params object[] arg)
    {
        visuable = true;
    }
}
