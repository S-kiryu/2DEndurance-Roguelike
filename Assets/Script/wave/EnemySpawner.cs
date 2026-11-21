using UnityEngine;

/// <summary>
/// 敵Prefabの生成を担当する。
/// </summary>
public sealed class EnemySpawner : MonoBehaviour
{
    public bool IsReady => _objectPool != null && _objectPool.IsReady;

    /// <summary>
    /// 敵のスポーン処理
    /// </summary>
    /// <param name="enemyPrefab"></param>
    /// <returns></returns>
    public WaveEnemyHandle Spawn(GameObject enemyPrefab)
    {
        return Spawn(enemyPrefab, EnemyStatScale.Default);
    }

    /// <summary>
    /// スポーン処理
    /// </summary>
    /// <param name="enemyPrefab"></param>
    /// <param name="statScale"></param>
    /// <returns></returns>
    public WaveEnemyHandle Spawn(GameObject enemyPrefab, EnemyStatScale statScale)
    {
        if (!IsReady)
        {
            Debug.LogError("EnemySpawnerに、シーン上の設定済みGameObjectPoolを指定してください。", this);
            return null;
        }

        if (enemyPrefab == null || !enemyPrefab.TryGetComponent<WaveEnemyHandle>(out _))
        {
            Debug.LogError("敵PrefabのルートにWaveEnemyHandleを事前に設定してください。", this);
            return null;
        }

        Transform spawnPoint = GetRandomSpawnPoint();

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.y = enemyPrefab.transform.position.y;

        GameObject enemy = _objectPool.Spawn(
            enemyPrefab,
            spawnPosition,
            spawnPoint.rotation
        );

        if (enemy == null)
        {
            return null;
        }

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

        if (enemyBase != null)
        {
            enemyBase.Initialize(_player, statScale);
        }

        WaveEnemyHandle handle =
            enemy.GetComponent<WaveEnemyHandle>();

        return handle;
    }

    [SerializeField]
    private GameObjectPool _objectPool;

    [SerializeField]
    private Transform[] _spawnPoints;
    [SerializeField]
    private GameObject _player;

    /// <summary>
    /// ランダムでスポーン位置を決める
    /// </summary>
    /// <returns></returns>
    private Transform GetRandomSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            return transform;
        }

        int index = Random.Range(0, _spawnPoints.Length);
        Transform spawnPoint = _spawnPoints[index];

        return spawnPoint != null
            ? spawnPoint
            : transform;
    }
}
