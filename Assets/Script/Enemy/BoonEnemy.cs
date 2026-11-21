using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーに近づいてから、前方に近接攻撃を行う骨の敵。
/// </summary>
[RequireComponent(typeof(Animator), typeof(EnemyMove))]
public class BoonEnemy : EnemyBase
{
    protected override bool DealsContactDamage => false;

    public override void Initialize(GameObject player, EnemyStatScale statScale)
    {
        base.Initialize(player, statScale);
        _animator.ResetTrigger(Attack);
        _animator.SetBool(IsMoving, false);
        _animator.Play(Idle, 0, 0f);
    }

    /// <summary>BoonAttack の Animation Event から呼ぶ。</summary>
    public void DealAttackDamage()
    {
        if (isActiveAndEnabled && _isAttacking && CurrentHP > 0 &&
            HasActiveTarget && HasPlayerInAttackHitbox())
        {
            DamagePlayer();
        }
    }

    /// <summary>BoonAttack の最終フレームから呼ぶ。</summary>
    public void FinishAttack()
    {
        if (!_isAttacking)
        {
            return;
        }

        _isAttacking = false;
        _nextAttackTime = Time.time + _attackCooldown;
    }

    [SerializeField] private BoxCollider2D _attackHitbox;
    [SerializeField, Min(0f)] private float _attackCooldown = 0.6f;

    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int Idle = Animator.StringToHash("Base Layer.BoonIdol");

    private Animator _animator;
    private EnemyMove _movement;
    private readonly List<Collider2D> _attackOverlaps = new();
    private Vector3 _leftHitboxPosition;
    private Vector3 _leftHitboxScale;
    private bool _isAttacking;
    private bool _hitboxFacesRight;
    private float _nextAttackTime;

    private bool HasActiveTarget => PlayerTarget != null && PlayerTarget.gameObject.activeInHierarchy;

    protected override void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<EnemyMove>();
        base.Awake();

        if (_attackHitbox != null)
        {
            // BoxCollider2D は攻撃範囲の形状として使い、通常の物理接触には参加させない。
            _attackHitbox.enabled = false;
            _leftHitboxPosition = _attackHitbox.transform.localPosition;
            _leftHitboxScale = _attackHitbox.transform.localScale;
        }
    }

    private void Update()
    {
        if (_isAttacking || CurrentHP <= 0 || !_animator.isActiveAndEnabled)
        {
            return;
        }

        if (!HasActiveTarget)
        {
            _movement.SetMovementPaused(true);
            _animator.SetBool(IsMoving, false);
            return;
        }

        float direction = PlayerTarget.position.x - transform.position.x;
        AimAttackHitbox(direction >= 0f);
        bool inRange = HasPlayerInAttackHitbox();
        _movement.SetMovementPaused(inRange);
        _animator.SetBool(IsMoving, !inRange);

        if (!inRange || Time.time < _nextAttackTime)
        {
            return;
        }

        _movement.FaceDirection(direction >= 0f ? 1f : -1f);
        _isAttacking = true;
        _animator.SetTrigger(Attack);
    }

    private void OnDisable()
    {
        _isAttacking = false;
        _nextAttackTime = 0f;
        if (_movement != null)
        {
            _movement.SetMovementPaused(false);
        }

        if (_animator != null && _animator.isActiveAndEnabled && _animator.isInitialized)
        {
            _animator.ResetTrigger(Attack);
        }
    }

    private void AimAttackHitbox(bool facesRight)
    {
        if (_attackHitbox == null || facesRight == _hitboxFacesRight)
        {
            return;
        }

        _hitboxFacesRight = facesRight;
        float mirror = facesRight ? -1f : 1f;
        Vector3 position = _leftHitboxPosition;
        Vector3 scale = _leftHitboxScale;
        position.x *= mirror;
        scale.x *= mirror;

        Transform hitbox = _attackHitbox.transform;
        hitbox.localPosition = position;
        hitbox.localScale = scale;
    }

    private bool HasPlayerInAttackHitbox()
    {
        if (_attackHitbox == null)
        {
            return false;
        }

        Transform hitbox = _attackHitbox.transform;
        Vector2 center = hitbox.TransformPoint(_attackHitbox.offset);
        Vector3 scale = hitbox.lossyScale;
        Vector2 size = Vector2.Scale(_attackHitbox.size, new Vector2(Mathf.Abs(scale.x), Mathf.Abs(scale.y)));
        Physics2D.OverlapBox(center, size, hitbox.eulerAngles.z, ContactFilter2D.noFilter, _attackOverlaps);

        foreach (Collider2D other in _attackOverlaps)
        {
            if (IsPlayerCollider(other))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (_attackHitbox == null)
        {
            return;
        }

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;
        Gizmos.matrix = _attackHitbox.transform.localToWorldMatrix;
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(_attackHitbox.offset, _attackHitbox.size);
        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }
}
