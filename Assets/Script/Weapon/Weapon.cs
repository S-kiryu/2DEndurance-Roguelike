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

    protected WeaponData Data => data;
    protected WeaponStats Stats => data.Stats;
    protected GameObjectPool ObjectPool { get; private set; }
    protected virtual bool RequiresPool => false;

    public WeaponData WeaponData => data;
    public string DisplayName => data != null ? data.Stats.WeaponName : name;
    public int DebugRarityLevel { get; private set; }

    public PlayerModel OwnerStats { get; private set; }
    public float WeaponDamageMultiplier => 1f + damageMultiplierBonus;
    public float EffectiveDamageMultiplier => WeaponDamageMultiplier * (OwnerStats?.AttackMultiplier ?? 1f);
    public float EffectiveAttackInterval => Stats.AttackInterval / (OwnerStats?.AttackSpeedMultiplier ?? 1f);
    public float EffectiveCriticalRate => Mathf.Clamp01(Stats.CriticalRate + (OwnerStats?.CriticalRateBonus ?? 0f));

    public bool AddDamageMultiplierBonus(float additionalMultiplier)
    {
        if (additionalMultiplier <= 0f)
        {
            return false;
        }
        damageMultiplierBonus += additionalMultiplier;
        return true;
    }

    /// <summary>正式なレアリティ実装までの仮状態。戦闘性能には影響しない。</summary>
    public void UpgradeDebugRarity()
    {
        int previousLevel = DebugRarityLevel;
        DebugRarityLevel++;
        Debug.Log($"[仮レアリティ] {DisplayName}: +{previousLevel} → +{DebugRarityLevel}（性能変更なし）", this);
    }

    /// <summary>
    /// クールタイムが経過したかどうかを確認し、攻撃可能であれば攻撃を行う
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Tick(float deltaTime)
    {
        if (data == null || deltaTime <= 0f)
        {
            return;
        }

        // 基準の攻撃間隔を現在の攻撃速度で消費し、待機中の強化も直ちに反映する。
        float attackSpeedMultiplier = OwnerStats?.AttackSpeedMultiplier ?? 1f;
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
        cooldownRemaining = 0f;
        OnInitialized();
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
        bool isCritical = criticalRate >= 1f || Random.value < criticalRate;
        float multiplier = isCritical ? Stats.CriticalMultiplier : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(Stats.Damage * EffectiveDamageMultiplier * multiplier));
    }

    protected virtual void OnInitialized()
    {
    }

    protected abstract bool IsCompatibleData(WeaponData weaponData);

    /// <summary>攻撃できた場合だけtrueを返す。</summary>
    protected abstract bool TryAttack();
}
