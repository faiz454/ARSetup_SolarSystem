using UnityEngine;

public class AudioManger : MonoBehaviour
{
    public AudioSource Solaraudio;

    [Header("Main Solar System Audio Clips")]
    public AudioClip[] Solarclip = new AudioClip[9]; // Mercury (0) to Neptune (7), Moon (8)

    [Header("Artificial Satellite Audio Clips")]
    public AudioClip[] Satelliteclip; // Assign 2 to 3 satellite clips here

    private int currentSatelliteIndex = 0;

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

    // Plays the selected satellite audio track
    public void PlaySatelliteAudio(int index)
    {
        if (Solaraudio != null && Satelliteclip != null && Satelliteclip.Length > 0)
        {
            currentSatelliteIndex = Mathf.Clamp(index, 0, Satelliteclip.Length - 1);
            Solaraudio.Stop();
            Solaraudio.clip = Satelliteclip[currentSatelliteIndex];
            Solaraudio.Play();
        }
    }

    // Call this from a UI Button OnClick event to skip to the next satellite audio track
    public void SkipToNextSatelliteAudio()
    {
        if (Satelliteclip == null || Satelliteclip.Length == 0) return;

        currentSatelliteIndex = (currentSatelliteIndex + 1) % Satelliteclip.Length;
        PlaySatelliteAudio(currentSatelliteIndex);
    }

    // Checks if the audio is still playing
    public bool IsPlaying()
    {
        return Solaraudio != null && Solaraudio.isPlaying;
    }

    public void StopAudio()
    {
        if (Solaraudio != null) Solaraudio.Stop();
    }
}