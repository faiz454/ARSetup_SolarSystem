using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManger : MonoBehaviour
{
    public AudioSource Solaraudio;
    public AudioClip[] Solarclip = new AudioClip[8];
    public int Currentaudio = 0;
    // Start is called before the first frame update
    void Start()
    {
        Solaraudio.clip = Solarclip[Currentaudio];
        Solaraudio.Play();
        Currentaudio++;

    }

    // Update is called once per frame
    void Update()
    {
        if (!Solaraudio.isPlaying && Currentaudio < Solarclip.Length)
        {
            Solaraudio.clip = Solarclip[Currentaudio];
            Solaraudio.Play();
            Currentaudio++;
        }
    }
}
