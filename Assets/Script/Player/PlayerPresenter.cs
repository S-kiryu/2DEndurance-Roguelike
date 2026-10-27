using System;
using UnityEngine;

/// <summary>
/// PlayerのModelとViewを仲介する
/// </summary>
public sealed class PlayerPresenter : MonoBehaviour, IDamageable
{
    public event Action PlayerDied;

    public PlayerModel Model => _model;

    [SerializeField]
    private PlayerView _view;

    [SerializeField]
    private PlayerMovement _movement;
    [SerializeField]
    private PlayerModel _model = new PlayerModel();

    private float _invincibilityEndsAt;

    private void Awake()
    {
        _model.Died += OnPlayerDied;
        _model.Initialize();
        _movement.Initialize(_model);
    }

    private void Update()
    {
        if (_model.IsInvincible && Time.time >= _invincibilityEndsAt)
        {
            _model.SetInvincible(false);
        }
    }

    private void OnDisable()
    {
        _model.SetInvincible(false);
    }

    private void OnDestroy()
    {
        _model.Died -= OnPlayerDied;
    }

    public void ReceiveDamage(int damage)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        int previousHealth = _model.CurrentHealth;
        _model.TakeDamage(damage);

        if (_model.CurrentHealth == previousHealth)
        {
            return;
        }

        if (_model.IsAlive)
        {
            _invincibilityEndsAt = Time.time + _model.InvincibilityTime;
            _model.SetInvincible(true);
        }

        if (_view != null)
        {
            _view.PlayDamageFlash();
        }

        Debug.Log(
            $"{name}: {previousHealth - _model.CurrentHealth}ダメージ " +
            $"HP: {_model.CurrentHealth}/{_model.MaxHealth}",
            this
        );
    }

    public void TakeDamage(int damage)
    {
        ReceiveDamage(damage);
    }

    public void Heal(int amount)
    {
        _model.Heal(amount);
    }

    public bool ApplyEnhancement(int additionalMaxHealth, float additionalMoveSpeed)
    {
        if (!_model.ApplyEnhancement(additionalMaxHealth, additionalMoveSpeed))
        {
            return false;
        }

        Debug.Log($"自己強化: HP {_model.CurrentHealth}/{_model.MaxHealth}, 移動速度 {_model.MoveSpeed}", this);
        return true;
    }

    private void OnPlayerDied()
    {
        PlayerDied?.Invoke();
    }
}
