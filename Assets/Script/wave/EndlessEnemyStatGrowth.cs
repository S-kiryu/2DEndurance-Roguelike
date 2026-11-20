using System;
using UnityEngine;

/// <summary>2周目以降に、新しく出現する敵へ適用する戦闘ステータス成長。</summary>
[Serializable]
public sealed class EndlessEnemyStatGrowth
{
    public bool Enabled => _enabled;
    public int IncreaseEveryWaves => Mathf.Max(1, _increaseEveryWaves);
    public float HealthIncreasePerStep => Mathf.Max(0f, _healthIncreasePerStep);
    public float DamageIncreasePerStep => Mathf.Max(0f, _damageIncreasePerStep);
    public EnemyStatGrowthMode GrowthMode => _growthMode;
    public EnemyStatRoundingMode RoundingMode => _roundingMode;

    public EnemyStatScale GetScale(int wavesAfterFinal)
    {
        if (!_enabled || wavesAfterFinal <= 0)
        {
            return EnemyStatScale.Default;
        }

        int steps = wavesAfterFinal / IncreaseEveryWaves;
        if (steps <= 0)
        {
            return EnemyStatScale.Default;
        }

        float healthMultiplier = GetMultiplier(HealthIncreasePerStep, steps);
        float damageMultiplier = GetMultiplier(DamageIncreasePerStep, steps);
        return new EnemyStatScale(healthMultiplier, damageMultiplier, _roundingMode);
    }

    [SerializeField]
    private bool _enabled;

    [Tooltip("1周目の終了から何Wave進むごとに倍率を1段階上げるか")]
    [SerializeField, Min(1)]
    private int _increaseEveryWaves = 1;

    [Tooltip("1段階ごとの最大HP増加率。0.15で+15%")]
    [SerializeField, Min(0f)]
    private float _healthIncreasePerStep = 0.15f;

    [Tooltip("1段階ごとの攻撃力増加率。0.1で+10%")]
    [SerializeField, Min(0f)]
    private float _damageIncreasePerStep = 0.1f;

    [SerializeField]
    private EnemyStatGrowthMode _growthMode = EnemyStatGrowthMode.Linear;

    [InspectorName("整数への丸め方")]
    [Tooltip("倍率適用後の最大HP・攻撃力に適用します。「最も近い整数」では、ちょうど中間の値は偶数側に丸めます。")]
    [SerializeField]
    private EnemyStatRoundingMode _roundingMode = EnemyStatRoundingMode.Nearest;

    private float GetMultiplier(float increase, int steps)
    {
        return _growthMode == EnemyStatGrowthMode.Compound
            ? Mathf.Pow(1f + increase, steps)
            : 1f + increase * steps;
    }
}

public enum EnemyStatGrowthMode
{
    Linear,
    Compound
}
