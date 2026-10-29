using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Waveの開始、敵の出現、終了、インターバルを管理する。
/// </summary>
public sealed class WaveController : MonoBehaviour
{
    public event Action<int> WaveStarted;
    public event Action<int> WaveEnded;
    public event Action SurvivalEnded;

    [SerializeField]
    private WaveSequenceData sequenceData;

    [SerializeField]
    private EnemySpawner enemySpawner;

    [SerializeField]
    private PlayerPresenter playerPresenter;

    [SerializeField]
    private bool playOnStart = true;

    private readonly List<EnemySpawnPlan> spawnPlans = new();
    private readonly Dictionary<string, int> enemyCounts = new();
    private readonly HashSet<WaveEnemyHandle> aliveEnemies = new();

    private WaveState state = WaveState.Idle;

    private int waveIndex;
    private int lastStartedWaveNumber = 1;
    private int nextSpawnIndex;

    private float waveElapsedTime;
    private float intervalRemainingTime;
    private EnemyStatScale currentEnemyStatScale = EnemyStatScale.Default;

    public int CurrentWaveNumber => waveIndex + 1;
    public int ReachedWaveNumber => Mathf.Max(1, lastStartedWaveNumber);
    public int AliveEnemyCount => aliveEnemies.Count;

    public float WaveRemainingTime
    {
        get
        {
            if (sequenceData == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                sequenceData.WaveDuration - waveElapsedTime
            );
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartSequence();
        }
    }

    private void OnEnable()
    {
        if (playerPresenter != null)
        {
            playerPresenter.PlayerDied += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        if (playerPresenter != null)
        {
            playerPresenter.PlayerDied -= OnPlayerDied;
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || Time.deltaTime <= 0f)
        {
            return;
        }

        switch (state)
        {
            case WaveState.Running:
                UpdateWave(Time.deltaTime);
                break;

            case WaveState.Intermission:
                UpdateIntermission(Time.deltaTime);
                break;
        }
    }

    private void OnDestroy()
    {
        UnsubscribeAllEnemies();
    }

    /// <summary>
    /// 一番初めのウェーブを開始する
    /// </summary>
    public void StartSequence()
    {
        if (!CanStartSequence())
        {
            return;
        }

        waveIndex = 0;
        BeginWave();
    }

    public void StopSequence()
    {
        spawnPlans.Clear();
        state = WaveState.Idle;
    }

    private bool CanStartSequence()
    {
        if (sequenceData == null)
        {
            Debug.LogWarning("WaveSequenceDataが設定されていません。");
            return false;
        }

        if (enemySpawner == null || !enemySpawner.IsReady)
        {
            Debug.LogWarning("EnemySpawnerとシーン上のGameObjectPoolを設定してください。", this);
            return false;
        }

        if (playerPresenter == null)
        {
            Debug.LogWarning("PlayerPresenterが設定されていません。");
            return false;
        }

        if (sequenceData.Waves == null ||
            sequenceData.Waves.Count == 0)
        {
            Debug.LogWarning("Waveが1つも設定されていません。");
            return false;
        }

        if (sequenceData.EnemyCatalog == null || sequenceData.WaveDuration <= 0f)
        {
            Debug.LogWarning("敵カタログと0より大きいWave時間を設定してください。", this);
            return false;
        }

        foreach (WaveData wave in sequenceData.Waves)
        {
            if (wave == null)
            {
                Debug.LogWarning("未設定のWaveがあります。", this);
                return false;
            }

            foreach (WaveEnemyData enemy in wave.Enemies)
            {
                if (enemy == null ||
                    !sequenceData.EnemyCatalog.TryGetPrefab(enemy.EnemyId, out GameObject prefab) ||
                    !IsValidEnemyPrefab(prefab))
                {
                    Debug.LogWarning("Waveの敵IDとPrefabのEnemyBase・PooledObject・WaveEnemyHandleを確認してください。", this);
                    return false;
                }
            }
        }

        foreach (EndlessEnemyGrowth growth in sequenceData.EndlessGrowth)
        {
            if (growth == null ||
                !sequenceData.EnemyCatalog.TryGetPrefab(growth.EnemyId, out GameObject prefab) ||
                !IsValidEnemyPrefab(prefab))
            {
                Debug.LogWarning("増加設定の敵IDとPrefabのEnemyBase・PooledObject・WaveEnemyHandleを確認してください。", this);
                return false;
            }
        }

        return true;
    }

    private static bool IsValidEnemyPrefab(GameObject prefab)
    {
        return prefab.TryGetComponent<EnemyBase>(out _) &&
            prefab.TryGetComponent<PooledObject>(out _) &&
            prefab.TryGetComponent<WaveEnemyHandle>(out _);
    }

    /// <summary>
    /// １ウェーブの初期化
    /// </summary>
    private void BeginWave()
    {
        //前のウェーブの時間や敵の生成位置を引き継がないようにリセット
        waveElapsedTime = 0f;
        lastStartedWaveNumber = CurrentWaveNumber;
        nextSpawnIndex = 0;

        CreateSpawnPlans();
        currentEnemyStatScale = sequenceData.GetEnemyStatScale(CurrentWaveNumber);

        state = WaveState.Running;
        WaveStarted?.Invoke(CurrentWaveNumber);

        // 0秒を割り当てられた敵をその場で生成する
        if (state == WaveState.Running)
        {
            SpawnDueEnemies();
        }
    }

    /// <summary>
    /// これから生成する敵の予定表を作成
    /// </summary>
    private void CreateSpawnPlans()
    {
        spawnPlans.Clear();

        float waveDuration = sequenceData.WaveDuration;
        sequenceData.GetEnemyCounts(CurrentWaveNumber, enemyCounts);

        foreach (KeyValuePair<string, int> enemy in enemyCounts)
        {
            if (!sequenceData.EnemyCatalog.TryGetPrefab(enemy.Key, out GameObject prefab))
            {
                continue;
            }

            int count = enemy.Value;

            for (int i = 0; i < count; i++)
            {
                float spawnTime = UnityEngine.Random.Range(
                    0f,
                    waveDuration
                );

                spawnPlans.Add(
                    new EnemySpawnPlan(
                        prefab,
                        spawnTime
                    )
                );
            }
        }

        if (sequenceData.SpawnTiming == WaveSpawnTiming.EvenlySpaced)
        {
            // 敵の種類順に偏らないよう、先に全個体をシャッフルする。
            for (int i = spawnPlans.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                EnemySpawnPlan temporary = spawnPlans[i];
                spawnPlans[i] = spawnPlans[j];
                spawnPlans[j] = temporary;
            }

            for (int i = 0; i < spawnPlans.Count; i++)
            {
                spawnPlans[i] = new EnemySpawnPlan(
                    spawnPlans[i].EnemyPrefab,
                    waveDuration * ((float)i / spawnPlans.Count)
                );
            }
        }
        else
        {
            spawnPlans.Sort((left, right) => left.SpawnTime.CompareTo(right.SpawnTime));
        }
    }

    /// <summary>
    /// ウェーブ進行関数
    /// </summary>
    /// <param name="deltaTime"></param>
    private void UpdateWave(float deltaTime)
    {
        waveElapsedTime = Mathf.Min(
            waveElapsedTime + deltaTime,
            sequenceData.WaveDuration
        );

        if (waveElapsedTime >= sequenceData.WaveDuration)
        {
            // 終了フレームで取りこぼした予定は次Waveへ持ち越さない。
            FinishWave();
            return;
        }

        SpawnDueEnemies();
    }

    /// <summary>
    /// 時間になった敵を生成する関数
    /// </summary>
    private void SpawnDueEnemies()
    {
        while (state == WaveState.Running && nextSpawnIndex < spawnPlans.Count)
        {
            EnemySpawnPlan plan =
                spawnPlans[nextSpawnIndex];

            if (plan.SpawnTime > waveElapsedTime)
            {
                break;
            }

            nextSpawnIndex++;
            SpawnEnemy(plan.EnemyPrefab);
        }
    }

    /// <summary>
    /// 指定した敵を生成する関数
    /// </summary>
    /// <param name="enemyPrefab"></param>
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        WaveEnemyHandle enemy =
            enemySpawner.Spawn(enemyPrefab, currentEnemyStatScale);

        if (ReferenceEquals(enemy, null))
        {
            return;
        }

        if (aliveEnemies.Add(enemy))
        {
            enemy.Removed += OnEnemyRemoved;
        }
    }


    /// <summary>
    /// 敵が倒された時の処理
    /// </summary>
    /// <param name="enemy"></param>
    private void OnEnemyRemoved(WaveEnemyHandle enemy)
    {
        if (ReferenceEquals(enemy, null))
        {
            return;
        }

        enemy.Removed -= OnEnemyRemoved;
        aliveEnemies.Remove(enemy);
    }

    /// <summary>
    /// １ウェーブが終わった時の処理
    /// </summary>
    private void FinishWave()
    {
        if (state != WaveState.Running)
        {
            return;
        }

        spawnPlans.Clear();

        int finishedWaveNumber = CurrentWaveNumber;
        waveIndex++;

        intervalRemainingTime =
            sequenceData.IntervalBetweenWaves;

        state = WaveState.Intermission;
        WaveEnded?.Invoke(finishedWaveNumber);
        Debug.Log($"Wave {finishedWaveNumber} 終了");
    }

    /// <summary>
    /// ウェーブ間の待機時間
    /// </summary>
    /// <param name="deltaTime"></param>
    private void UpdateIntermission(float deltaTime)
    {
        intervalRemainingTime -= deltaTime;

        if (intervalRemainingTime <= 0f)
        {
            BeginWave();
        }
    }

    private void OnPlayerDied()
    {
        if (state == WaveState.Idle)
        {
            return;
        }

        StopSequence();
        SurvivalEnded?.Invoke();
        Debug.Log("プレイヤー死亡によりウェーブ進行を終了");
    }

    /// <summary>
    /// 敵のイベント登録解除
    /// </summary>
    private void UnsubscribeAllEnemies()
    {
        foreach (WaveEnemyHandle enemy in aliveEnemies)
        {
            if (enemy != null)
            {
                enemy.Removed -= OnEnemyRemoved;
            }
        }

        aliveEnemies.Clear();
    }

    /// <summary>
    /// ウェーブの状態
    /// </summary>
    private enum WaveState
    {
        Idle,
        Running,
        Intermission
    }

    /// <summary>
    /// 一体分のスポーンデータ
    /// </summary>
    private readonly struct EnemySpawnPlan
    {
        public GameObject EnemyPrefab { get; }
        public float SpawnTime { get; }

        public EnemySpawnPlan(
            GameObject enemyPrefab,
            float spawnTime)
        {
            EnemyPrefab = enemyPrefab;
            SpawnTime = spawnTime;
        }
    }
}
