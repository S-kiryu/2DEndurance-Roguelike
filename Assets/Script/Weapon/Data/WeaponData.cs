using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class WeaponUpgradeStage
{
    public int StarLevel => Mathf.Clamp(_starLevel, 2, 5);
    public string DisplayName => _displayName;
    public string Description => _description;
    public float DamageMultiplierBonus => Mathf.Max(0f, _damageMultiplierBonus);
    public float AttackSpeedMultiplierBonus => Mathf.Max(0f, _attackSpeedMultiplierBonus);
    public float CriticalRateBonus => Mathf.Max(0f, _criticalRateBonus);
    public float RangeMultiplierBonus => Mathf.Max(0f, _rangeMultiplierBonus);
    public float ProjectileSpeedMultiplierBonus => Mathf.Max(0f, _projectileSpeedMultiplierBonus);
    public int PenetrationBonus => Mathf.Max(0, _penetrationBonus);
    public IReadOnlyList<WeaponUpgradeEffect> Effects => _effects;

    [SerializeField, Range(2, 5)] private int _starLevel = 2;
    [SerializeField] private string _displayName;
    [SerializeField, TextArea(2, 5)] private string _description;

    [Header("この段階で加算する性能")]
    [Tooltip("0.2で攻撃力+20%")]
    [SerializeField, Min(0f)] private float _damageMultiplierBonus;
    [Tooltip("0.2で攻撃速度+20%")]
    [SerializeField, Min(0f)] private float _attackSpeedMultiplierBonus;
    [Tooltip("0.05でクリティカル率+5%")]
    [SerializeField, Min(0f)] private float _criticalRateBonus;
    [Tooltip("0.2で攻撃範囲+20%")]
    [SerializeField, Min(0f)] private float _rangeMultiplierBonus;
    [Tooltip("0.2で弾速+20%")]
    [SerializeField, Min(0f)] private float _projectileSpeedMultiplierBonus;
    [SerializeField, Min(0)] private int _penetrationBonus;

    [Header("追加効果")]
    [SerializeField] private List<WeaponUpgradeEffect> _effects = new();
}

/// <summary>
/// 武器の抽象データクラス
/// </summary>
public abstract class WeaponData : ScriptableObject
{
    public WeaponStats Stats => _stats;
    public Weapon WeaponPrefab => _weaponPrefab;
    public bool OrbitEnabled => _orbitEnabled;
    public int MaxStarLevel => Mathf.Clamp(_stats.MaxEnchantLevel, 1, 5);
    public float PostMaxDamageMultiplierBonus => Mathf.Max(0.001f, _postMaxDamageMultiplierBonus);

    public WeaponUpgradeStage GetUpgradeForStar(int starLevel)
    {
        if (_upgradeStages == null)
        {
            return null;
        }

        foreach (WeaponUpgradeStage stage in _upgradeStages)
        {
            if (stage != null && stage.StarLevel == starLevel)
            {
                return stage;
            }
        }

        return null;
    }

    [SerializeField]
    private WeaponStats _stats = new();

    [SerializeField]
    private Weapon _weaponPrefab;

    [Tooltip("オンでプレイヤーの周りを公転し、オフで装備位置に固定します。")]
    [SerializeField]
    private bool _orbitEnabled = true;

    [Header("星2～5の固定強化")]
    [Tooltip("星2～最大星を1つずつ登録します。固有効果は各段階の追加効果に登録します。")]
    [SerializeField]
    private List<WeaponUpgradeStage> _upgradeStages = new();

    [Header("星5到達後")]
    [Tooltip("繰り返し取得するたびに加算する攻撃倍率。0.15で+15%")]
    [SerializeField, Min(0.001f)]
    private float _postMaxDamageMultiplierBonus = 0.15f;
}
