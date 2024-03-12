using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingHealth : MonoBehaviour
{
    private Collider _collider;
    public bool blocksBroken;
    private AudioSource _audioSource;
    [SerializeField] private AudioClip broke;
    public Material[] buildingMat;
    private bool playonce;
    public float buildingHealth;

    private void Start()
    {
        playonce = false;
        blocksBroken = false;
        _collider = gameObject.GetComponent<BoxCollider>();
        _audioSource = gameObject.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (buildingHealth <= 0)
        {
            //break the bricks
            blocksBroken = true;
            _collider.enabled = false;
            //play audio once
            if (!playonce)
            {
                _audioSource.PlayOneShot(broke);
                playonce = true;
            }
        }
    }
}
