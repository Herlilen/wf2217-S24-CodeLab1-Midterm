using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatlingAudio : MonoBehaviour
{
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void gatlingFireAudio()
    {
        _audioSource.PlayOneShot(_audioClip);
    }
}
