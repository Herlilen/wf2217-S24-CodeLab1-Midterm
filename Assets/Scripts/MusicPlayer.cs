using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource _audioSource;
    public AudioClip _audioClip;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void hitAlarmAudio()
    {
        _audioSource.PlayOneShot(_audioClip);
    }
}
