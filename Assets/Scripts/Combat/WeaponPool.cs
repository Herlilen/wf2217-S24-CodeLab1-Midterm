using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

// Owned by one player controller. Active shots stay in world space, not under the moving player.
public sealed class WeaponPool : IDisposable
{
    private readonly GameObject root;
    private readonly Transform creationRoot;
    private readonly Dictionary<GameObject, ObjectPool<PooledWeaponObject>> pools =
        new Dictionary<GameObject, ObjectPool<PooledWeaponObject>>();
    private readonly HashSet<PooledWeaponObject> active = new HashSet<PooledWeaponObject>();
    private int reservedActiveCount;
    private bool disposed;

    public WeaponPool()
    {
        root = new GameObject("Player Weapon Pools");
        UnityEngine.Object.DontDestroyOnLoad(root);
        GameObject staging = new GameObject("Inactive Creation");
        staging.transform.SetParent(root.transform, false);
        staging.SetActive(false);
        creationRoot = staging.transform;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    public void Prewarm(GameObject prefab, int count, bool returnWhenParticlesFinish = false)
    {
        if (disposed || prefab == null || pools.ContainsKey(prefab)) return;
        count = Mathf.Max(0, count);
        Transform group = new GameObject(prefab.name + " Pool").transform;
        group.SetParent(root.transform, false);
        var pool = new ObjectPool<PooledWeaponObject>(
            () => CreateItem(prefab, group, returnWhenParticlesFinish),
            null, item => item.EndLease(), item =>
            {
                // Unity may destroy this independent root before the player when exiting Play mode.
                if (item != null) UnityEngine.Object.Destroy(item.gameObject);
            },
            true, Mathf.Max(1, count), Mathf.Max(256, count));
        pools.Add(prefab, pool);

        // Hold every item until the pool has created the requested number of distinct instances.
        var warmed = new PooledWeaponObject[count];
        for (int i = 0; i < count; i++) warmed[i] = pool.Get();
        for (int i = 0; i < count; i++) pool.Release(warmed[i]);
        reservedActiveCount += count;
        active.EnsureCapacity(reservedActiveCount);
    }

    private PooledWeaponObject CreateItem(GameObject prefab, Transform group, bool particleEffect)
    {
        // Creating beneath an inactive parent prevents play-on-awake particles and Start callbacks.
        GameObject instance = UnityEngine.Object.Instantiate(prefab, creationRoot, false);
        instance.SetActive(false);
        instance.transform.SetParent(group, false);
        PooledWeaponObject item = instance.AddComponent<PooledWeaponObject>();
        item.Configure(this, prefab, particleEffect);
        return item;
    }

    public GameObject Rent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (disposed || prefab == null) return null;
        ObjectPool<PooledWeaponObject> pool = pools[prefab];
        PooledWeaponObject item = pool.Get();
        active.Add(item);
        item.BeginLease(position, rotation);
        return item.gameObject;
    }

    public void PlayImpact(GameObject prefab, Vector3 position)
    {
        if (disposed || prefab == null) return;
        if (!pools.ContainsKey(prefab)) Prewarm(prefab, 0, true);
        Rent(prefab, position, Quaternion.identity);
    }

    internal void Return(PooledWeaponObject item)
    {
        if (disposed || item == null || !active.Remove(item)) return;
        pools[item.SourcePrefab].Release(item);
    }

    public void ReturnAll()
    {
        // Take one item at a time: Return modifies the set, so do not hold an enumerator across it.
        while (active.Count > 0)
        {
            PooledWeaponObject item;
            using (var iterator = active.GetEnumerator())
            {
                iterator.MoveNext();
                item = iterator.Current;
            }
            if (item == null) active.Remove(item);
            else Return(item);
        }
    }

    private void OnActiveSceneChanged(Scene previous, Scene current)
    {
        ReturnAll();
    }

    public void Dispose()
    {
        if (disposed) return;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        ReturnAll();
        disposed = true;
        foreach (ObjectPool<PooledWeaponObject> pool in pools.Values) pool.Clear();
        pools.Clear();
        if (root != null) UnityEngine.Object.Destroy(root);
    }
}
