using System;
using UnityEngine;

public class GatlingBullet : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private float destryCountDown;
    private bool damageDealt;
    private bool initialized;
    private float damage;
    private Transform owner;
    private Vector3 previousPhysicsPosition;
    private static RaycastHit[] pathHits = new RaycastHit[16];
    [SerializeField] private GameObject hitParticleGatlin;
    public GameObject HitEffectPrefab => hitParticleGatlin;
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
        Initialize(transform.forward * (player != null ? player.gatlingSpeed : 500f),
            player != null ? player.gatlingDamage : 1f, player != null ? player.transform : null);
    }

    public void Initialize(Vector3 velocity, float shotDamage, Transform shotOwner)
    {
        if (pooledObject == null) pooledObject = GetComponent<PooledWeaponObject>();
        initialized = true;
        damageDealt = false;
        destryCountDown = 3f;
        damage = shotDamage;
        owner = shotOwner;
        previousPhysicsPosition = _rigidbody.position;
        _rigidbody.linearVelocity = velocity;
        _rigidbody.angularVelocity = Vector3.zero;
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

    // Low-arc solution at the existing muzzle speed, including the current gravity.
    // The stable time-of-flight form avoids cancellation for fast, nearby shots.
    public static Vector3 CalculateLaunchVelocity(Vector3 origin, Vector3 target,
        float speed, Vector3 gravity, float fixedStep)
    {
        Vector3 offset = target - origin;
        speed = Mathf.Max(speed, 0.01f);
        if (offset.sqrMagnitude < 0.000001f) return Vector3.zero;
        if (gravity.sqrMagnitude < 0.000001f) return offset.normalized * speed;

        double b = (double)speed * speed + Vector3.Dot(offset, gravity);
        double discriminant = b * b - (double)gravity.sqrMagnitude * offset.sqrMagnitude;
        if (b <= 0.0 || discriminant < 0.0)
            return offset.normalized * speed; // Outside the reachable ballistic range.

        float flightTime = (float)Math.Sqrt(2.0 * offset.sqrMagnitude / (b + Math.Sqrt(discriminant)));
        Vector3 velocity = offset / flightTime - gravity * (0.5f * flightTime);
        // PhysX advances velocity before position; compensate that half-step bias.
        return velocity - gravity * (0.5f * Mathf.Max(fixedStep, 0f));
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

        // Trigger callbacks alone can miss thin targets at 500 m/s. Sweep the
        // actual traveled segment, and stop at the first solid surface on it.
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
        // Check the final physics step before rendering, including frames with
        // several physics steps. The center-line hit matches the aiming ray.
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
        if (hitParticleGatlin != null && playerController.instance != null)
            playerController.instance.PlayWeaponImpact(hitParticleGatlin, point);
        // Buildings also have untagged mesh colliders beneath their damageable root.
        BuildingHealth building = other.GetComponentInParent<BuildingHealth>();
        EnemyController enemy = other.GetComponentInParent<EnemyController>();
        if (building != null && building.CompareTag("Attackable") && !building.blocksBroken)
            building.buildingHealth -= damage;
        if (enemy != null && enemy.CompareTag("Attackable"))
            enemy.enemyHealth -= damage;
        Recycle();
    }
}
