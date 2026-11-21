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

    public int MaxHealth => _maxHealth + _maxHealthBonus;
    public int CurrentHealth { get; private set; }
    public int Level { get; private set; } = 1;
    public int CurrentExperience { get; private set; }
    public int ExperienceToNextLevel => (int)Math.Min(int.MaxValue,
        Math.Max(1, _experienceForFirstLevelUp) + (long)(Level - 1) * Math.Max(0, _experienceIncreasePerLevel));

    public float MoveSpeed => _moveSpeed + _moveSpeedBonus;
    public float JumpSpeed => _jumpPower;
    public float InvincibilityTime => _invincibilityTime;

    public float AttackMultiplier => _attackMultiplier + _attackMultiplierBonus;
    public float AttackSpeedMultiplier => _attackSpeedMultiplier + _attackSpeedMultiplierBonus;
    public float CriticalRateBonus => _criticalRateBonus + _criticalRateEnhancementBonus;

    public bool IsAlive => CurrentHealth > 0;
    public bool IsInvincible { get; private set; }

    public void Initialize()
    {
        _maxHealthBonus = 0;
        _moveSpeedBonus = 0f;
        _attackMultiplierBonus = 0f;
        _attackSpeedMultiplierBonus = 0f;
        _criticalRateEnhancementBonus = 0f;
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
    /// このラン中だけ累積する自己強化
    /// </summary>
    /// <param name="additionalMaxHealth">追加の最大HP</param>
    /// <param name="additionalMoveSpeed">追加の移動速度</param>
    /// <param name="additionalAttackMultiplier">追加の攻撃力倍率</param>
    /// <param name="additionalAttackSpeedMultiplier">追加の攻撃速度倍率</param>
    /// <param name="additionalCriticalRate">追加のクリティカル率</param>
    /// <returns></returns>
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

        _maxHealthBonus += additionalMaxHealth;
        _moveSpeedBonus += additionalMoveSpeed;
        _attackMultiplierBonus += additionalAttackMultiplier;
        _attackSpeedMultiplierBonus += additionalAttackSpeedMultiplier;
        _criticalRateEnhancementBonus += additionalCriticalRate;
        Heal(additionalMaxHealth);
        return true;
    }

    public void SetInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

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
    [SerializeField, Min(1)] private int _experienceForFirstLevelUp = 5;
    [SerializeField, Min(0)] private int _experienceIncreasePerLevel = 3;

    private int _maxHealthBonus;
    private float _moveSpeedBonus;
    private float _attackMultiplierBonus;
    private float _attackSpeedMultiplierBonus;
    private float _criticalRateEnhancementBonus;

    /// <summary>
    /// 死亡処理
    /// </summary>
    private void OnPlayerDead()
    {
        Died?.Invoke();
    }
}
