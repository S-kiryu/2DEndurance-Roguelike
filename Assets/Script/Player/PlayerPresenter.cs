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

    /// <summary>
    /// 敵からのダメージを受け取り伝達する
    /// </summary>
    /// <param name="damage">攻撃ダメージ</param>
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

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <param name="damage">攻撃ダメージ</param>
    public void TakeDamage(int damage)
    {
        ReceiveDamage(damage);
    }

    /// <summary>
    /// 回復
    /// </summary>
    /// <param name="amount">回復量</param>
    public void Heal(int amount)
    {
        _model.Heal(amount);
    }

    /// <summary>
    /// ステータスを追加する関数
    /// </summary>
    /// <param name="additionalMaxHealth">最大体力</param>
    /// <param name="additionalMoveSpeed">スピード</param>
    /// <param name="additionalAttackMultiplier">攻撃倍率</param>
    /// <param name="additionalAttackSpeedMultiplier">攻撃速度</param>
    /// <param name="additionalCriticalRate">クリティカル率</param>
    /// <returns></returns>
    public bool ApplyEnhancement(
        int additionalMaxHealth,
        float additionalMoveSpeed,
        float additionalAttackMultiplier = 0f,
        float additionalAttackSpeedMultiplier = 0f,
        float additionalCriticalRate = 0f)
    {
        if (!_model.ApplyEnhancement(additionalMaxHealth, additionalMoveSpeed,
            additionalAttackMultiplier, additionalAttackSpeedMultiplier, additionalCriticalRate))
        {
            return false;
        }

        Debug.Log(
            $"自己強化: HP {_model.CurrentHealth}/{_model.MaxHealth}, 移動速度 {_model.MoveSpeed}, " +
            $"攻撃 {_model.AttackMultiplier:F2}倍, 攻撃速度 {_model.AttackSpeedMultiplier:F2}倍, " +
            $"クリティカル率加算 {_model.CriticalRateBonus:P0}", this);
        return true;
    }

    /// <summary>
    /// 死亡処理
    /// </summary>
    private void OnPlayerDied()
    {
        PlayerDied?.Invoke();
    }
}
