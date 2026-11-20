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

    public int CurrentWaveNumber => _waveIndex + 1;
    public int ReachedWaveNumber => Mathf.Max(1, _lastStartedWaveNumber);
    public int AliveEnemyCount => _aliveEnemies.Count;

    public float WaveRemainingTime
    {
        get
        {
            if (_sequenceData == null)
            {
                return 0f;
            }

            return Mathf.Max(
                0f,
                _sequenceData.WaveDuration - _waveElapsedTime
            );
        }
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

        _waveIndex = 0;
        BeginWave();
    }

    /// <summary>
    /// ウェーブを止める
    /// </summary>
    public void StopSequence()
    {
        _spawnPlans.Clear();
        _state = WaveState.Idle;
    }

    [SerializeField]
    private WaveSequenceData _sequenceData;

    [SerializeField]
    private EnemySpawner _enemySpawner;

    [SerializeField]
    private PlayerPresenter _playerPresenter;

    [SerializeField]
    private bool _playOnStart = true;

    private readonly List<EnemySpawnPlan> _spawnPlans = new();
    private readonly Dictionary<string, int> _enemyCounts = new();
    private readonly HashSet<WaveEnemyHandle> _aliveEnemies = new();

    private WaveState _state = WaveState.Idle;

    private int _waveIndex;
    private int _lastStartedWaveNumber = 1;
    private int _nextSpawnIndex;

    private float _waveElapsedTime;
    private float _intervalRemainingTime;
    private EnemyStatScale _currentEnemyStatScale = EnemyStatScale.Default;

    private void Start()
    {
        if (_playOnStart)
        {
            StartSequence();
        }
    }

    private void OnEnable()
    {
        if (_playerPresenter != null)
        {
            _playerPresenter.PlayerDied += OnPlayerDied;
        }
    }

    private void OnDisable()
    {
        if (_playerPresenter != null)
        {
            _playerPresenter.PlayerDied -= OnPlayerDied;
        }
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || Time.deltaTime <= 0f)
        {
            return;
        }

        switch (_state)
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
    /// スタートに必要な情報が残っているのかを確認する
    /// </summary>
    /// <returns></returns>
    private bool CanStartSequence()
    {
        if (_sequenceData == null)
        {
            Debug.LogWarning("WaveSequenceDataが設定されていません。");
            return false;
        }

        if (_enemySpawner == null || !_enemySpawner.IsReady)
        {
            Debug.LogWarning("EnemySpawnerとシーン上のGameObjectPoolを設定してください。", this);
            return false;
        }

        if (_playerPresenter == null)
        {
            Debug.LogWarning("PlayerPresenterが設定されていません。");
            return false;
        }

        if (_sequenceData.Waves == null ||
            _sequenceData.Waves.Count == 0)
        {
            Debug.LogWarning("Waveが1つも設定されていません。");
            return false;
        }

        if (_sequenceData.EnemyCatalog == null || _sequenceData.WaveDuration <= 0f)
        {
            Debug.LogWarning("敵カタログと0より大きいWave時間を設定してください。", this);
            return false;
        }

        foreach (WaveData wave in _sequenceData.Waves)
        {
            if (wave == null)
            {
                Debug.LogWarning("未設定のWaveがあります。", this);
                return false;
            }

            foreach (WaveEnemyData enemy in wave.Enemies)
            {
                if (enemy == null ||
                    !_sequenceData.EnemyCatalog.TryGetPrefab(enemy.EnemyId, out GameObject prefab) ||
                    !IsValidEnemyPrefab(prefab))
                {
                    Debug.LogWarning("Waveの敵IDとPrefabのEnemyBase・WaveEnemyHandleを確認してください。", this);
                    return false;
                }
            }
        }

        foreach (EndlessEnemyGrowth growth in _sequenceData.EndlessGrowth)
        {
            if (growth == null ||
                !_sequenceData.EnemyCatalog.TryGetPrefab(growth.EnemyId, out GameObject prefab) ||
                !IsValidEnemyPrefab(prefab))
            {
                Debug.LogWarning("増加設定の敵IDとPrefabのEnemyBase・WaveEnemyHandleを確認してください。", this);
                return false;
            }
        }

        return true;
    }

    private static bool IsValidEnemyPrefab(GameObject prefab)
    {
        return prefab.TryGetComponent<EnemyBase>(out _) &&
            prefab.TryGetComponent<WaveEnemyHandle>(out _);
    }

    /// <summary>
    /// １ウェーブの初期化
    /// </summary>
    private void BeginWave()
    {
        //前のウェーブの時間や敵の生成位置を引き継がないようにリセット
        _waveElapsedTime = 0f;
        _lastStartedWaveNumber = CurrentWaveNumber;
        _nextSpawnIndex = 0;

        CreateSpawnPlans();
        _currentEnemyStatScale = _sequenceData.GetEnemyStatScale(CurrentWaveNumber);

        _state = WaveState.Running;
        WaveStarted?.Invoke(CurrentWaveNumber);

        // 0秒を割り当てられた敵をその場で生成する
        if (_state == WaveState.Running)
        {
            SpawnDueEnemies();
        }
    }

    /// <summary>
    /// これから生成する敵の予定表を作成
    /// </summary>
    private void CreateSpawnPlans()
    {
        _spawnPlans.Clear();

        float waveDuration = _sequenceData.WaveDuration;
        _sequenceData.GetEnemyCounts(CurrentWaveNumber, _enemyCounts);

        foreach (KeyValuePair<string, int> enemy in _enemyCounts)
        {
            if (!_sequenceData.EnemyCatalog.TryGetPrefab(enemy.Key, out GameObject prefab))
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

                _spawnPlans.Add(
                    new EnemySpawnPlan(
                        prefab,
                        spawnTime
                    )
                );
            }
        }

        if (_sequenceData.SpawnTiming == WaveSpawnTiming.EvenlySpaced)
        {
            // 敵の種類順に偏らないよう、先に全個体をシャッフルする。
            for (int i = _spawnPlans.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                EnemySpawnPlan temporary = _spawnPlans[i];
                _spawnPlans[i] = _spawnPlans[j];
                _spawnPlans[j] = temporary;
            }

            for (int i = 0; i < _spawnPlans.Count; i++)
            {
                _spawnPlans[i] = new EnemySpawnPlan(
                    _spawnPlans[i].EnemyPrefab,
                    waveDuration * ((float)i / _spawnPlans.Count)
                );
            }
        }
        else
        {
            _spawnPlans.Sort((left, right) => left.SpawnTime.CompareTo(right.SpawnTime));
        }
    }

    /// <summary>
    /// ウェーブ進行関数
    /// </summary>
    /// <param name="deltaTime"></param>
    private void UpdateWave(float deltaTime)
    {
        _waveElapsedTime = Mathf.Min(
            _waveElapsedTime + deltaTime,
            _sequenceData.WaveDuration
        );

        SpawnDueEnemies();

        if (_waveElapsedTime >= _sequenceData.WaveDuration)
        {
            FinishWave();
        }
    }

    /// <summary>
    /// 時間になった敵を生成する関数
    /// </summary>
    private void SpawnDueEnemies()
    {
        while (_state == WaveState.Running && _nextSpawnIndex < _spawnPlans.Count)
        {
            EnemySpawnPlan plan =
                _spawnPlans[_nextSpawnIndex];

            if (plan.SpawnTime > _waveElapsedTime)
            {
                break;
            }

            _nextSpawnIndex++;
            SpawnEnemy(plan.EnemyPrefab);
        }
    }

    /// <summary>
    /// 指定した敵を生成する関数
    /// </summary>
    /// <param name="enemyPrefab"></param>
    private void SpawnEnemy(GameObject enemyPrefab)
    {
        TrackEnemy(_enemySpawner.Spawn(enemyPrefab, _currentEnemyStatScale));
    }

    private void TrackEnemy(WaveEnemyHandle enemy)
    {
        if (ReferenceEquals(enemy, null))
        {
            return;
        }

        if (_aliveEnemies.Add(enemy))
        {
            enemy.Removed += OnEnemyRemoved;
            enemy.ChildSpawned += TrackEnemy;
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
        enemy.ChildSpawned -= TrackEnemy;
        _aliveEnemies.Remove(enemy);
    }

    /// <summary>
    /// １ウェーブが終わった時の処理
    /// </summary>
    private void FinishWave()
    {
        if (_state != WaveState.Running)
        {
            return;
        }

        _spawnPlans.Clear();

        int finishedWaveNumber = CurrentWaveNumber;
        _waveIndex++;

        _intervalRemainingTime =
            _sequenceData.IntervalBetweenWaves;

        _state = WaveState.Intermission;
        WaveEnded?.Invoke(finishedWaveNumber);
        Debug.Log($"Wave {finishedWaveNumber} 終了");
    }

    /// <summary>
    /// ウェーブ間の待機時間
    /// </summary>
    /// <param name="deltaTime"></param>
    private void UpdateIntermission(float deltaTime)
    {
        _intervalRemainingTime -= deltaTime;

        if (_intervalRemainingTime <= 0f)
        {
            BeginWave();
        }
    }

    private void OnPlayerDied()
    {
        if (_state == WaveState.Idle)
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
        foreach (WaveEnemyHandle enemy in _aliveEnemies)
        {
            if (enemy != null)
            {
                enemy.Removed -= OnEnemyRemoved;
                enemy.ChildSpawned -= TrackEnemy;
            }
        }

        _aliveEnemies.Clear();
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
