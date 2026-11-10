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
    [Tooltip("Degrees per mouse-axis unit; independent of frame rate.")]
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
    [Tooltip("The two reticle rings share the center of the camera view as their aim point.")]
    [FormerlySerializedAs("gatlingAimDistance")]
    [SerializeField, Min(1f)] private float weaponAimDistance = 1000f;
    [FormerlySerializedAs("gatlingAimLayers")]
    [SerializeField] private LayerMask weaponAimLayers = Physics.DefaultRaycastLayers;
    private RaycastHit[] weaponAimHits = new RaycastHit[32];
    private bool gatlingUsesGravity;
    private Vector3 gatlingLeftVelocity;
    private Vector3 gatlingRightVelocity;
    [SerializeField] private GameObject beamBullet;
    [SerializeField] private GameObject beamHolder;
    private float beamFireCoolDown = 0f;
    [SerializeField] private float beamFireCoolDownMax = 5f;
    public float beamDamage = 100f;
    public float beamSpeed = 10f;
    private Vector3 beamVelocity;
    private float beamTargetDistance;

    [Header("Weapon Pool Prewarm")]
    [SerializeField, Min(0)] private int gatlingPoolSize = 128;
    [SerializeField, Min(0)] private int beamPoolSize = 4;
    [SerializeField, Min(0)] private int gatlingImpactPoolSize = 96;
    [SerializeField, Min(0)] private int beamImpactPoolSize = 4;
    private WeaponPool weaponPool;
    
    [Header("Audio")] 
    [SerializeField] private GameObject enginePlayer;
    [SerializeField] private GatlingAudio gatlingHolder;
    [SerializeField] private GameObject musicPlayer;
    [SerializeField] private MusicPlayer _music;
    
    private void Awake()
    {
        Rigidbody projectileBody = gatlingBullet != null ? gatlingBullet.GetComponent<Rigidbody>() : null;
        gatlingUsesGravity = projectileBody != null && projectileBody.useGravity;

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
        if (instance == this)
        {
            weaponPool = new WeaponPool();
            weaponPool.Prewarm(gatlingBullet, gatlingPoolSize);
            weaponPool.Prewarm(beamBullet, beamPoolSize);
            weaponPool.Prewarm(gatlingBullet.GetComponent<GatlingBullet>().HitEffectPrefab, gatlingImpactPoolSize, true);
            weaponPool.Prewarm(beamBullet.GetComponent<BeamBullet>().HitEffectPrefab, beamImpactPoolSize, true);
        }
    }

    private void OnDestroy()
    {
        weaponPool?.Dispose();
        if (instance == this) instance = null;
    }

    public void PlayWeaponImpact(GameObject prefab, Vector3 position)
    {
        weaponPool?.PlayImpact(prefab, position);
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
                UpdateWeaponAim();
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
                if (_rigidbody.isKinematic)
                {
                    _rigidbody.isKinematic = false;
                    _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                }
                Movement();
            }
            else
            {
                // Startup positions the cockpit directly before physics takes over.
                _rigidbody.interpolation = RigidbodyInterpolation.None;
                _rigidbody.isKinematic = true;
            }
        }
    }

    public void HitAudio()
    {
        musicPlayer.GetComponent<AudioSource>().PlayOneShot(_music._audioClip);
    }

    public void ResetDevelopmentPose(Vector3 position, Quaternion rotation)
    {
        weaponPool?.ReturnAll();
        if (!_rigidbody.isKinematic)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
        _rigidbody.isKinematic = true;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        transform.SetPositionAndRotation(position, rotation);
        _camera.transform.localRotation = Quaternion.identity;
        xRotation = yRotation = 0f;
        pitchRotateSpeed = yawRotateSpeed = rollRotateSpeed = 0f;
        gatlingIsFireing = false;
        gatlingFireTimer = beamFireCoolDown = 0f;
        health = 100f;
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
            FireGatling(gatLingL.transform, gatlingLeftVelocity);
            FireGatling(gatLingR.transform, gatlingRightVelocity);
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
            GameObject shot = weaponPool.Rent(beamBullet, beamHolder.transform.position, beamHolder.transform.rotation);
            shot.GetComponent<BeamBullet>().Initialize(beamVelocity, beamDamage, transform, beamTargetDistance);
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

    void UpdateWeaponAim()
    {
        // This is the same view-center ray used by the sphere's reticle shader.
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        float range = Mathf.Min(Mathf.Max(1f, weaponAimDistance), _camera.farClipPlane);
        int count;
        // Raycast results are unordered. Grow only if the buffer is full so an
        // omitted foreground object can never make us aim through a nearer wall.
        while (true)
        {
            count = Physics.RaycastNonAlloc(ray, weaponAimHits, range, weaponAimLayers,
                QueryTriggerInteraction.Ignore);
            if (count < weaponAimHits.Length) break;
            Array.Resize(ref weaponAimHits, weaponAimHits.Length * 2);
        }
        Vector3 target = ray.GetPoint(range);
        float nearest = range;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = weaponAimHits[i];
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform)) continue;
            if (hit.distance >= nearest) continue;
            nearest = hit.distance;
            target = hit.point;
        }

        Vector3 gravity = gatlingUsesGravity ? Physics.gravity : Vector3.zero;
        gatlingLeftVelocity = AimGatlingMuzzle(gatLingL.transform, target, gravity);
        gatlingRightVelocity = AimGatlingMuzzle(gatLingR.transform, target, gravity);

        Transform beamMuzzle = beamHolder.transform;
        Vector3 beamDirection = target - beamMuzzle.position;
        beamTargetDistance = beamDirection.magnitude;
        beamVelocity = beamDirection.normalized * beamSpeed;
        if (beamDirection.sqrMagnitude > 0.000001f)
        {
            // The beam model and its launch axis use local +Y, unlike the gatling's +Z.
            beamMuzzle.rotation = Quaternion.LookRotation(beamDirection, _camera.transform.up)
                * Quaternion.Euler(90f, 0f, 0f);
        }
    }

    Vector3 AimGatlingMuzzle(Transform muzzle, Vector3 target, Vector3 gravity)
    {
        Vector3 velocity = GatlingBullet.CalculateLaunchVelocity(muzzle.position, target,
            gatlingSpeed, gravity, Time.fixedDeltaTime);
        if (velocity.sqrMagnitude > 0.000001f)
            muzzle.rotation = Quaternion.LookRotation(velocity, _camera.transform.up);
        return velocity;
    }

    void FireGatling(Transform muzzle, Vector3 velocity)
    {
        GameObject shot = weaponPool.Rent(gatlingBullet, muzzle.position, muzzle.rotation);
        shot.GetComponent<GatlingBullet>().Initialize(velocity, gatlingDamage, transform);
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
        // Mouse axes already report displacement for this frame, not a speed.
        float mouseX = Input.GetAxis("Mouse X") * camSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * camSpeed;

        ApplyCameraRotation(mouseX, mouseY);
    }

    void ApplyCameraRotation(float mouseX, float mouseY)
    {
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -70f, 70f);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -70f, 50f);

        _camera.transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0);
    }

    void Movement()
    {
        // Preserve the original order: forces, current velocity, then rotation.
        ApplyTranslationForces();
        LimitAndDampVelocity();
        ApplyBodyRotation();
    }

    void ApplyTranslationForces()
    {
        _rigidbody.AddForce(Vector3.up * antiGravityBoosterForceAmount);

        // Opposing keys are independent because their thrust amounts can differ.
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
    }

    void LimitAndDampVelocity()
    {
        if (_rigidbody.linearVelocity.magnitude > maxSpeed)
        {
            _rigidbody.linearVelocity = Vector3.ClampMagnitude(_rigidbody.linearVelocity, maxSpeed);
        }
        _rigidbody.linearVelocity *= 0.99f;
    }

    void ApplyBodyRotation()
    {
        // Keep local pitch/yaw/roll order, then let physics interpolate the result.
        Quaternion rotation = _rigidbody.rotation;
        ApplyRotationAxis(ref rotation, new Vector3(pitchRotateSpeed, 0, 0), ref pitchRotateSpeed,
            pitchRotateSpeedMax, KeyCode.R, KeyCode.F);
        ApplyRotationAxis(ref rotation, new Vector3(0, yawRotateSpeed, 0), ref yawRotateSpeed,
            yawRotateSpeedMax, KeyCode.E, KeyCode.Q);
        ApplyRotationAxis(ref rotation, new Vector3(0, 0, rollRotateSpeed), ref rollRotateSpeed,
            rollRotateSpeedMax, KeyCode.Z, KeyCode.C);
        _rigidbody.MoveRotation(rotation);
    }

    void ApplyRotationAxis(ref Quaternion rotation, Vector3 rotationAmount, ref float speed, float speedLimit,
        KeyCode positiveKey, KeyCode negativeKey)
    {
        // Mathematics quaternion.Euler takes radians. Apply the previous speed first.
        rotation *= (Quaternion)quaternion.Euler(rotationAmount * Time.deltaTime);

        // Legacy key state is stable within this physics tick.
        bool positivePressed = Input.GetKey(positiveKey);
        bool negativePressed = Input.GetKey(negativeKey);

        // Do not combine these branches: positive then negative preserves both-key behavior.
        if (positivePressed)
        {
            if (speed < speedLimit)
            {
                speed += 20 * Time.deltaTime;
            }

            if (speed >= speedLimit)
            {
                speed = speedLimit;
            }
        }

        if (negativePressed)
        {
            if (speed > -speedLimit)
            {
                speed -= 20 * Time.deltaTime;
            }

            if (speed <= -speedLimit)
            {
                speed = -speedLimit;
            }
        }

        if (!positivePressed && !negativePressed)
        {
            speed *= 0.9f;
        }
    }
}
