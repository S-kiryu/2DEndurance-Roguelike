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
    private readonly HashSet<WaveEnemyHandle> aliveEnemies = new();

    private WaveState state = WaveState.Idle;

    private int waveIndex;
    private int nextSpawnIndex;

    private float waveElapsedTime;
    private float intervalRemainingTime;

    public int CurrentWaveNumber => waveIndex + 1;
    public int AliveEnemyCount => aliveEnemies.Count;

    /// <summary>
    /// 登録された全Waveを一巡するごとに増える、1始まりのループ番号。
    /// </summary>
    public int CurrentLoopNumber
    {
        get
        {
            if (sequenceData == null ||
                sequenceData.Waves == null ||
                sequenceData.Waves.Count == 0)
            {
                return 1;
            }

            return waveIndex / sequenceData.Waves.Count + 1;
        }
    }

    /// <summary>
    /// 初回は1倍。全Waveを一巡するたびに共通設定の倍率を累積する。
    /// </summary>
    public float CurrentEnemyMultiplier => sequenceData == null
        ? 1f
        : Mathf.Pow(
            sequenceData.MultiplierPerLoop,
            CurrentLoopNumber - 1
        );

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
        Debug.Log("ゲームスタート");
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

        if (enemySpawner == null)
        {
            Debug.LogWarning("EnemySpawnerが設定されていません。");
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

        return true;
    }

    /// <summary>
    /// １ウェーブの初期化
    /// </summary>
    private void BeginWave()
    {
        int waveDataIndex = waveIndex % sequenceData.Waves.Count;
        WaveData wave = sequenceData.Waves[waveDataIndex];

        //前のウェーブの時間や敵の生成位置を引き継がないようにリセット
        waveElapsedTime = 0f;
        nextSpawnIndex = 0;

        CreateSpawnPlans(wave);

        state = WaveState.Running;
        WaveStarted?.Invoke(CurrentWaveNumber);

        // 0秒を割り当てられた敵をその場で生成する
        SpawnDueEnemies();
    }

    /// <summary>
    /// これから生成する敵の予定表を作成
    /// </summary>
    /// <param name="wave">予定表を作成したいウェーブデータを入れる</param>
    private void CreateSpawnPlans(WaveData wave)
    {
        spawnPlans.Clear();

        if (wave == null || wave.Enemies == null)
        {
            return;
        }

        float waveDuration = sequenceData.WaveDuration;

        foreach (WaveEnemyData enemyData in wave.Enemies)
        {
            if (enemyData == null ||
                enemyData.EnemyPrefab == null)
            {
                continue;
            }

            int count = Mathf.Max(0, enemyData.Count);

            for (int i = 0; i < count; i++)
            {
                float spawnTime = UnityEngine.Random.Range(
                    0f,
                    waveDuration
                );

                spawnPlans.Add(
                    new EnemySpawnPlan(
                        enemyData.EnemyPrefab,
                        spawnTime
                    )
                );
                Debug.Log(spawnTime);
            }
        }

        spawnPlans.Sort(
            (left, right) =>
                left.SpawnTime.CompareTo(right.SpawnTime)
        );
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

        SpawnDueEnemies();

        if (waveElapsedTime >= sequenceData.WaveDuration)
        {
            FinishWave();
        }
    }

    /// <summary>
    /// 時間になった敵を生成する関数
    /// </summary>
    private void SpawnDueEnemies()
    {
        while (nextSpawnIndex < spawnPlans.Count)
        {
            EnemySpawnPlan plan =
                spawnPlans[nextSpawnIndex];

            if (plan.SpawnTime > waveElapsedTime)
            {
                break;
            }

            SpawnEnemy(plan.EnemyPrefab);
            nextSpawnIndex++;
        }
    }

    /// <summary>
    /// 指定した敵を生成する関数
    /// </summary>
    /// <param name="enemyPrefab"></param>
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        WaveEnemyHandle enemy =
            enemySpawner.Spawn(enemyPrefab);

        if (enemy == null)
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
        if (enemy == null)
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
    private sealed class EnemySpawnPlan
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
