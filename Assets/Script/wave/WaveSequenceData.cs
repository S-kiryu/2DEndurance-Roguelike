using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1ウェーブで出てくる敵の設定
/// </summary>
[Serializable]
public sealed class WaveEnemyData
{
    public string EnemyId => _enemyId;
    public int Count => Mathf.Max(0, _count);

    [SerializeField]
    private string _enemyId;

    [Min(0)]
    [SerializeField]
    private int _count = 1;
}

/// <summary>2周目以降に適用する、敵ごとの追加出現ルール。</summary>
[Serializable]
public sealed class EndlessEnemyGrowth
{
    public string EnemyId => _enemyId;
    public int IncreaseCount => _increaseCount;
    public int IncreaseEveryWaves => Mathf.Max(1, _increaseEveryWaves);

    public int GetAdditionalCount(int wavesAfterFinal)
    {
        long increases = Mathf.Max(0, wavesAfterFinal) / IncreaseEveryWaves;
        long total = increases * Mathf.Max(0, _increaseCount);
        return (int)Math.Min(int.MaxValue, total);
    }

    [SerializeField]
    private string _enemyId;

    [Tooltip("増加周期を迎えるたびに追加する数")]
    [SerializeField, Min(0)]
    private int _increaseCount = 1;

    [Tooltip("1周目の終了から何Wave進むごとに出現数を増やすか")]
    [SerializeField, Min(1)]
    private int _increaseEveryWaves = 1;
}

public enum WaveSpawnTiming
{
    Random,
    EvenlySpaced
}

/// <summary>
/// 1ウェーブのデータ
/// </summary>
[Serializable]
public sealed class WaveData
{
    public IReadOnlyList<WaveEnemyData> Enemies => _enemies;

    [SerializeField]
    private List<WaveEnemyData> _enemies = new();
}

/// <summary>
/// 全ウェーブをまとめるクラス
/// </summary>
[CreateAssetMenu(
    fileName = "NewWaveSequence",
    menuName = "Wave/Wave Sequence"
)]
public sealed class WaveSequenceData : ScriptableObject
{
    public float WaveDuration => _waveDuration;
    public float IntervalBetweenWaves => _intervalBetweenWaves;
    public EnemyCatalog EnemyCatalog => _enemyCatalog;
    public WaveSpawnTiming SpawnTiming => _spawnTiming;
    public IReadOnlyList<WaveData> Waves => _waves;
    public IReadOnlyList<EndlessEnemyGrowth> EndlessGrowth => _endlessGrowth;
    public EndlessEnemyStatGrowth EndlessStatGrowth => _endlessStatGrowth;

    /// <summary>設定した敵構成を先頭から順番に繰り返す。</summary>
    /// <param name="waveNumber">1始まりの通算Wave番号。</param>
    public WaveData GetWave(int waveNumber)
    {
        if (_waves == null || _waves.Count == 0)
        {
            return null;
        }

        int waveIndex = (Mathf.Max(1, waveNumber) - 1) % _waves.Count;
        return _waves[waveIndex];
    }

    /// <summary>
    /// 1周目は各Waveの指定数を使う。
    /// 2周目以降は、そのWaveの基礎数に1周目終了からの累積増加分を足す。
    /// </summary>
    public void GetEnemyCounts(int waveNumber, Dictionary<string, int> counts)
    {
        counts.Clear();
        WaveData wave = GetWave(waveNumber);
        if (wave == null)
        {
            return;
        }

        foreach (WaveEnemyData enemy in wave.Enemies)
        {
            if (enemy != null)
            {
                AddCount(counts, enemy.EnemyId, enemy.Count);
            }
        }

        int wavesAfterFinal = waveNumber - _waves.Count;
        if (wavesAfterFinal <= 0)
        {
            return;
        }

        foreach (EndlessEnemyGrowth growth in _endlessGrowth)
        {
            if (growth != null)
            {
                AddCount(counts, growth.EnemyId, growth.GetAdditionalCount(wavesAfterFinal));
            }
        }
    }

    /// <summary>指定Waveで新規生成する敵へ適用する倍率を返す。</summary>
    public EnemyStatScale GetEnemyStatScale(int waveNumber)
    {
        if (_waves == null || _waves.Count == 0 || _endlessStatGrowth == null)
        {
            return EnemyStatScale.Default;
        }

        int wavesAfterFinal = waveNumber - _waves.Count;
        return _endlessStatGrowth.GetScale(wavesAfterFinal);
    }

    [SerializeField]
    private EnemyCatalog _enemyCatalog;

    [Header("全Wave共通設定")]
    [Min(0.1f)]
    [SerializeField]
    private float _waveDuration = 30f;

    [Min(0f)]
    [SerializeField]
    private float _intervalBetweenWaves = 5f;

    [SerializeField]
    private WaveSpawnTiming _spawnTiming = WaveSpawnTiming.Random;

    [Header("Waveごとの敵構成")]
    [Tooltip("最後のWaveが終わると先頭の構成に戻ります。")]
    [SerializeField]
    private List<WaveData> _waves = new();

    [Header("2周目以降の出現数増加")]
    [Tooltip("周回中の各Waveの出現数に、指定周期ごとの増加分を累積して加えます。")]
    [SerializeField]
    private List<EndlessEnemyGrowth> _endlessGrowth = new();

    [Header("2周目以降の敵ステータス成長")]
    [SerializeField]
    private EndlessEnemyStatGrowth _endlessStatGrowth = new();

    private static void AddCount(Dictionary<string, int> counts, string id, int count)
    {
        if (string.IsNullOrWhiteSpace(id) || count <= 0)
        {
            return;
        }

        counts.TryGetValue(id, out int current);
        counts[id] = (int)Math.Min(int.MaxValue, (long)current + count);
    }
}
