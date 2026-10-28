using System.Collections.Generic;
using UnityEngine;

/// <summary>プールの所有者と貸出状態。対象Prefabのルートへ事前に設定する。</summary>
[DisallowMultipleComponent]
public sealed class PooledObject : MonoBehaviour
{
    private IPoolable[] callbacks;
    private Rigidbody2D[] bodies;
    private Vector3 originalScale;

    internal GameObjectPool Owner { get; private set; }
    internal GameObject Prefab { get; private set; }
    public bool IsSpawned { get; internal set; }
    public int SpawnVersion { get; private set; }

    internal void Initialize(GameObjectPool owner, GameObject prefab)
    {
        Owner = owner;
        Prefab = prefab;
        originalScale = prefab.transform.localScale;
        bodies = GetComponentsInChildren<Rigidbody2D>(true);

        var listeners = new List<IPoolable>();
        foreach (MonoBehaviour behaviour in GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour is IPoolable listener)
            {
                listeners.Add(listener);
            }
        }

        callbacks = listeners.ToArray();
    }

    internal void PrepareForSpawn()
    {
        SpawnVersion = unchecked(SpawnVersion + 1);
        transform.localScale = originalScale;
        foreach (Rigidbody2D body in bodies)
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }
        }

        foreach (IPoolable listener in callbacks)
        {
            listener.OnPoolSpawned();
        }
    }

    internal void PrepareForDespawn()
    {
        foreach (IPoolable listener in callbacks)
        {
            listener.OnPoolDespawned();
        }
    }

    public void Release()
    {
        if (Owner != null)
        {
            Owner.Release(this);
        }
    }
}
