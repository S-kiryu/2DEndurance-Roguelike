using UnityEngine;

/// <summary>武器1個の星と、初期化されるまで累積する強化値を保持する。</summary>
internal sealed class WeaponUpgradeState
{
    public int StarLevel { get; private set; } = 1;
    public int PostMaxDamageUpgradeCount { get; private set; }
    public float DamageMultiplierBonus { get; private set; }
    public float AttackSpeedMultiplierBonus { get; private set; }
    public float CriticalRateBonus { get; private set; }
    public float RangeMultiplierBonus { get; private set; }
    public float ProjectileSpeedMultiplierBonus { get; private set; }
    public int PenetrationBonus { get; private set; }
    public int AdditionalHits { get; private set; }
    public int AdditionalProjectiles { get; private set; }
    public float SpreadAngleBonus { get; private set; }

    public void ApplyStage(WeaponUpgradeStage stage)
    {
        ApplyStatBonus(WeaponUpgradeStat.Damage, stage.DamageMultiplierBonus);
        ApplyStatBonus(WeaponUpgradeStat.AttackSpeed, stage.AttackSpeedMultiplierBonus);
        ApplyStatBonus(WeaponUpgradeStat.CriticalRate, stage.CriticalRateBonus);
        ApplyStatBonus(WeaponUpgradeStat.Range, stage.RangeMultiplierBonus);
        ApplyStatBonus(WeaponUpgradeStat.ProjectileSpeed, stage.ProjectileSpeedMultiplierBonus);
        PenetrationBonus += stage.PenetrationBonus;
        StarLevel = stage.StarLevel;
    }

    public void ApplyPostMaxDamageUpgrade(float amount)
    {
        ApplyStatBonus(WeaponUpgradeStat.Damage, amount);
        PostMaxDamageUpgradeCount++;
    }

    public void ApplyStatBonus(WeaponUpgradeStat stat, float amount)
    {
        amount = Mathf.Max(0f, amount);
        switch (stat)
        {
            case WeaponUpgradeStat.Damage: DamageMultiplierBonus += amount; break;
            case WeaponUpgradeStat.AttackSpeed: AttackSpeedMultiplierBonus += amount; break;
            case WeaponUpgradeStat.CriticalRate: CriticalRateBonus += amount; break;
            case WeaponUpgradeStat.Range: RangeMultiplierBonus += amount; break;
            case WeaponUpgradeStat.ProjectileSpeed: ProjectileSpeedMultiplierBonus += amount; break;
            case WeaponUpgradeStat.Penetration: PenetrationBonus += Mathf.RoundToInt(amount); break;
            case WeaponUpgradeStat.AdditionalHits: AdditionalHits += Mathf.RoundToInt(amount); break;
            case WeaponUpgradeStat.AdditionalProjectiles: AdditionalProjectiles += Mathf.RoundToInt(amount); break;
            case WeaponUpgradeStat.SpreadAngle: SpreadAngleBonus += amount; break;
        }
    }
}
