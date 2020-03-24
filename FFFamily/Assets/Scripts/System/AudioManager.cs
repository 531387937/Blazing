using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource bgm;
    public AudioSource dropInWater;
    public AudioSource[] hit;
    private void Start()
    {
    }

    public void PlayHit()
    {
        hit[Random.Range(0, hit.Length)].Play();
    }

    public void DropInWater()
    {
        dropInWater.Play();
    }
}
