using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class Title : MonoBehaviour
{
    public Button start;
    public Button help;
    public Button about;
    public DOTweenAnimation doAnim;
    // Start is called before the first frame update
    void Start()
    {
        start.onClick.AddListener(StartButton);
        doAnim.onComplete.AddListener(() =>
        {
            EventManager.Instance.TriggerEvent("EnterGame");
            gameObject.SetActive(false);
        });
        AddEffect(start.gameObject);
        AddEffect(help.gameObject);
        AddEffect(about.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void StartButton()
    {start.interactable = false;
        //EventManager.Instance.TriggerEvent("EnterGame");
        //gameObject.SetActive(false);
        doAnim.DOPlay();
        
    }
    private void AddEffect(GameObject obj)
    {
        EventTriggerListener.Get(obj).onEnter += (go) =>
        {
            obj.transform.DOScale(1.1f, 0.2f);
        };
        EventTriggerListener.Get(obj).onExit += (go) =>
        {
            obj.transform.DOScale(1, 0.2f);
        };
        EventTriggerListener.Get(obj).onDown += (go) =>
        {
            obj.transform.DOScale(1, 0.2f);
        };
    }
}
