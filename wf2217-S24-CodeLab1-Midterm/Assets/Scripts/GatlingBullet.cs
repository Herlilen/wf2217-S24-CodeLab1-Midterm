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
    private bool damageDealt;
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
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Attackable")
        {
            //vfx and destroy self
            var collisionPoint = other.ClosestPoint(transform.position);
            Instantiate(hitParticleGatlin,
                new Vector3(collisionPoint.x, collisionPoint.y, collisionPoint.z),
                quaternion.identity);
        
            //deal damage
            if (!damageDealt)
            {
                BuildingHealth _buildingHealth = other.GetComponent<BuildingHealth>();
                EnemyController _enemyController = other.GetComponent<EnemyController>();
                if (_buildingHealth != null)
                {
                    _buildingHealth.buildingHealth -= playerController.instance.gatlingDamage;

                    damageDealt = true;
                }

                if (_enemyController != null)
                {
                    _enemyController.enemyHealth -= playerController.instance.gatlingDamage;
                    damageDealt = true;
                }
            }
        
            Destroy(gameObject);
        }
    }
}
