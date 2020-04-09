using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager :MonoBehaviour
{
    private List<AudioSource> AudioPlayers = new List<AudioSource>();
    public Queue<AudioSource> freePlayers = new Queue<AudioSource>();
    private string path = "Audio/音效/";
    private void Awake()
    {
        PlaySound("bgm",true);
    }
    private void Update()
    {
        for(int i = 0;i<AudioPlayers.Count;i++)
        {
            if(!AudioPlayers[i].isPlaying)
            {
                freePlayers.Enqueue(AudioPlayers[i]);
                AudioPlayers.RemoveAt(i);
            }
        }
    }
    public void PlayHit()
    {

    }

    public void DropInWater()
    {
        
    }

    public void PlaySound(string name,bool loop = false)
    {
        if(freePlayers.Count<1)
        {
            GameObject player = new GameObject();
            player.transform.SetParent(this.transform);
            freePlayers.Enqueue( player.AddComponent<AudioSource>());
            
        }
        AudioSource audio = freePlayers.Dequeue();
        audio.playOnAwake = false;
        audio.clip =  Resources.Load<AudioClip>(path+name);
        audio.loop = loop;
        AudioPlayers.Add(audio);
        audio.Play();
    }
}
