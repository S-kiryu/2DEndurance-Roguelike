using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class WeaponUpgradeStage
{
    [SerializeField, Range(2, 5)] private int starLevel = 2;
    [SerializeField] private string displayName;
    [SerializeField, TextArea(2, 5)] private string description;

    [Header("この段階で加算する性能")]
    [Tooltip("0.2で攻撃力+20%")]
    [SerializeField, Min(0f)] private float damageMultiplierBonus;
    [Tooltip("0.2で攻撃速度+20%")]
    [SerializeField, Min(0f)] private float attackSpeedMultiplierBonus;
    [Tooltip("0.05でクリティカル率+5ポイント")]
    [SerializeField, Min(0f)] private float criticalRateBonus;
    [Tooltip("0.2で攻撃範囲+20%")]
    [SerializeField, Min(0f)] private float rangeMultiplierBonus;
    [Tooltip("0.2で弾速+20%")]
    [SerializeField, Min(0f)] private float projectileSpeedMultiplierBonus;
    [SerializeField, Min(0)] private int penetrationBonus;

    [Header("追加効果（登録した順に適用）")]
    [Tooltip("ProjectのCreate > Weapons > Effectsから作成した効果を登録。星5以外にも設定できます。")]
    [SerializeField] private List<WeaponUpgradeEffect> effects = new();

    public int StarLevel => Mathf.Clamp(starLevel, 2, 5);
    public string DisplayName => displayName;
    public string Description => description;
    public float DamageMultiplierBonus => Mathf.Max(0f, damageMultiplierBonus);
    public float AttackSpeedMultiplierBonus => Mathf.Max(0f, attackSpeedMultiplierBonus);
    public float CriticalRateBonus => Mathf.Max(0f, criticalRateBonus);
    public float RangeMultiplierBonus => Mathf.Max(0f, rangeMultiplierBonus);
    public float ProjectileSpeedMultiplierBonus => Mathf.Max(0f, projectileSpeedMultiplierBonus);
    public int PenetrationBonus => Mathf.Max(0, penetrationBonus);
    public IReadOnlyList<WeaponUpgradeEffect> Effects => effects;
}

/// <summary>
/// 武器の抽象データクラス
/// </summary>
public abstract class WeaponData : ScriptableObject
{
    [SerializeField]
    private WeaponStats stats = new();

    [SerializeField]
    private Weapon weaponPrefab;

    [Header("星2～5の固定強化")]
    [Tooltip("星2～最大星を1つずつ登録します。固有効果は各段階の追加効果に登録します。")]
    [SerializeField]
    private List<WeaponUpgradeStage> upgradeStages = new();

    [Header("星5到達後")]
    [Tooltip("繰り返し取得するたびに加算する攻撃倍率。0.15で+15%")]
    [SerializeField, Min(0.001f)]
    private float postMaxDamageMultiplierBonus = 0.15f;

    public WeaponStats Stats => stats;
    public Weapon WeaponPrefab => weaponPrefab;
    public int MaxStarLevel => Mathf.Clamp(stats.MaxEnchantLevel, 1, 5);
    public float PostMaxDamageMultiplierBonus => Mathf.Max(0.001f, postMaxDamageMultiplierBonus);

    public WeaponUpgradeStage GetUpgradeForStar(int starLevel)
    {
        if (upgradeStages == null)
        {
            return null;
        }

        foreach (WeaponUpgradeStage stage in upgradeStages)
        {
            if (stage != null && stage.StarLevel == starLevel)
            {
                return stage;
            }
        }

        return null;
    }
}
