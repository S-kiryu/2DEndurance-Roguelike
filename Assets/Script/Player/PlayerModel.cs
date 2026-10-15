using System;
using UnityEngine;

/// <summary>
/// Playerの状態とステータスを管理するModel
/// </summary>
[Serializable]
public sealed class PlayerModel
{
    public int MaxHealth => _maxHealth;
    public int CurrentHealth { get; private set; }

    public float MoveSpeed => _moveSpeed;
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

    public void Initialize()
    {
        CurrentHealth = _maxHealth;
        IsInvincible = false;
    }

    public void TakeDamage(int damage)
    {
        if (IsInvincible || !IsAlive)
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
            _maxHealth
        );
    }

    public void SetInvincible(bool isInvincible)
    {
        IsInvincible = isInvincible;
    }

    private void OnPlayerDead()
    {
        // GameFlowPresenterへ死亡を通知する
    }
}