using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class GatlingBullet : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private float destryCountDown;
    [SerializeField] private GameObject hitParticleGatlin;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        destryCountDown = 3f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody.velocity = transform.forward * playerController.instance.gatlingSpeed;
    }

    private void Update()
    {
        if (destryCountDown > 0)
        {
            destryCountDown -= Time.deltaTime;
        }

        if (destryCountDown <= 0)
        {
            Destroy(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var collisionPoint = other.ClosestPoint(transform.position);
        Instantiate(hitParticleGatlin,
            new Vector3(collisionPoint.x, collisionPoint.y, collisionPoint.z),
            quaternion.identity);
        Destroy(this);
        Debug.Log("hit" + other.gameObject.name);
    }
}
