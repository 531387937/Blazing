using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : Singleton<EventManager>
{
    private Dictionary<string,List<Event_CallBack>> eventDictionary = new Dictionary<string, List<Event_CallBack>>();

    public void AddListener(string name, Event_CallBack listener)
    {
        List<Event_CallBack> callback;
        if(eventDictionary.TryGetValue(name,out callback))
        {
            callback.Add(listener);
        }
        else
        {
            callback = new List<Event_CallBack>();
            callback.Add(listener);
            eventDictionary.Add(name, callback);
        }
    }
    public void RemoveListener(string name,Event_CallBack listener)
    {
        if (eventDictionary.ContainsKey(name))
        {
            if(eventDictionary[name].Contains(listener))
            {
                eventDictionary[name].Remove(listener);
            }
            if(eventDictionary[name].Count==0)
            {
                eventDictionary.Remove(name);
            }
        }
    }
    public void TriggerEvent(string name,params object[] arg)
    {
        if (eventDictionary.ContainsKey(name))
        {
            for (int i = 0;i < eventDictionary[name].Count;i++)
            {
                eventDictionary[name][i](arg);
            }
            //eventDictionary[name](arg);
        }
    }
}

public delegate void Event_CallBack(params object[] arg);