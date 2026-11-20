using UnityEngine;

/// <summary>
/// 敵1体の生成時に確定し、その個体が生存する間は変化しない倍率
/// </summary>
public readonly struct EnemyStatScale
{
    public static EnemyStatScale Default =>
        new(1f, 1f, EnemyStatRoundingMode.Nearest);

    public float HealthMultiplier { get; }
    public float DamageMultiplier { get; }
    public EnemyStatRoundingMode RoundingMode { get; }

    /// <summary>
    /// 生成時に倍率をもらう
    /// </summary>
    /// <param name="healthMultiplier">体力倍率</param>
    /// <param name="damageMultiplier">ダメージ倍率</param>
    /// <param name="roundingMode">倍率適用後の値を整数に丸める方法</param>
    public EnemyStatScale(
        float healthMultiplier,
        float damageMultiplier,
        EnemyStatRoundingMode roundingMode)
    {
        HealthMultiplier = Mathf.Max(0f, healthMultiplier);
        DamageMultiplier = Mathf.Max(0f, damageMultiplier);
        RoundingMode = roundingMode;
    }

    public int ScaleHealth(int baseValue) => Scale(baseValue, HealthMultiplier, 1);
    public int ScaleDamage(int baseValue) => Scale(baseValue, DamageMultiplier, 0);


    /// <summary>
    /// 
    /// </summary>
    /// <param name="baseValue"></param>
    /// <param name="multiplier"></param>
    /// <param name="minimum"></param>
    /// <returns></returns>
    private int Scale(int baseValue, float multiplier, int minimum)
    {
        float value = Mathf.Max(0, baseValue) * multiplier;
        if (!float.IsFinite(value) || value >= int.MaxValue)
        {
            return int.MaxValue;
        }

        int rounded = RoundingMode switch
        {
            EnemyStatRoundingMode.RoundDown => Mathf.FloorToInt(value),
            EnemyStatRoundingMode.RoundUp => Mathf.CeilToInt(value),
            _ => Mathf.RoundToInt(value)
        };
        return Mathf.Max(minimum, rounded);
    }
}

public enum EnemyStatRoundingMode
{
    [InspectorName("最も近い整数")]
    Nearest = 0,
    [InspectorName("切り捨て")]
    RoundDown = 1,
    [InspectorName("切り上げ")]
    RoundUp = 2
}
