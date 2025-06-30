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

    public void PlayAudioClip(AudioClip clip, float minPitch = 1f, float maxPitch = 1f)
    {
        sourceSFX.pitch = Random.Range(minPitch, maxPitch);
        sourceSFX.PlayOneShot(clip);
    }

    public void PlayBackgroundMusic()
    {
        sourceBg.Play();
    }

    public void SetMusicVolume(float vol)
    {
        sourceBg.volume = vol;
    }
}
