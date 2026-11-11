using System;
using UnityEngine;

/// <summary>
/// Playerの状態とステータスを管理するModel
/// </summary>
[Serializable]
public sealed class PlayerModel
{
    public event Action Died;

    public int MaxHealth => _maxHealth + maxHealthBonus;
    public int CurrentHealth { get; private set; }

    public float MoveSpeed => _moveSpeed + moveSpeedBonus;
    public float JumpSpeed => _jumpPower;
    public float InvincibilityTime => _invincibilityTime;

    public float AttackMultiplier => _attackMultiplier + attackMultiplierBonus;
    public float AttackSpeedMultiplier => _attackSpeedMultiplier + attackSpeedMultiplierBonus;
    public float CriticalRateBonus => _criticalRateBonus + criticalRateBonus;

    public bool IsAlive => CurrentHealth > 0;
    public bool IsInvincible { get; private set; }

    [Header("基礎ステータス")]
    [SerializeField, Min(1)]
    private int _maxHealth = 100;

    [SerializeField, Min(0.01f)]
    private float _moveSpeed = 5f;

    [SerializeField, Min(0.01f)]
    private float _jumpPower = 10f;

    [SerializeField, Min(0.01f)]
    private float _invincibilityTime = 1f;

    [Header("基礎戦闘ステータス")]
    [SerializeField, Min(0f)]
    private float _attackMultiplier = 1f;

    [SerializeField, Min(0.01f)]
    private float _attackSpeedMultiplier = 1f;

    [SerializeField, Min(0f)]
    private float _criticalRateBonus;

    private int maxHealthBonus;
    private float moveSpeedBonus;
    private float attackMultiplierBonus;
    private float attackSpeedMultiplierBonus;
    private float criticalRateBonus;

    public void Initialize()
    {
        maxHealthBonus = 0;
        moveSpeedBonus = 0f;
        attackMultiplierBonus = 0f;
        attackSpeedMultiplierBonus = 0f;
        criticalRateBonus = 0f;
        CurrentHealth = MaxHealth;
        IsInvincible = false;
    }

    /// <summary>
    /// 攻撃を食らう処理
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsInvincible || !IsAlive)
        {
            return;
        }

        CurrentHealth = Mathf.Max(
            CurrentHealth - damage,
            0
        );

        if (!IsAlive)
        {
            OnPlayerDead();
        }
    }


    /// <summary>
    /// 回復処理
    /// </summary>
    /// <param name="amount"></param>
    public void Heal(int amount)
    {
        if (!IsAlive)
        {
            return;
        }

        CurrentHealth = Mathf.Min(
            CurrentHealth + amount,
            MaxHealth
        );
    }

    /// <summary>このラン中だけ累積する自己強化。最大HPの増加分だけ現在HPも回復する。</summary>
    public bool ApplyEnhancement(
        int additionalMaxHealth,
        float additionalMoveSpeed,
        float additionalAttackMultiplier = 0f,
        float additionalAttackSpeedMultiplier = 0f,
        float additionalCriticalRate = 0f)
    {
        if (!IsAlive || additionalMaxHealth < 0 || additionalMoveSpeed < 0f ||
            additionalAttackMultiplier < 0f || additionalAttackSpeedMultiplier < 0f || additionalCriticalRate < 0f)
        {
            return false;
        }

        maxHealthBonus += additionalMaxHealth;
        moveSpeedBonus += additionalMoveSpeed;
        attackMultiplierBonus += additionalAttackMultiplier;
        attackSpeedMultiplierBonus += additionalAttackSpeedMultiplier;
        criticalRateBonus += additionalCriticalRate;
        Heal(additionalMaxHealth);
        return true;
    }

    public void SetInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    private void OnPlayerDead()
    {
        Died?.Invoke();
    }
}
