using System;
using UnityEngine;

public class BeamBullet : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private float destryCountDown;
    private bool damageDealt;
    private bool initialized;
    private float damage;
    private Transform owner;
    private Vector3 previousPhysicsPosition;
    private static RaycastHit[] pathHits = new RaycastHit[16];
    
    [SerializeField] private GameObject hitParticleBeam;
    public GameObject HitEffectPrefab => hitParticleBeam;
    private PooledWeaponObject pooledObject;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        pooledObject = GetComponent<PooledWeaponObject>();
        destryCountDown = 3f;
        previousPhysicsPosition = _rigidbody.position;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (initialized) return;
        playerController player = playerController.instance;
        Initialize(transform.up * (player != null ? player.beamSpeed : 300f),
            player != null ? player.beamDamage : 100f, player != null ? player.transform : null);
    }

    public void Initialize(Vector3 velocity, float shotDamage, Transform shotOwner, float targetDistance = 0f)
    {
        if (pooledObject == null) pooledObject = GetComponent<PooledWeaponObject>();
        initialized = true;
        damageDealt = false;
        damage = shotDamage;
        owner = shotOwner;
        previousPhysicsPosition = _rigidbody.position;
        _rigidbody.linearVelocity = velocity;
        _rigidbody.angularVelocity = Vector3.zero;
        // At 300 m/s the original three-second lifetime ends before a 1000 m target.
        // Allow this shot to reach the selected aim point before it expires.
        destryCountDown = Mathf.Max(3f, targetDistance / Mathf.Max(velocity.magnitude, 0.01f) + 0.1f);
    }

    private void OnDisable()
    {
        initialized = false;
        damageDealt = false;
        damage = 0f;
        owner = null;
        destryCountDown = 0f;
        previousPhysicsPosition = Vector3.zero;
    }

    private void Recycle()
    {
        if (pooledObject != null) pooledObject.ReturnToPool();
        else Destroy(gameObject); // Standalone scene copies are not owned by a pool.
    }

    private void FixedUpdate()
    {
        CheckTraveledPath();
    }

    private void CheckTraveledPath()
    {
        if (!initialized || damageDealt) return;
        Vector3 position = _rigidbody.position;
        Vector3 travel = position - previousPhysicsPosition;
        previousPhysicsPosition = position;
        float distance = travel.magnitude;
        if (distance < 0.00001f) return;

        // Resolve the first solid surface along the beam's actual path. This also
        // catches thin targets skipped between physics steps at high projectile speed.
        Ray ray = new Ray(position - travel, travel / distance);
        int count;
        while (true)
        {
            count = Physics.RaycastNonAlloc(ray, pathHits, distance, Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (count < pathHits.Length) break;
            Array.Resize(ref pathHits, pathHits.Length * 2);
        }
        float nearest = float.PositiveInfinity;
        RaycastHit impact = default;
        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = pathHits[i];
            if (Ignored(hit.collider) || hit.distance >= nearest) continue;
            nearest = hit.distance;
            impact = hit;
        }
        if (impact.collider != null) ApplyImpact(impact.collider, impact.point);
    }

    private void Update()
    {
        CheckTraveledPath();
        if (!initialized) return;
        if (destryCountDown > 0)
        {
            destryCountDown -= Time.deltaTime;
        }

        if (destryCountDown <= 0)
        {
            Recycle();
        }
    }

    private bool Ignored(Collider other)
    {
        return other == null || other.isTrigger ||
            (owner != null && other.transform.IsChildOf(owner));
    }

    private void ApplyImpact(Collider other, Vector3 point)
    {
        if (damageDealt) return;
        damageDealt = true;
        if (hitParticleBeam != null && playerController.instance != null)
            playerController.instance.PlayWeaponImpact(hitParticleBeam, point);
        BuildingHealth building = other.GetComponentInParent<BuildingHealth>();
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (building != null && building.CompareTag("Attackable") && !building.blocksBroken)
            building.buildingHealth -= damage;
        if (enemy != null && enemy.CompareTag("Attackable"))
            enemy.enemyHealth -= damage;
        Recycle();
    }
}
