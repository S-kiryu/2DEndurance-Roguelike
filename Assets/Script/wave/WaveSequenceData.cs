using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1ウェーブで出てくる敵の設定
/// </summary>
[Serializable]
public sealed class WaveEnemyData
{
    [SerializeField]
    private string enemyId;

    [Min(0)]
    [SerializeField]
    private int count = 1;

    public string EnemyId => enemyId;
    public int Count => Mathf.Max(0, count);
}

/// <summary>全設定Waveの終了後だけ適用する、敵ごとの追加出現ルール。</summary>
[Serializable]
public sealed class EndlessEnemyGrowth
{
    [SerializeField]
    private string enemyId;

    [Tooltip("増加周期を迎えるたびに追加する数")]
    [SerializeField, Min(0)]
    private int increaseCount = 1;

    [Tooltip("最終設定Waveから何Wave進むごとに増加するか")]
    [SerializeField, Min(1)]
    private int increaseEveryWaves = 1;

    public string EnemyId => enemyId;
    public int IncreaseCount => increaseCount;
    public int IncreaseEveryWaves => Mathf.Max(1, increaseEveryWaves);

    public int GetAdditionalCount(int wavesAfterFinal)
    {
        long increases = Mathf.Max(0, wavesAfterFinal) / IncreaseEveryWaves;
        long total = increases * Mathf.Max(0, increaseCount);
        return (int)Math.Min(int.MaxValue, total);
    }
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
    [SerializeField]
    private List<WaveEnemyData> enemies = new();

    public IReadOnlyList<WaveEnemyData> Enemies => enemies;
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
    [SerializeField]
    private EnemyCatalog enemyCatalog;

    [Header("全Wave共通設定")]
    [Min(0.1f)]
    [SerializeField]
    private float waveDuration = 30f;

    [Min(0f)]
    [SerializeField]
    private float intervalBetweenWaves = 5f;

    [SerializeField]
    private WaveSpawnTiming spawnTiming = WaveSpawnTiming.Random;

    [Header("Waveごとの敵構成")]
    [SerializeField]
    private List<WaveData> waves = new();

    [Header("設定Wave終了後の増加設定")]
    [Tooltip("通常Waveには適用しません。最後のWaveの出現数を基準に、指定周期ごとに加算します。")]
    [SerializeField]
    private List<EndlessEnemyGrowth> endlessGrowth = new();

    public float WaveDuration => waveDuration;
    public float IntervalBetweenWaves => intervalBetweenWaves;
    public EnemyCatalog EnemyCatalog => enemyCatalog;
    public WaveSpawnTiming SpawnTiming => spawnTiming;
    public IReadOnlyList<WaveData> Waves => waves;
    public IReadOnlyList<EndlessEnemyGrowth> EndlessGrowth => endlessGrowth;

    /// <param name="waveNumber">1始まりの通算Wave番号。</param>
    public WaveData GetWave(int waveNumber)
    {
        if (waves == null || waves.Count == 0)
        {
            return null;
        }

        return waves[Mathf.Clamp(waveNumber - 1, 0, waves.Count - 1)];
    }

    /// <summary>
    /// 通常WaveはそのWaveの指定数だけを使用する。
    /// 設定終了後は最終Waveの基礎数に、別設定の増加分を足す。
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

        int wavesAfterFinal = waveNumber - waves.Count;
        if (wavesAfterFinal <= 0)
        {
            return;
        }

        foreach (EndlessEnemyGrowth growth in endlessGrowth)
        {
            if (growth != null)
            {
                AddCount(counts, growth.EnemyId, growth.GetAdditionalCount(wavesAfterFinal));
            }
        }
    }

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
