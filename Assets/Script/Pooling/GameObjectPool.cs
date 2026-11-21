using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>開始時に確保するPrefabと待機数</summary>
[Serializable]
public sealed class PoolPrewarmEntry
{
    public GameObject Prefab => _prefab;
    public int Count => _count;

    [SerializeField] private GameObject _prefab;
    [SerializeField, Min(0)] private int _count;
}

/// <summary>
/// ゲームオブジェクトのプール
/// </summary>
[DefaultExecutionOrder(-1000)]
[DisallowMultipleComponent]
public sealed class GameObjectPool : MonoBehaviour
{
    public bool IsReady => _initialized && !_shuttingDown && isActiveAndEnabled;

    /// <summary>
    /// 指定したプレハブを出現させる
    /// </summary>
    /// <param name="prefab">出現させたいプレハブ</param>
    /// <param name="position">位置</param>
    /// <param name="rotation">回転</param>
    /// <returns>出現したインスタンス</returns>
    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (!IsReady || prefab == null)
        {
            return null;
        }

        Stack<GameObject> stack = GetStack(prefab);
        GameObject instance = null;
        while (stack.Count > 0 && instance == null)
        {
            instance = stack.Pop();
        }

        if (instance == null)
        {
            instance = CreateInstance(prefab);
        }

        instance.transform.SetParent(null, false);
        instance.transform.SetPositionAndRotation(position, rotation);
        _instances[instance].PrepareForSpawn(instance.transform);
        instance.SetActive(true);
        return instance;
    }

    /// <summary>
    /// コンポーメントを指定して出現させる
    /// </summary>
    /// <typeparam name="T">コンポーメントの型</typeparam>
    /// <param name="prefab">出現させたいコンポーメント</param>
    /// <param name="position">生成位置</param>
    /// <param name="rotation">回転</param>
    /// <returns></returns>
    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        if (prefab == null)
        {
            return null;
        }

        GameObject instance = Spawn(prefab.gameObject, position, rotation);
        return instance != null ? instance.GetComponent<T>() : null;
    }

    /// <summary>戦闘開始前などに個体を確保して初回生成の負荷を分散できる。</summary>
    public void Prewarm(GameObject prefab, int count)
    {
        if (!IsReady || prefab == null)
        {
            return;
        }

        Stack<GameObject> stack = GetStack(prefab);
        int target = Mathf.Max(0, count);
        while (stack.Count < target)
        {
            stack.Push(CreateInstance(prefab));
        }
    }

    /// <summary>
    /// プール由来なら返却、それ以外の個体は通常どおり破棄する。
    /// </summary>
    public static void Despawn(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        GameObjectPool pool = _scenePool;
        if (pool != null && pool._instances.TryGetValue(instance, out InstanceData data))
        {
            pool.Release(instance, data);
            return;
        }

        Destroy(instance);
    }

    internal static GameObjectPool FindPoolFor(GameObject instance)
    {
        return _scenePool != null && instance != null && _scenePool._instances.ContainsKey(instance)
            ? _scenePool
            : null;
    }

    /// <summary>
    /// プールに戻す処理
    /// </summary>
    /// <param name="instance"></param>
    private void Release(GameObject instance, InstanceData data)
    {
        if (_shuttingDown || !instance.activeSelf)
        {
            return;
        }

        // 先に無効化して、通知中の二重返却を防ぐ。
        instance.SetActive(false);
        data.NotifyDespawned();
        instance.transform.SetParent(_storage, false);

        GetStack(data.Prefab).Push(instance);
    }

    [Tooltip("シーンに配置した待機用の子オブジェクト")]
    [SerializeField] private Transform _storage;

    [Tooltip("ゲーム開始時に確保する個体。以降は不足した分だけ生成します。")]
    [SerializeField] private List<PoolPrewarmEntry> _prewarmEntries = new();

    private static GameObjectPool _scenePool;
    private readonly Dictionary<GameObject, Stack<GameObject>> _inactive = new();
    private readonly Dictionary<GameObject, InstanceData> _instances = new();
    private bool _initialized;
    private bool _shuttingDown;

    private void Awake()
    {
        if (_storage == null || _storage == transform || !_storage.IsChildOf(transform))
        {
            Debug.LogError("GameObjectPoolのStorageに、シーンへ配置した待機用の子オブジェクトを設定してください。", this);
            enabled = false;
            return;
        }

        _storage.gameObject.SetActive(false);
        _initialized = true;
        _scenePool = this;
        foreach (PoolPrewarmEntry entry in _prewarmEntries)
        {
            if (entry != null)
            {
                Prewarm(entry.Prefab, entry.Count);
            }
        }
    }

    /// <summary>
    /// スタック数を取得できる
    /// </summary>
    /// <param name="prefab">数が欲しいプレハブ</param>
    /// <returns></returns>
    private Stack<GameObject> GetStack(GameObject prefab)
    {
        if (!_inactive.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>();
            _inactive.Add(prefab, stack);
        }

        return stack;
    }

    /// <summary>
    /// いなかった時の生成処理
    /// </summary>
    /// <param name="prefab">生成するプレハブ</param>
    /// <returns></returns>
    private GameObject CreateInstance(GameObject prefab)
    {
        // 最初から非アクティブな親の下に作り、初期化前のOnEnableを防ぐ。
        GameObject instance = Instantiate(prefab, _storage);
        instance.SetActive(false);
        _instances.Add(instance, new InstanceData(instance, prefab));
        return instance;
    }

    private void OnDestroy()
    {
        _shuttingDown = true;
        _inactive.Clear();
        foreach (GameObject instance in _instances.Keys)
        {
            if (instance != null)
            {
                Destroy(instance);
            }
        }

        _instances.Clear();
        if (_scenePool == this)
            _scenePool = null;
    }

    /// <summary>
    /// 再利用するオブジェクト1個分の情報
    /// </summary>
    private sealed class InstanceData
    {
        public GameObject Prefab { get; }

        private readonly Vector3 _originalScale;
        private readonly Rigidbody2D[] _bodies;
        private readonly IPoolable[] _callbacks;

        public InstanceData(GameObject instance, GameObject prefab)
        {
            Prefab = prefab;
            _originalScale = prefab.transform.localScale;
            _bodies = instance.GetComponentsInChildren<Rigidbody2D>(true);
            _callbacks = instance.GetComponentsInChildren<IPoolable>(true);
        }

        public void PrepareForSpawn(Transform instanceTransform)
        {
            instanceTransform.localScale = _originalScale;
            foreach (Rigidbody2D body in _bodies)
            {
                if (body == null)
                    continue;

                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            foreach (IPoolable listener in _callbacks)
                listener.OnPoolSpawned();
        }

        public void NotifyDespawned()
        {
            foreach (IPoolable listener in _callbacks)
                listener.OnPoolDespawned();
        }
    }
}
