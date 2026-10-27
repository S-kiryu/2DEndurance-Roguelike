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

    private int maxHealthBonus;
    private float moveSpeedBonus;

    public void Initialize()
    {
        maxHealthBonus = 0;
        moveSpeedBonus = 0f;
        CurrentHealth = MaxHealth;
        IsInvincible = false;
    }

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
    public bool ApplyEnhancement(int additionalMaxHealth, float additionalMoveSpeed)
    {
        if (!IsAlive || additionalMaxHealth < 0 || additionalMoveSpeed < 0f)
        {
            return false;
        }

        maxHealthBonus += additionalMaxHealth;
        moveSpeedBonus += additionalMoveSpeed;
        Heal(additionalMaxHealth);
        return true;
    }

    public void SetInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

    private void OnPlayerDead()
    {
        Died?.Invoke();
    }
}
