using UnityEngine;

/// <summary>
/// 敵Prefabの生成を担当する。
/// </summary>
public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private Transform[] spawnPoints;
    [SerializeField]
    private GameObject player;

    public WaveEnemyHandle Spawn(GameObject enemyPrefab)
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("敵Prefabが設定されていません。");
            return null;
        }

        Transform spawnPoint = GetRandomSpawnPoint();

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        EnemyMove enemyMove = enemy.GetComponent<EnemyMove>();

        if (enemyMove != null)
        {
            enemyMove.Initialize(player);
        }

        WaveEnemyHandle handle =
            enemy.GetComponent<WaveEnemyHandle>();

        if (handle == null)
        {
            handle = enemy.AddComponent<WaveEnemyHandle>();
        }

        return handle;
    }

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