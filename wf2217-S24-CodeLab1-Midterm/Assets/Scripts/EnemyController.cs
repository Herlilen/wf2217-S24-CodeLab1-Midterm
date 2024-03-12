using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float enemyHealth;
    [SerializeField] private float rotationSpeed = 5 ;
    [SerializeField] private GameObject fireHolder;
    [SerializeField] private GameObject bullet;
    private float shootCoolDown;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //always face to player
        if (playerController.instance != null)
        {
            Vector3 directionToPlayer = playerController.instance.gameObject.transform.position - transform.position;
            
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            
            //slow the rotation speed
            float slowSpeed = rotationSpeed * Time.deltaTime;
            
            //rotate the enemy
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, slowSpeed);
        }
        
        //fire every two seconds
        if (shootCoolDown > 0)
        {
            shootCoolDown -= Time.deltaTime;
        }

        if (shootCoolDown <= 0)
        {
            shootCoolDown = 0;
        }

        if (shootCoolDown == 0 && GameManager.instance.controlReady) 
        {
            Instantiate(bullet, fireHolder.transform.position, fireHolder.transform.rotation);
            shootCoolDown = 2;
        }

        if (enemyHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
