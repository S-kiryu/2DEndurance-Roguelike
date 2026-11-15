using System;
using UnityEngine;

/// <summary>
/// Playerの状態とステータスを管理するModel
/// </summary>
[Serializable]
public sealed class PlayerModel
{
    /// <summary>死亡時に発火するイベント</summary>
    public event Action Died;
    public event Action<int> LeveledUp;

    public int MaxHealth => _maxHealth + maxHealthBonus;
    public int CurrentHealth { get; private set; }
    public int Level { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int ExperienceToNextLevel => (int)Math.Min(int.MaxValue,
        Math.Max(1, experienceForFirstLevelUp) + (long)(Level - 1) * Math.Max(0, experienceIncreasePerLevel));

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

    [Header("レベルアップ")]
    [SerializeField, Min(1)] private int experienceForFirstLevelUp = 5;
    [SerializeField, Min(0)] private int experienceIncreasePerLevel = 3;

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
        Level = 1;
        CurrentExperience = 0;
    }

    public bool TryGainExperience(int amount)
    {
        if (!IsAlive || amount <= 0)
        {
            return false;
        }

        long experience = (long)CurrentExperience + amount;
        int startingLevel = Level;
        while (experience >= ExperienceToNextLevel)
        {
            experience -= ExperienceToNextLevel;
            Level++;
        }

        CurrentExperience = (int)experience;
        if (Level > startingLevel)
        {
            LeveledUp?.Invoke(Level - startingLevel);
        }

        return true;
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

    /// <summary>
    /// このラン中だけ累積する自己強化。最大HPの増加分だけ現在HPも回復する。
    /// </summary>
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
