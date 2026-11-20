using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 攻撃範囲まで追いかけ、予備動作、突進、硬直の順で攻撃する敵。
/// </summary>
[RequireComponent(typeof(Animator), typeof(EnemyMove))]
public class kimo : EnemyBase
{
    protected override bool DealsContactDamage => false;

    public override void Initialize(GameObject player, EnemyStatScale statScale)
    {
        base.Initialize(player, statScale);
        ResetAttack();
    }

    /// <summary>Attack の Animation Event から突進を開始する。</summary>
    public void BeginDash()
    {
        if (!isActiveAndEnabled || _phase != Phase.Preparing || CurrentHP <= 0 || !HasActiveTarget)
        {
            return;
        }

        _phase = Phase.Attacking;
        _hasHitPlayer = false;
        _movement.StartDash(_dashDirection, _dashSpeed, _dashDistance, CheckDashHit);
    }

    [SerializeField, Min(0.01f)] private float _dashSpeed = 12f;
    [SerializeField, Min(0.01f)] private float _dashDistance = 4f;
    [SerializeField, Min(0.01f)] private float _attackRange = 3.5f;
    [FormerlySerializedAs("_dashInterval")]
    [SerializeField, Min(0f)] private float _recoveryDuration = 1.5f;
    [SerializeField] private BoxCollider2D _dashHitbox;

    private enum Phase
    {
        Moving,
        Preparing,
        Attacking,
        Recovering
    }

    private static readonly int Move = Animator.StringToHash("Base Layer.Move");
    private static readonly int PrepareAttack = Animator.StringToHash("PrepareAttack");
    private static readonly int BeginRecovery = Animator.StringToHash("BeginRecovery");
    private static readonly int FinishRecovery = Animator.StringToHash("FinishRecovery");
    private static readonly int RecoverySpeed = Animator.StringToHash("RecoverySpeed");

    private readonly List<Collider2D> _dashOverlaps = new();
    private Animator _animator;
    private EnemyMove _movement;
    private Phase _phase;
    private float _recoveryEndsAt;
    private float _dashDirection;
    private bool _hasHitPlayer;

    private bool HasActiveTarget => PlayerTarget != null && PlayerTarget.gameObject.activeInHierarchy;

    protected override void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<EnemyMove>();
        if (_dashHitbox != null)
        {
            // 判定の形状だけを借りる。物理接触は既存の本体 Collider が担当する。
            _dashHitbox.enabled = false;
        }
        base.Awake();
    }

    private void Update()
    {
        if (CurrentHP <= 0 || !_animator.isActiveAndEnabled)
        {
            return;
        }

        if (!HasActiveTarget)
        {
            if (_phase != Phase.Moving)
            {
                ResetAttack();
            }
            _movement.SetMovementPaused(true);
            return;
        }

        if (_phase == Phase.Attacking && !_movement.IsDashing)
        {
            _phase = Phase.Recovering;
            _recoveryEndsAt = Time.time + _recoveryDuration;
            // 1秒の Recovery クリップを指定された硬直時間に合わせる。
            _animator.SetFloat(RecoverySpeed, 1f / Mathf.Max(0.01f, _recoveryDuration));
            _animator.SetTrigger(BeginRecovery);
            return;
        }

        if (_phase == Phase.Recovering && Time.time >= _recoveryEndsAt)
        {
            _phase = Phase.Moving;
            _movement.SetMovementPaused(false);
            _animator.SetTrigger(FinishRecovery);
            return;
        }

        if (_phase != Phase.Moving)
        {
            return;
        }

        _movement.SetMovementPaused(false);
        float direction = Mathf.Sign(PlayerTarget.position.x - transform.position.x);
        if (!IsPlayerInAttackRange(direction))
        {
            return;
        }

        _dashDirection = direction;
        _phase = Phase.Preparing;
        _movement.SetMovementPaused(true);
        _movement.FaceDirection(_dashDirection);
        _animator.SetTrigger(PrepareAttack);
    }

    private void OnDisable()
    {
        ResetAttack();
        if (_movement != null)
        {
            _movement.SetMovementPaused(false);
        }
    }

    private void ResetAttack()
    {
        if (_movement != null)
        {
            _movement.StopDash();
        }
        _phase = Phase.Moving;
        if (_animator != null && _animator.isActiveAndEnabled && _animator.isInitialized)
        {
            _animator.ResetTrigger(PrepareAttack);
            _animator.ResetTrigger(BeginRecovery);
            _animator.ResetTrigger(FinishRecovery);
            _animator.Play(Move, 0, 0f);
        }
    }

    private bool IsPlayerInAttackRange(float direction)
    {
        if (_dashHitbox == null)
        {
            return false;
        }

        // 水平の開始範囲は調整可能。高さは実際の突進判定と揃える。
        Transform hitbox = _dashHitbox.transform;
        Vector2 center = hitbox.TransformPoint(_dashHitbox.offset);
        center.x = transform.position.x + direction * _attackRange * 0.5f;
        float height = _dashHitbox.size.y * Mathf.Abs(hitbox.lossyScale.y);
        return HasPlayerInBox(center, new Vector2(_attackRange, height));
    }

    private void CheckDashHit(float step)
    {
        if (_hasHitPlayer || _dashHitbox == null || !HasActiveTarget)
        {
            return;
        }

        Transform hitbox = _dashHitbox.transform;
        Vector2 offset = _dashHitbox.offset;
        offset.x *= -_dashDirection;
        Vector2 center = hitbox.TransformPoint(offset);
        Vector3 scale = hitbox.lossyScale;
        Vector2 size = Vector2.Scale(_dashHitbox.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));

        // この物理フレームで横切る範囲全体を判定し、高速でもすり抜けを防ぐ。
        center.x += step * 0.5f;
        size.x += Mathf.Abs(step);
        if (HasPlayerInBox(center, size))
        {
            _hasHitPlayer = true;
            DamagePlayer();
        }
    }

    private bool HasPlayerInBox(Vector2 center, Vector2 size)
    {
        ContactFilter2D bodyFilter = ContactFilter2D.noFilter;
        bodyFilter.useTriggers = false;
        Physics2D.OverlapBox(center, size, 0f, bodyFilter, _dashOverlaps);
        foreach (Collider2D other in _dashOverlaps)
        {
            if (IsPlayerCollider(other))
            {
                return true;
            }
        }

        return false;
    }
}
