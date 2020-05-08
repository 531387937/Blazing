using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<string, Event_CallBack> eventDictionary = new Dictionary<string, Event_CallBack>();

    public void AddListener(string name, Event_CallBack listener)
    {
        Event_CallBack callback;
        if(eventDictionary.TryGetValue(name,out callback))
        {
            callback += listener;
        }
        else
        {
            eventDictionary.Add(name, listener);
        }
    }
    public void RemoveListener(string name,Event_CallBack listener)
    {
        if (eventDictionary.ContainsKey(name))
        {
            eventDictionary[name] -= listener;
            if(eventDictionary[name]==null)
            {
                eventDictionary.Remove(name);
            }
        }
    }
    public void TriggerEvent(string name,params object[] arg)
    {
        if (eventDictionary.ContainsKey(name))
        {
            eventDictionary[name](arg);
        }
    }
}

public delegate void Event_CallBack(params object[] arg);