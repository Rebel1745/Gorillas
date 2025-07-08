using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] AudioSource sourceSFX; // sound effects
    [SerializeField] AudioSource sourceBg; // background music
    public float MinimumMovementSoundPitch = 1f;
    public float MaximumMovementSoundPitch = 1f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        sourceBg.volume = PlayerPrefs.GetFloat("MusicVolume", 1);
        sourceSFX.volume = PlayerPrefs.GetFloat("sfxVolume", 1);
    }

    public void PlayAudioClip(AudioClip clip, float minPitch = 1f, float maxPitch = 1f)
    {
        sourceSFX.pitch = Random.Range(minPitch, maxPitch);
        sourceSFX.PlayOneShot(clip);
    }

    public void PlayBackgroundMusic(AudioClip clip)
    {
        sourceBg.PlayOneShot(clip);
    }

    public void StopBackgroundMusic()
    {
        sourceBg.Stop();
    }

    public void SetMusicVolume(float vol)
    {
        sourceBg.volume = vol;
        PlayerPrefs.SetFloat("MusicVolume", vol);
    }

    public void SetSFXVolume(float vol)
    {
        sourceSFX.volume = vol;
        PlayerPrefs.SetFloat("sfxVolume", vol);
    }
}
