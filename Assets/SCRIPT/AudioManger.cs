using UnityEngine;

public class AudioManger : MonoBehaviour
{
    public AudioSource Solaraudio;
    public AudioClip[] Solarclip = new AudioClip[8];

    // Plays the exact planet clip when called
    public void PlayPlanetAudio(int index)
    {
        if (Solaraudio != null && index >= 0 && index < Solarclip.Length)
        {
            Solaraudio.Stop();
            Solaraudio.clip = Solarclip[index];
            Solaraudio.Play();
        }
    }

    // Checks if the audio is still talking
    public bool IsPlaying()
    {
        return Solaraudio != null && Solaraudio.isPlaying;
    }
}