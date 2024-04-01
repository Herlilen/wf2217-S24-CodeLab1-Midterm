using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class playerController : MonoBehaviour
{
    public static playerController instance;
    
    
    

    [Header("Player Status")] 
    public float health;
    
    [Header("Camera")] 
    [SerializeField] private Camera _camera;
    [SerializeField] private float camSpeed;
    private float yRotation;
    private float xRotation;

    [Header("Movement")] 
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private float antiGravityBoosterForceAmount;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float forwardForceAmount;
    [SerializeField] private float rightForceAmount;
    [SerializeField] private float backwardForceAmount;
    [SerializeField] private float ascendForceAmount;
    [SerializeField] private float descendForceAmount;
    private float yawRotateSpeed;
    [SerializeField] float yawRotateSpeedMax;
    private float pitchRotateSpeed;
    [SerializeField] float pitchRotateSpeedMax;
    private float rollRotateSpeed;
    [SerializeField] float rollRotateSpeedMax;

    [Header("Attack")] 
    [SerializeField] private GameObject gatlingBullet;
    [SerializeField] private GameObject gatLingL;
    [SerializeField] private GameObject gatLingR;
    private bool gatlingIsFireing = false;
    [SerializeField] private float gatlingFireRate = 10f;
    float gatlingFireTimer = 0f;
    public float gatlingDamage = 1;
    public float gatlingSpeed = 10f;
    [SerializeField] private GameObject beamBullet;
    [SerializeField] private GameObject beamHolder;
    private float beamFireCoolDown = 0f;
    [SerializeField] private float beamFireCoolDownMax = 5f;
    public float beamDamage = 100f;
    public float beamSpeed = 10f;
    
    [Header("Audio")] 
    [SerializeField] private GameObject enginePlayer;
    [SerializeField] private GatlingAudio gatlingHolder;
    [SerializeField] private GameObject musicPlayer;
    [SerializeField] private MusicPlayer _music;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        health = 100;



    }

    // Update is called once per frame
    void Update()
    {
        if (health > 0)
        {
            CameraControl();
            AudioPlayControl();
            if (GameManager.instance.controlReady)
            {
                Shoot();
            }
        }

    }

    private void FixedUpdate()
    {
        if (health > 0)
        {
            if (GameManager.instance.controlReady)
            {
                _rigidbody.isKinematic = false;
                Movement();
            }
            else
            {
                _rigidbody.isKinematic = true;
            }
        }
    }

    public void HitAudio()
    {
        musicPlayer.GetComponent<AudioSource>().PlayOneShot(_music._audioClip);
    }
    
    void Shoot()
    {
        // shott when key hold
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            gatlingIsFireing = true;
        }
        
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            gatlingIsFireing = false;
            gatlingFireTimer = 0f; //reset fire rate cool down
        }

        if (gatlingIsFireing && gatlingFireTimer <= 0f)
        {
            Instantiate(gatlingBullet, gatLingL.transform.position, gatLingL.transform.rotation);
            Instantiate(gatlingBullet, gatLingR.transform.position, gatLingR.transform.rotation);
            //set timer
            gatlingFireTimer = 1f / gatlingFireRate;
            //play audio
            gatlingHolder.gatlingFireAudio();
        }

        if (gatlingFireTimer > 0)
        {
            gatlingFireTimer -= Time.deltaTime;
        }

        if (gatlingFireTimer <= 0)
        {
            gatlingFireTimer = 0;
        }
        
        //shoot beam
        if (Input.GetKeyDown(KeyCode.Mouse1) && beamFireCoolDown == 0)
        {
            Instantiate(beamBullet, beamHolder.transform.position, beamHolder.transform.rotation);
            beamFireCoolDown = beamFireCoolDownMax;
            beamHolder.GetComponent<BeamAudio>().BeamFireAudio();
        }

        if (beamFireCoolDown > 0)
        {
            beamFireCoolDown -= Time.deltaTime;
        }

        if (beamFireCoolDown <= 0)
        {
            beamFireCoolDown = 0;
        }
    }
    
    void AudioPlayControl()
    {
        if (GameManager.instance.controlReady)
        {
            enginePlayer.GetComponent<AudioSource>().enabled = true;
        }
        else
        {
            enginePlayer.GetComponent<AudioSource>().enabled = false;

        }
    }
    
    void CameraControl()
    {
        //get mouse with input system
        float mouseX = Input.GetAxis("Mouse X") * camSpeed * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * camSpeed * Time.deltaTime;
        
        //set y rotation to mouse x
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -70f, 70f);
        
        //set x ration according to mouse Y
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70f, 50f);
        
        //rotate camera
        _camera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    void Movement()
    {
        _rigidbody.AddForce(Vector3.up * antiGravityBoosterForceAmount);
        
        //WASD local level strafe
        if (Input.GetKey(KeyCode.W))
        {
            _rigidbody.AddForce(transform.forward * forwardForceAmount);
        }

        if (Input.GetKey(KeyCode.A))
        {
            _rigidbody.AddForce(-transform.right * rightForceAmount);
        }

        if (Input.GetKey(KeyCode.D))
        {
            _rigidbody.AddForce(transform.right * rightForceAmount);
        }

        if (Input.GetKey(KeyCode.S))
        {
            _rigidbody.AddForce(-transform.forward * backwardForceAmount);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            _rigidbody.AddForce(transform.up * ascendForceAmount);
        }
        
        if (Input.GetKey(KeyCode.LeftControl))
        {
            _rigidbody.AddForce(-transform.up * descendForceAmount);
        }

        if (_rigidbody.velocity.magnitude > maxSpeed)
        {
            _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
        }
        _rigidbody.velocity *= 0.99f;
        
        //RF a-axis turning
        Vector3 pitchRotationAmount = new Vector3(pitchRotateSpeed, 0, 0);
        transform.localRotation *= quaternion.Euler(pitchRotationAmount * Time.deltaTime);
        if (Input.GetKey(KeyCode.R))
        {
            if (pitchRotateSpeed < pitchRotateSpeedMax)
            {
                pitchRotateSpeed += 20 * Time.deltaTime;
            }

            if (pitchRotateSpeed >= pitchRotateSpeedMax)
            {
                pitchRotateSpeed = pitchRotateSpeedMax;
            }
        }

        if (Input.GetKey(KeyCode.F))
        {
            if (pitchRotateSpeed > -pitchRotateSpeedMax)
            {
                pitchRotateSpeed -= 20 * Time.deltaTime;
            }

            if (pitchRotateSpeed <= -pitchRotateSpeedMax)
            {
                pitchRotateSpeed = -pitchRotateSpeedMax;
            }
        }

        if (!Input.GetKey(KeyCode.R) && !Input.GetKey(KeyCode.F))
        {
            pitchRotateSpeed *= 0.9f;
        }
        
        //QE Y-axis turning
        Vector3 yawRotationAmount = new Vector3(0, yawRotateSpeed, 0);
        transform.localRotation *= quaternion.Euler(yawRotationAmount * Time.deltaTime);
        if (Input.GetKey(KeyCode.E))
        {
            if (yawRotateSpeed < yawRotateSpeedMax)
            {
                yawRotateSpeed += 20 * Time.deltaTime;
            }

            if (yawRotateSpeed >= yawRotateSpeedMax)
            {
                yawRotateSpeed = yawRotateSpeedMax;
            }
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (yawRotateSpeed > -yawRotateSpeedMax)
            {
                yawRotateSpeed -= 20 * Time.deltaTime;
            }

            if (yawRotateSpeed <= -yawRotateSpeedMax)
            {
                yawRotateSpeed = -yawRotateSpeedMax;
            }
        }

        if (!Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E))
        {
            yawRotateSpeed *= 0.9f;
        }
        
        //ZX roll turning
        Vector3 rollRotationAmount = new Vector3(0, 0, rollRotateSpeed);
        transform.localRotation *= quaternion.Euler(rollRotationAmount * Time.deltaTime);
        if (Input.GetKey(KeyCode.Z))
        {
            if (rollRotateSpeed < rollRotateSpeedMax)
            {
                rollRotateSpeed += 20 * Time.deltaTime;
            }

            if (rollRotateSpeed >= rollRotateSpeedMax)
            {
                rollRotateSpeed = rollRotateSpeedMax;
            }
        }

        if (Input.GetKey(KeyCode.C))
        {
            if (rollRotateSpeed > -rollRotateSpeedMax)
            {
                rollRotateSpeed -= 20 * Time.deltaTime;
            }

            if (rollRotateSpeed <= -rollRotateSpeedMax)
            {
                rollRotateSpeed = -rollRotateSpeedMax;
            }
        }

        if (!Input.GetKey(KeyCode.Z) && !Input.GetKey(KeyCode.C))
        {
            rollRotateSpeed *= 0.9f;
        }
    }
}
