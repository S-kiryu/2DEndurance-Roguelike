using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>開始時に確保するPrefabと待機数。</summary>
[Serializable]
public sealed class PoolPrewarmEntry
{
    [SerializeField] private GameObject prefab;
    [SerializeField, Min(0)] private int count;

    public GameObject Prefab => prefab;
    public int Count => count;
}

/// <summary>
/// Prefab単位の汎用プール。敵・弾・エフェクトで共用できる。
/// シーンへ事前配置し、利用側から明示的に参照する。
/// </summary>
[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class GameObjectPool : MonoBehaviour
{
    [Tooltip("シーンに配置した待機用の子オブジェクト")]
    [SerializeField] private Transform storage;

    [Tooltip("ゲーム開始時に確保する個体。以降は不足した分だけ生成します。")]
    [SerializeField] private List<PoolPrewarmEntry> prewarmEntries = new();

    private readonly Dictionary<GameObject, Stack<PooledObject>> inactive = new();
    private readonly HashSet<PooledObject> instances = new();
    private bool initialized;
    private bool shuttingDown;

    public bool IsReady => initialized && !shuttingDown && isActiveAndEnabled;

    private void Awake()
    {
        if (storage == null || storage == transform || !storage.IsChildOf(transform))
        {
            Debug.LogError("GameObjectPoolのStorageに、シーンへ配置した待機用の子オブジェクトを設定してください。", this);
            enabled = false;
            return;
        }

        storage.gameObject.SetActive(false);
        initialized = true;
        foreach (PoolPrewarmEntry entry in prewarmEntries)
        {
            if (entry != null)
            {
                Prewarm(entry.Prefab, entry.Count);
            }
        }
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!IsReady || !ValidatePrefab(prefab))
        {
            return null;
        }

        Stack<PooledObject> stack = GetStack(prefab);
        PooledObject instance = null;
        while (stack.Count > 0 && instance == null)
        {
            instance = stack.Pop();
        }

        if (instance == null)
        {
            instance = CreateInstance(prefab);
        }

        // 敵と弾を同じrootにまとめず、所有者判定にtransform.rootを使えるようにする。
        instance.transform.SetParent(null, false);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.IsSpawned = true;
        instance.PrepareForSpawn();
        instance.gameObject.SetActive(true);
        return instance.gameObject;
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject instance = Spawn(prefab.gameObject, position, rotation);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    /// <summary>任意。戦闘開始前などに個体を確保して初回生成の負荷を分散できる。</summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (!IsReady || !ValidatePrefab(prefab))
        {
            return;
        }

        Stack<PooledObject> stack = GetStack(prefab);
        int target = Mathf.Max(0, count);
        while (stack.Count < target)
        {
            stack.Push(CreateInstance(prefab));
        }
    }

    /// <summary>プール由来なら返却。それ以外の個体は通常どおり破棄する。</summary>
    public static void Despawn(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        if (instance.TryGetComponent(out PooledObject pooled) && pooled.Owner != null)
        {
            pooled.Release();
            return;
        }

        Destroy(instance);
    }

    internal void Release(PooledObject instance)
    {
        if (shuttingDown || instance.Owner != this || !instance.IsSpawned)
        {
            return;
        }

        // 通知から再度返却されても二重登録しない。
        instance.IsSpawned = false;
        instance.PrepareForDespawn();
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(storage, false);

        GetStack(instance.Prefab).Push(instance);
    }

    private Stack<PooledObject> GetStack(GameObject prefab)
    {
        if (!inactive.TryGetValue(prefab, out Stack<PooledObject> stack))
        {
            stack = new Stack<PooledObject>();
            inactive.Add(prefab, stack);
        }

        return stack;
    }

    private PooledObject CreateInstance(GameObject prefab)
    {
        // 最初から非アクティブな親の下に作り、初期化前のOnEnableを防ぐ。
        GameObject instance = Instantiate(prefab, storage);
        instance.SetActive(false);
        PooledObject pooled = instance.GetComponent<PooledObject>();
        pooled.Initialize(this, prefab);
        instances.Add(pooled);
        return pooled;
    }

    private bool ValidatePrefab(GameObject prefab)
    {
        if (prefab == null || !prefab.TryGetComponent<PooledObject>(out _))
        {
            Debug.LogError("プール対象のPrefabのルートにPooledObjectを事前に設定してください。", this);
            return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        shuttingDown = true;
        inactive.Clear();
        foreach (PooledObject instance in instances)
        {
            if (instance != null)
            {
                Destroy(instance.gameObject);
            }
        }

        instances.Clear();
    }
}
