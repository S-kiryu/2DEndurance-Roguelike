using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近接武器のクラス
/// </summary>
public class Sword : Weapon
{
    /// <summary>
    /// 剣が敵に当たるフレームから Animation Event で呼ぶ。
    /// </summary>
    public void DealAttackDamage()
    {
        if (_isAttacking)
        {
            if (_swordData.UsePlayerAnimation)
            {
                if (_dealtPlayerAttackDamage) return;
                _dealtPlayerAttackDamage = true;
            }
            int hitCount = 1 + AdditionalHits;

            for (int i = 0; i < hitCount; i++)
            {
                DealDamage();
            }
        }
    }

    /// <summary>
    /// 攻撃アニメーションの最終フレームから Animation Event で呼ぶ。
    /// </summary>
    public void FinishAttack()
    {
        bool finished = _isAttacking;
        if (_visualRoot != null)
        {
            _visualRoot.localRotation = _idleRotation;
        }

        SetVisualVisible(false);
        _isAttacking = false;
        if (finished) NotifyAttackFinished();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_ownerView != null) _ownerView.CancelNataAttack(this);
        _isAttacking = false;

        if (_animator != null)
        {
            _animator.ResetTrigger(_attackTriggerHash);
        }

        if (_visualRoot != null)
        {
            _visualRoot.localRotation = _idleRotation;
        }

        SetVisualVisible(false);
    }

    protected override bool IsCompatibleData(WeaponData weaponData)
    {
        return weaponData is SwordData;
    }

    protected override void OnInitialized()
    {
        _swordData = (SwordData)Data;
        _ownerView = GetComponentInParent<PlayerView>();
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <returns></returns>
    protected override bool TryAttack()
    {
        if (_isAttacking || !HasTargetInRange())
        {
            return false;
        }

        if (_swordData.UsePlayerAnimation)
        {
            if (_ownerView == null || !_ownerView.TryPlayNataAttack(this, EffectiveAttackSpeedMultiplier))
                return false;
            _isAttacking = true;
            _dealtPlayerAttackDamage = false;
            NotifyAttackStarted();
            return true;
        }

        if (_animator == null) return false;
        _isAttacking = true;
        NotifyAttackStarted();
        SetVisualVisible(true);
        _animator.ResetTrigger(_attackTriggerHash);
        _animator.SetTrigger(_attackTriggerHash);
        return true;
    }

    [Header("攻撃モーション")]
    [SerializeField]
    private Transform _visualRoot;

    private Animator _animator;

    [Header("デバッグ")]
    [SerializeField]
    private bool _showAttackRange;

    private static readonly int _attackTriggerHash = Animator.StringToHash("Attack");
    private readonly HashSet<IDamageable> _hitTargets = new();
    private readonly List<Collider2D> _targetsInRange = new();
    private SpriteRenderer[] _visualRenderers;
    private SwordData _swordData;
    private Quaternion _idleRotation;
    private bool _isAttacking;
    private PlayerView _ownerView;
    private bool _dealtPlayerAttackDamage;

    private void Awake()
    {
        if (_visualRoot == null)
        {
            _visualRoot = transform;
        }

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        _idleRotation = _visualRoot.localRotation;
        _visualRenderers = _visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        SetVisualVisible(false);
    }

    private bool HasTargetInRange()
    {
        FindTargetsInRange();

        foreach (Collider2D targetCollider in _targetsInRange)
        {
            if (IsValidTarget(targetCollider))
            {
                return true;
            }
        }

        return false;
    }

    private void DealDamage()
    {
        FindTargetsInRange();
        _hitTargets.Clear();

        foreach (Collider2D targetCollider in _targetsInRange)
        {
            if (!IsValidTarget(targetCollider))
            {
                continue;
            }

            IDamageable target = targetCollider.GetComponentInParent<IDamageable>();

            if (target != null && _hitTargets.Add(target))
            {
                Context.DealDamage(targetCollider, CalculateDamage());
            }
        }
    }

    private void FindTargetsInRange()
    {
        int layerMask = _swordData.TargetLayers.value == 0
            ? Physics2D.AllLayers
            : _swordData.TargetLayers.value;

        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        filter.SetLayerMask(layerMask);
        Physics2D.OverlapCircle(transform.position, GetAttackRadius(_swordData), filter, _targetsInRange);
    }

    private float GetAttackRadius(SwordData attackData)
    {
        return attackData.AttackRange * WeaponRangeMultiplier;
    }

    private Vector2 GetAttackDirection()
    {
        Transform facingRoot = transform.parent != null ? transform.parent : transform;
        return facingRoot.lossyScale.x < 0f ? Vector2.left : Vector2.right;
    }

    private bool IsValidTarget(Collider2D targetCollider)
    {
        if (targetCollider.transform.root == transform.root)
        {
            return false;
        }

        Vector2 targetDirection = targetCollider.bounds.center - transform.position;

        if (targetDirection.sqrMagnitude > 0f &&
            Vector2.Angle(GetAttackDirection(), targetDirection) > _swordData.AttackAngle * 0.5f)
        {
            return false;
        }

        return targetCollider.GetComponentInParent<IDamageable>() != null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_showAttackRange || !isActiveAndEnabled || Data is not SwordData attackData)
        {
            return;
        }

        Vector3 center = transform.position;
        Vector3 reach = GetAttackDirection() * GetAttackRadius(attackData);
        float angle = attackData.AttackAngle;
        float halfAngle = angle * 0.5f;
        int segments = Mathf.Max(1, Mathf.CeilToInt(angle / 5f));
        Vector3 firstPoint = center + Quaternion.Euler(0f, 0f, -halfAngle) * reach;
        Vector3 previousPoint = firstPoint;

        Color previousColor = Gizmos.color;
        Gizmos.color = Color.cyan;
        for (int i = 1; i <= segments; i++)
        {
            float degrees = Mathf.Lerp(-halfAngle, halfAngle, (float)i / segments);
            Vector3 nextPoint = center + Quaternion.Euler(0f, 0f, degrees) * reach;
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }

        if (angle < 360f)
        {
            Gizmos.DrawLine(center, firstPoint);
            Gizmos.DrawLine(center, previousPoint);
        }
        Gizmos.color = previousColor;
    }
#endif

    private void SetVisualVisible(bool isVisible)
    {
        if (_visualRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer visualRenderer in _visualRenderers)
        {
            visualRenderer.enabled = isVisible;
        }
    }
}
