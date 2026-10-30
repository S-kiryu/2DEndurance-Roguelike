using UnityEngine;

/// <summary>
/// 敵Prefabの生成を担当する。
/// </summary>
public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private GameObjectPool objectPool;

    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject player;

    public bool IsReady => objectPool != null && objectPool.IsReady;

    /// <summary>
    /// 敵のスポーン処理
    /// </summary>
    /// <param name="enemyPrefab"></param>
    /// <returns></returns>
    public WaveEnemyHandle Spawn(GameObject enemyPrefab)
    {
        return Spawn(enemyPrefab, EnemyStatScale.Default);
    }

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

        GameObject enemy = objectPool.Spawn(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        if (enemy == null)
        {
            return null;
        }

        EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();

        if (enemyBase != null)
        {
            enemyBase.Initialize(player, statScale);
        }

        WaveEnemyHandle handle =
            enemy.GetComponent<WaveEnemyHandle>();

        return handle;
    }

    /// <summary>
    /// ランダムでスポーン位置を決める
    /// </summary>
    /// <returns></returns>
    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return transform;
        }

        int index = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[index];

        return spawnPoint != null
            ? spawnPoint
            : transform;
    }
}
