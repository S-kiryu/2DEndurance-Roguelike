using UnityEngine;

/// <summary>
/// 武器のベースクラス
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [SerializeField]
    private WeaponData data;

    private float cooldownRemaining;

    protected WeaponData Data => data;
    protected WeaponStats Stats => data.Stats;

    public WeaponData WeaponData => data;
    public string DisplayName => data != null ? data.Stats.WeaponName : name;
    public int DebugRarityLevel { get; private set; }

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

        cooldownRemaining = Mathf.Max(cooldownRemaining - deltaTime, 0f);

        if (cooldownRemaining <= 0f && TryAttack())
        {
            cooldownRemaining = Stats.AttackInterval;
        }
    }

    public bool Initialize(WeaponData weaponData = null)
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
        bool isCritical = Random.value < Stats.CriticalRate;
        float multiplier = isCritical ? Stats.CriticalMultiplier : 1f;
        return Mathf.Max(1, Mathf.RoundToInt(Stats.Damage * multiplier));
    }

    protected virtual void OnInitialized()
    {
    }

    protected abstract bool IsCompatibleData(WeaponData weaponData);

    /// <summary>攻撃できた場合だけtrueを返す。</summary>
    protected abstract bool TryAttack();
}
