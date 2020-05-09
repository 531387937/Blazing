using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager :MonoBehaviour
{
    private List<AudioSource> audioPlayers = new List<AudioSource>();
    private Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip>();
    public Queue<AudioSource> freePlayers = new Queue<AudioSource>();

    private string path = "Audio/音效/";
    private void Awake()
    {
        PlaySound("bgm",true,0.05f);
    }
    private void Update()
    {
        for(int i = 0;i<audioPlayers.Count;i++)
        {
            if(!audioPlayers[i].isPlaying)
            {
                freePlayers.Enqueue(audioPlayers[i]);
                audioPlayers.RemoveAt(i);
            }
        }
    }

    public void PlaySound(string name,bool loop = false,float volum = 1)
    {
        if(freePlayers.Count<1)
        {
            GameObject player = new GameObject();
            player.transform.SetParent(this.transform);
            freePlayers.Enqueue( player.AddComponent<AudioSource>());
            
        }
        AudioSource audio = freePlayers.Dequeue();
        audio.playOnAwake = false;
        AudioClip clip;
        if(!audios.TryGetValue(name,out clip))
        {
            clip = Resources.Load<AudioClip>(path + name);
            audios.Add(name, clip);
        }
        audio.clip = clip;
        audio.loop = loop;
        audioPlayers.Add(audio);
        audio.volume = volum;
        audio.Play();
    }
}
