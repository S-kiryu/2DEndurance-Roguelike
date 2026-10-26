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
    private GameObject enemyPrefab;

    [Min(1)]
    [SerializeField]
    private int count = 1;

    public GameObject EnemyPrefab => enemyPrefab;
    public int Count => count;
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
    [Header("全Wave共通設定")]
    [Min(0.1f)]
    [SerializeField]
    private float waveDuration = 30f;

    [Min(0f)]
    [SerializeField]
    private float intervalBetweenWaves = 5f;

    [Tooltip("1ループ終了ごとに掛ける倍率。1なら変化なし")]
    [SerializeField, Min(1f)]
    private float multiplierPerLoop = 1.2f;

    [Header("Waveごとの敵構成")]
    [SerializeField]
    private List<WaveData> waves = new();

    public float WaveDuration => waveDuration;
    public float IntervalBetweenWaves => intervalBetweenWaves;
    public float MultiplierPerLoop => multiplierPerLoop;
    public IReadOnlyList<WaveData> Waves => waves;
}
