using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BeamBullet : MonoBehaviour
{
    private Rigidbody _rigidbody;

    private float destryCountDown;

    private bool damageDealt;
    
    [SerializeField] private GameObject hitParticleBeam;
    
    private void Awake()
    {
        damageDealt = false;
        _rigidbody = GetComponent<Rigidbody>();
        destryCountDown = 3f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody.velocity = transform.up * playerController.instance.beamSpeed;
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
            Instantiate(hitParticleBeam,
                new Vector3(collisionPoint.x, collisionPoint.y, collisionPoint.z),
                quaternion.identity);
        
            //deal damage
            if (!damageDealt)
            {
                BuildingHealth _buildingHealth = other.GetComponent<BuildingHealth>();
                EnemyController _enemyController = other.GetComponent<EnemyController>();
                if (_buildingHealth != null)
                {
                    _buildingHealth.buildingHealth -= playerController.instance.beamDamage;

                    damageDealt = true;
                }

                if (_enemyController != null)
                {
                    _enemyController.enemyHealth -= playerController.instance.beamDamage;

                    damageDealt = true;
                }
            }
        
            Destroy(gameObject);
        }
    }
}