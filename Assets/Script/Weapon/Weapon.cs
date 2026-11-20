using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器のベースクラス
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    public WeaponContext Context { get; private set; }
    public int AdditionalHits =>
        _upgrades.AdditionalHits + Mathf.RoundToInt(GetEffectBonus(WeaponUpgradeStat.AdditionalHits));
    public int AdditionalProjectiles =>
        _upgrades.AdditionalProjectiles + Mathf.RoundToInt(GetEffectBonus(WeaponUpgradeStat.AdditionalProjectiles));
    public float SpreadAngleBonus => _upgrades.SpreadAngleBonus + GetEffectBonus(WeaponUpgradeStat.SpreadAngle);

    public WeaponData WeaponData => _data;
    public string DisplayName => _data != null ? _data.Stats.WeaponName : name;
    public int StarLevel => _upgrades.StarLevel;
    public int PostMaxDamageUpgradeCount => _upgrades.PostMaxDamageUpgradeCount;
    public int MaxStarLevel => _data != null ? _data.MaxStarLevel : 1;
    public bool IsAtMaxStar => StarLevel >= MaxStarLevel;
    public bool CanUpgradeStar => GetNextStarUpgrade() != null;
    public bool CanUpgradePostMaxDamage => _data != null && IsAtMaxStar;

    public PlayerModel OwnerStats { get; private set; }
    public float WeaponDamageMultiplier =>
        Mathf.Max(0f, 1f + _damageMultiplierBonus + _upgrades.DamageMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.Damage));
    public float WeaponAttackSpeedMultiplier =>
        Mathf.Max(0.01f, 1f + _upgrades.AttackSpeedMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.AttackSpeed));
    public float WeaponRangeMultiplier =>
        Mathf.Max(0f, 1f + _upgrades.RangeMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.Range));
    public float WeaponProjectileSpeedMultiplier =>
        Mathf.Max(0f, 1f + _upgrades.ProjectileSpeedMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.ProjectileSpeed));
    public int WeaponPenetrationBonus =>
        _upgrades.PenetrationBonus + Mathf.RoundToInt(GetEffectBonus(WeaponUpgradeStat.Penetration));
    public float EffectiveDamageMultiplier => WeaponDamageMultiplier * (OwnerStats?.AttackMultiplier ?? 1f);
    public float EffectiveAttackInterval => Stats.AttackInterval / EffectiveAttackSpeedMultiplier;
    public float EffectiveCriticalRate => Mathf.Clamp01(
        Stats.CriticalRate + _upgrades.CriticalRateBonus +
        GetEffectBonus(WeaponUpgradeStat.CriticalRate) + (OwnerStats?.CriticalRateBonus ?? 0f));

    public bool AddDamageMultiplierBonus(float additionalMultiplier)
    {
        if (additionalMultiplier <= 0f)
        {
            return false;
        }
        _damageMultiplierBonus += additionalMultiplier;
        return true;
    }

    public WeaponUpgradeStage GetNextStarUpgrade()
    {
        return _data != null && !IsAtMaxStar ? _data.GetUpgradeForStar(StarLevel + 1) : null;
    }

    public bool ApplyNextStarUpgrade()
    {
        WeaponUpgradeStage stage = GetNextStarUpgrade();
        if (stage == null)
        {
            return false;
        }

        _upgrades.ApplyStage(stage);

        if (stage.Effects != null)
        {
            foreach (WeaponUpgradeEffect effect in stage.Effects)
            {
                if (effect != null && effect.CanApplyTo(_data))
                {
                    effect.Apply(this);
                }
            }
        }

        OnStarLevelChanged(StarLevel);
        foreach (WeaponEffect effect in _effectSnapshot)
            if (effect != null) effect.RefreshActivation();
        Debug.Log($"{DisplayName}が★{StarLevel}になりました。", this);
        return true;
    }

    public bool ApplyPostMaxDamageUpgrade()
    {
        if (!CanUpgradePostMaxDamage)
        {
            return false;
        }

        _upgrades.ApplyPostMaxDamageUpgrade(_data.PostMaxDamageMultiplierBonus);
        Debug.Log(
            $"{DisplayName} ★{MaxStarLevel}後攻撃強化 +{PostMaxDamageUpgradeCount} " +
            $"（攻撃倍率 {WeaponDamageMultiplier:F2}倍）",
            this
        );
        return true;
    }

    public void ApplyStatBonus(WeaponUpgradeStat stat, float amount)
    {
        _upgrades.ApplyStatBonus(stat, amount);
    }

    /// <summary>
    /// クールタイムが経過したかどうかを確認し、攻撃可能であれば攻撃を行う
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Tick(float deltaTime)
    {
        if (Context == null || !isActiveAndEnabled || _data == null || deltaTime <= 0f)
        {
            return;
        }

        foreach (WeaponEffect effect in _effectSnapshot)
        {
            if (effect == null) continue;
            effect.RefreshActivation();
            if (effect.IsEffectActive) effect.OnTick(deltaTime);
        }
        // 基準の攻撃間隔を現在の攻撃速度で消費し、待機中の強化も直ちに反映する。
        _cooldownRemaining = Mathf.Max(_cooldownRemaining - deltaTime * EffectiveAttackSpeedMultiplier, 0f);

        if (_cooldownRemaining <= 0f && TryAttack())
        {
            _cooldownRemaining = Stats.AttackInterval;
        }
    }

    public bool Initialize(WeaponData weaponData = null, PlayerModel ownerStats = null, GameObjectPool objectPool = null)
    {
        WeaponData targetData = weaponData != null ? weaponData : _data;
        string error = GetInitializationError(targetData, objectPool);
        if (error != null)
        {
            Debug.LogError($"{name}: {error}", this);
            return false;
        }

        _data = targetData;
        ObjectPool = objectPool;
        OwnerStats = ownerStats;
        foreach (WeaponEffect effect in _effectSnapshot)
            if (effect != null) effect.Bind(null);
        Context = null;
        _cooldownRemaining = 0f;
        _upgrades = new WeaponUpgradeState();
        OnInitialized();
        Context = new WeaponContext(this, ownerStats, objectPool);
        foreach (WeaponEffect effect in GetComponentsInChildren<WeaponEffect>(true))
            if (effect.isActiveAndEnabled && effect.GetComponentInParent<Weapon>() == this) RegisterEffect(effect);
        return true;
    }

    /// <summary>
    /// つける
    /// </summary>
    /// <param name="mount"></param>
    public void Equip(Transform mount)
    {
        transform.SetParent(mount, false);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 外す
    /// </summary>
    public void Unequip()
    {
        gameObject.SetActive(false);
    }

    protected WeaponData Data => _data;
    protected WeaponStats Stats => _data.Stats;
    protected GameObjectPool ObjectPool { get; private set; }
    protected virtual bool RequiresPool => false;
    protected float EffectiveAttackSpeedMultiplier =>
        (OwnerStats?.AttackSpeedMultiplier ?? 1f) * WeaponAttackSpeedMultiplier;

    internal void RegisterEffect(WeaponEffect effect)
    {
        if (!_effects.Contains(effect))
        {
            _effects.Add(effect);
            _effectSnapshot = _effects.ToArray();
        }
        effect.Bind(Context);
    }

    internal void UnregisterEffect(WeaponEffect effect)
    {
        if (_effects.Remove(effect)) _effectSnapshot = _effects.ToArray();
        effect.Bind(null);
    }

    protected void NotifyAttackStarted() => ForEachEffect(effect => effect.OnAttackStarted());
    protected void NotifyAttackFinished() => ForEachEffect(effect => effect.OnAttackFinished());
    internal void NotifyHit(WeaponHit hit)
    {
        ForEachEffect(effect => effect.OnHit(hit));
        if (hit.WasKilled) ForEachEffect(effect => effect.OnKill(hit));
    }

    protected virtual void OnEnable()
    {
        foreach (WeaponEffect effect in _effectSnapshot)
            if (effect != null) effect.RefreshActivation();
    }

    protected virtual void OnDisable()
    {
        foreach (WeaponEffect effect in _effectSnapshot)
            if (effect != null) effect.RefreshActivation();
    }

    protected virtual void OnDestroy()
    {
        foreach (WeaponEffect effect in _effectSnapshot)
            if (effect != null) effect.Bind(null);
        Context = null;
        _effects.Clear();
        _effectSnapshot = Array.Empty<WeaponEffect>();
    }

    internal string GetInitializationError(WeaponData weaponData, GameObjectPool objectPool)
    {
        if (weaponData == null)
            return "WeaponDataが設定されていません。";
        if (!IsCompatibleData(weaponData))
            return $"{GetType().Name}に対応していないWeaponData ({weaponData.GetType().Name}) です。";
        if (RequiresPool && objectPool == null)
            return "WeaponManagerにシーン上のGameObjectPoolを設定してください。";

        return GetDataError(weaponData);
    }

    protected virtual string GetDataError(WeaponData weaponData) => null;

    /// <summary>
    /// ダメージ計算
    /// </summary>
    /// <returns></returns>
    protected int CalculateDamage()
    {
        float criticalRate = EffectiveCriticalRate;
        bool isCritical = criticalRate >= 1f || UnityEngine.Random.value < criticalRate;
        float multiplier = isCritical ? Stats.CriticalMultiplier : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(Stats.Damage * EffectiveDamageMultiplier * multiplier));
    }

    protected virtual void OnInitialized()
    {
    }

    protected virtual void OnStarLevelChanged(int starLevel)
    {
    }

    protected abstract bool IsCompatibleData(WeaponData weaponData);

    /// <summary>攻撃できた場合だけtrueを返す。</summary>
    protected abstract bool TryAttack();

    [SerializeField]
    private WeaponData _data;

    [Header("武器固有の攻撃強化")]
    [Tooltip("0.2で武器の攻撃倍率+20%。プレイヤー側の倍率とは掛け合わせます。")]
    [SerializeField, Min(0f)] private float _damageMultiplierBonus;

    private WeaponUpgradeState _upgrades = new();
    private float _cooldownRemaining;
    private readonly List<WeaponEffect> _effects = new();
    private WeaponEffect[] _effectSnapshot = Array.Empty<WeaponEffect>();

    private float GetEffectBonus(WeaponUpgradeStat stat)
    {
        float bonus = 0f;
        foreach (WeaponEffect effect in _effectSnapshot)
            if (effect != null && effect.IsEffectActive) bonus += effect.GetStatBonus(stat);
        return bonus;
    }

    private void ForEachEffect(Action<WeaponEffect> action)
    {
        foreach (WeaponEffect effect in _effectSnapshot)
        {
            if (effect == null) continue;
            effect.RefreshActivation();
            if (effect.IsEffectActive) action(effect);
        }
    }
}
