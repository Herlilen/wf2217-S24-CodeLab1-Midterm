using UnityEngine;

// Attached only to runtime copies; the original projectile and effect prefabs stay reusable.
public sealed class PooledWeaponObject : MonoBehaviour
{
    public GameObject SourcePrefab { get; private set; }
    private WeaponPool pool;
    private ParticleSystem[] particles;
    private TrailRenderer[] trails;
    private Rigidbody[] bodies;
    private bool particleEffect;
    private bool leased;
    private int spawnFrame;

    internal void Configure(WeaponPool ownerPool, GameObject sourcePrefab, bool isParticleEffect)
    {
        pool = ownerPool;
        SourcePrefab = sourcePrefab;
        particleEffect = isParticleEffect;
        particles = GetComponentsInChildren<ParticleSystem>(true);
        trails = GetComponentsInChildren<TrailRenderer>(true);
        bodies = GetComponentsInChildren<Rigidbody>(true);
        foreach (ParticleSystem system in particles)
        {
            var main = system.main;
            main.playOnAwake = false;
            main.stopAction = ParticleSystemStopAction.None;
            if (particleEffect)
            {
                main.loop = false;
                // Offscreen effects must still finish so they can be returned to the pool.
                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            }
        }
        foreach (TrailRenderer trail in trails) trail.autodestruct = false;
    }

    internal void BeginLease(Vector3 position, Quaternion rotation)
    {
        ClearState();
        transform.SetPositionAndRotation(position, rotation);
        leased = true;
        spawnFrame = Time.frameCount;
        gameObject.SetActive(true);
        foreach (Rigidbody body in bodies)
        {
            body.position = body.transform.position;
            body.rotation = body.transform.rotation;
            if (!body.isKinematic) body.WakeUp();
        }
        foreach (TrailRenderer trail in trails) trail.Clear();
        foreach (ParticleSystem system in particles)
            if (system.gameObject.activeInHierarchy) system.Play(false);
    }

    internal void EndLease()
    {
        leased = false;
        ClearState();
        gameObject.SetActive(false);
    }

    private void ClearState()
    {
        foreach (Rigidbody body in bodies)
        {
            if (body == null || body.isKinematic) continue;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
        foreach (ParticleSystem system in particles)
            if (system != null) system.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        foreach (TrailRenderer trail in trails)
            if (trail != null) trail.Clear();
    }

    private void LateUpdate()
    {
        if (!leased || !particleEffect || Time.frameCount == spawnFrame) return;
        foreach (ParticleSystem system in particles)
            if (system.gameObject.activeInHierarchy && system.IsAlive(false)) return;
        ReturnToPool();
    }

    public void ReturnToPool()
    {
        if (leased) pool.Return(this);
    }
}
