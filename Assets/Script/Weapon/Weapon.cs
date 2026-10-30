using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器のベースクラス
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponData data;

    [Header("武器固有の攻撃強化")]
    [Tooltip("0.2で武器の攻撃倍率+20%。プレイヤー側の倍率とは掛け合わせます。")]
    [SerializeField, Min(0f)] private float damageMultiplierBonus;

    private float cooldownRemaining;
    private float upgradeDamageMultiplierBonus;
    private float upgradeAttackSpeedMultiplierBonus;
    private float upgradeCriticalRateBonus;
    private float upgradeRangeMultiplierBonus;
    private float upgradeProjectileSpeedMultiplierBonus;
    private int upgradePenetrationBonus;
    private int additionalHits;
    private int additionalProjectiles;
    private float spreadAngleBonus;
    private readonly List<WeaponEffect> effects = new();
    private WeaponEffect[] effectSnapshot = Array.Empty<WeaponEffect>();
    public WeaponContext Context { get; private set; }
    public int AdditionalHits => additionalHits + Mathf.RoundToInt(GetEffectBonus(WeaponUpgradeStat.AdditionalHits));
    public int AdditionalProjectiles => additionalProjectiles + Mathf.RoundToInt(GetEffectBonus(WeaponUpgradeStat.AdditionalProjectiles));
    public float SpreadAngleBonus => spreadAngleBonus + GetEffectBonus(WeaponUpgradeStat.SpreadAngle);

    protected WeaponData Data => data;
    protected WeaponStats Stats => data.Stats;
    protected GameObjectPool ObjectPool { get; private set; }
    protected virtual bool RequiresPool => false;

    public WeaponData WeaponData => data;
    public string DisplayName => data != null ? data.Stats.WeaponName : name;
    public int StarLevel { get; private set; } = 1;
    public int PostMaxDamageUpgradeCount { get; private set; }
    public int MaxStarLevel => data != null ? data.MaxStarLevel : 1;
    public bool IsAtMaxStar => StarLevel >= MaxStarLevel;
    public bool CanUpgradeStar => data != null && !IsAtMaxStar &&
        data.GetUpgradeForStar(StarLevel + 1) != null;
    public bool CanUpgradePostMaxDamage => data != null && IsAtMaxStar;

    public PlayerModel OwnerStats { get; private set; }
    public float WeaponDamageMultiplier =>
        Mathf.Max(0f, 1f + damageMultiplierBonus + upgradeDamageMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.Damage));
    public float WeaponAttackSpeedMultiplier => Mathf.Max(0.01f, 1f + upgradeAttackSpeedMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.AttackSpeed));
    public float WeaponRangeMultiplier => Mathf.Max(0f, 1f + upgradeRangeMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.Range));
    public float WeaponProjectileSpeedMultiplier => Mathf.Max(0f, 1f + upgradeProjectileSpeedMultiplierBonus + GetEffectBonus(WeaponUpgradeStat.ProjectileSpeed));
    public int WeaponPenetrationBonus => upgradePenetrationBonus + Mathf.RoundToInt(GetEffectBonus(WeaponUpgradeStat.Penetration));
    public float EffectiveDamageMultiplier => WeaponDamageMultiplier * (OwnerStats?.AttackMultiplier ?? 1f);
    public float EffectiveAttackInterval => Stats.AttackInterval /
        ((OwnerStats?.AttackSpeedMultiplier ?? 1f) * WeaponAttackSpeedMultiplier);
    public float EffectiveCriticalRate => Mathf.Clamp01(
        Stats.CriticalRate + upgradeCriticalRateBonus + GetEffectBonus(WeaponUpgradeStat.CriticalRate) + (OwnerStats?.CriticalRateBonus ?? 0f));

    internal void RegisterEffect(WeaponEffect effect)
    {
        if (!effects.Contains(effect))
        {
            effects.Add(effect);
            effectSnapshot = effects.ToArray();
        }
        effect.Bind(Context);
    }

    internal void UnregisterEffect(WeaponEffect effect)
    {
        if (effects.Remove(effect)) effectSnapshot = effects.ToArray();
        effect.Bind(null);
    }

    private float GetEffectBonus(WeaponUpgradeStat stat)
    {
        float bonus = 0f;
        foreach (WeaponEffect effect in effectSnapshot)
            if (effect != null && effect.IsEffectActive) bonus += effect.GetStatBonus(stat);
        return bonus;
    }

    private void ForEachEffect(Action<WeaponEffect> action)
    {
        foreach (WeaponEffect effect in effectSnapshot)
        {
            if (effect == null) continue;
            effect.RefreshActivation();
            if (effect.IsEffectActive) action(effect);
        }
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
        foreach (WeaponEffect effect in effectSnapshot)
            if (effect != null) effect.RefreshActivation();
    }

    protected virtual void OnDisable()
    {
        foreach (WeaponEffect effect in effectSnapshot)
            if (effect != null) effect.RefreshActivation();
    }

    protected virtual void OnDestroy()
    {
        foreach (WeaponEffect effect in effectSnapshot)
            if (effect != null) effect.Bind(null);
        Context = null;
        effects.Clear();
        effectSnapshot = Array.Empty<WeaponEffect>();
    }

    public bool AddDamageMultiplierBonus(float additionalMultiplier)
    {
        if (additionalMultiplier <= 0f)
        {
            return false;
        }
        damageMultiplierBonus += additionalMultiplier;
        return true;
    }

    public WeaponUpgradeStage GetNextStarUpgrade()
    {
        return CanUpgradeStar ? data.GetUpgradeForStar(StarLevel + 1) : null;
    }

    public bool ApplyNextStarUpgrade()
    {
        WeaponUpgradeStage stage = GetNextStarUpgrade();
        if (stage == null)
        {
            return false;
        }

        upgradeDamageMultiplierBonus += stage.DamageMultiplierBonus;
        upgradeAttackSpeedMultiplierBonus += stage.AttackSpeedMultiplierBonus;
        upgradeCriticalRateBonus += stage.CriticalRateBonus;
        upgradeRangeMultiplierBonus += stage.RangeMultiplierBonus;
        upgradeProjectileSpeedMultiplierBonus += stage.ProjectileSpeedMultiplierBonus;
        upgradePenetrationBonus += stage.PenetrationBonus;
        StarLevel = stage.StarLevel;

        if (stage.Effects != null)
        {
            foreach (WeaponUpgradeEffect effect in stage.Effects)
            {
                if (effect != null && effect.CanApplyTo(data))
                {
                    effect.Apply(this);
                }
            }
        }

        OnStarLevelChanged(StarLevel);
        foreach (WeaponEffect effect in effectSnapshot)
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

        upgradeDamageMultiplierBonus += data.PostMaxDamageMultiplierBonus;
        PostMaxDamageUpgradeCount++;
        Debug.Log(
            $"{DisplayName} ★5後攻撃強化 +{PostMaxDamageUpgradeCount} " +
            $"（攻撃倍率 {WeaponDamageMultiplier:F2}倍）",
            this
        );
        return true;
    }

    public void ApplyStatBonus(WeaponUpgradeStat stat, float amount)
    {
        amount = Mathf.Max(0f, amount);
        switch (stat)
        {
            case WeaponUpgradeStat.Damage: upgradeDamageMultiplierBonus += amount; break;
            case WeaponUpgradeStat.AttackSpeed: upgradeAttackSpeedMultiplierBonus += amount; break;
            case WeaponUpgradeStat.CriticalRate: upgradeCriticalRateBonus += amount; break;
            case WeaponUpgradeStat.Range: upgradeRangeMultiplierBonus += amount; break;
            case WeaponUpgradeStat.ProjectileSpeed: upgradeProjectileSpeedMultiplierBonus += amount; break;
            case WeaponUpgradeStat.Penetration: upgradePenetrationBonus += Mathf.RoundToInt(amount); break;
            case WeaponUpgradeStat.AdditionalHits: additionalHits += Mathf.RoundToInt(amount); break;
            case WeaponUpgradeStat.AdditionalProjectiles: additionalProjectiles += Mathf.RoundToInt(amount); break;
            case WeaponUpgradeStat.SpreadAngle: spreadAngleBonus += amount; break;
        }
    }

    /// <summary>
    /// クールタイムが経過したかどうかを確認し、攻撃可能であれば攻撃を行う
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Tick(float deltaTime)
    {
        if (Context == null || !isActiveAndEnabled || data == null || deltaTime <= 0f)
        {
            return;
        }

        foreach (WeaponEffect effect in effectSnapshot)
        {
            if (effect == null) continue;
            effect.RefreshActivation();
            if (effect.IsEffectActive) effect.OnTick(deltaTime);
        }
        // 基準の攻撃間隔を現在の攻撃速度で消費し、待機中の強化も直ちに反映する。
        float attackSpeedMultiplier =
            (OwnerStats?.AttackSpeedMultiplier ?? 1f) * WeaponAttackSpeedMultiplier;
        cooldownRemaining = Mathf.Max(cooldownRemaining - deltaTime * attackSpeedMultiplier, 0f);

        if (cooldownRemaining <= 0f && TryAttack())
        {
            cooldownRemaining = Stats.AttackInterval;
        }
    }

    public bool Initialize(WeaponData weaponData = null, PlayerModel ownerStats = null, GameObjectPool objectPool = null)
    {
        if (weaponData != null)
        {
            data = weaponData;
        }

        if (data == null)
        {
            Debug.LogError($"{name}: WeaponDataが設定されていません。", this);
            return false;
        }

        if (!IsCompatibleData(data))
        {
            Debug.LogError(
                $"{name}: {GetType().Name}に対応していないWeaponData ({data.GetType().Name}) です。",
                this
            );
            return false;
        }

        if (RequiresPool && objectPool == null)
        {
            Debug.LogError($"{name}: WeaponManagerにシーン上のGameObjectPoolを設定してください。", this);
            return false;
        }

        ObjectPool = objectPool;
        OwnerStats = ownerStats;
        foreach (WeaponEffect effect in effectSnapshot)
            if (effect != null) effect.Bind(null);
        Context = null;
        cooldownRemaining = 0f;
        StarLevel = 1;
        PostMaxDamageUpgradeCount = 0;
        upgradeDamageMultiplierBonus = 0f;
        upgradeAttackSpeedMultiplierBonus = 0f;
        upgradeCriticalRateBonus = 0f;
        upgradeRangeMultiplierBonus = 0f;
        upgradeProjectileSpeedMultiplierBonus = 0f;
        upgradePenetrationBonus = 0;
        additionalHits = 0;
        additionalProjectiles = 0;
        spreadAngleBonus = 0f;
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
}
