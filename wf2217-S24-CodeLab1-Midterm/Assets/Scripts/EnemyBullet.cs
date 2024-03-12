using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float bulletDamage;
    private Rigidbody _rigidbody;
    private float destryCountDown;
    private bool damageDealt;
    [SerializeField] private GameObject hitParticleEnemy;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        destryCountDown = 10f;
    }

    // Start is called before the first frame update
    void Start()
    {
        _rigidbody.velocity = transform.forward * bulletSpeed;
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
        if (other.tag == "Player")
        {
            //vfx and destroy self
            var collisionPoint = other.ClosestPoint(transform.position);
            Instantiate(hitParticleEnemy,
                new Vector3(collisionPoint.x, collisionPoint.y, collisionPoint.z),
                quaternion.identity);
        
            //deal damage
            if (!damageDealt)
            {
                if (playerController.instance != null)
                {
                    playerController.instance.health -= bulletDamage;
                    
                    playerController.instance.HitAudio();

                    damageDealt = true;
                }
            }
            Destroy(gameObject);
        }

        if (other.tag == "PlayerAttack" || other.tag == "PlayerAttack1")
        {
            Destroy(gameObject);
        }
    }
}
