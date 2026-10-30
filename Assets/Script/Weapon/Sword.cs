using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近接武器のクラス
/// </summary>
public class Sword : Weapon
{
    [Header("攻撃モーション")]
    [SerializeField]
    private Transform visualRoot;

    [SerializeField]
    private Animator animator;

    private static readonly int AttackTrigger = Animator.StringToHash("Attack");
    private readonly HashSet<IDamageable> hitTargets = new();
    private SpriteRenderer[] visualRenderers;
    private SwordData swordData;
    private Quaternion idleRotation;
    private bool isAttacking;

    private void Awake()
    {
        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        idleRotation = visualRoot.localRotation;
        visualRenderers = visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        SetVisualVisible(false);
    }

    private void OnDisable()
    {
        isAttacking = false;

        if (animator != null)
        {
            animator.ResetTrigger(AttackTrigger);
        }

        if (visualRoot != null)
        {
            visualRoot.localRotation = idleRotation;
        }

        SetVisualVisible(false);
    }

    protected override bool IsCompatibleData(WeaponData weaponData)
    {
        return weaponData is SwordData;
    }

    protected override void OnInitialized()
    {
        swordData = (SwordData)Data;
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <returns></returns>
    protected override bool TryAttack()
    {
        if (isAttacking || animator == null || !HasTargetInRange())
        {
            return false;
        }

        isAttacking = true;
        SetVisualVisible(true);
        animator.ResetTrigger(AttackTrigger);
        animator.SetTrigger(AttackTrigger);
        return true;
    }

    /// <summary>
    /// 剣が敵に当たるフレームから Animation Event で呼ぶ。
    /// </summary>
    public void DealAttackDamage()
    {
        if (isAttacking)
        {
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
        if (visualRoot != null)
        {
            visualRoot.localRotation = idleRotation;
        }

        SetVisualVisible(false);
        isAttacking = false;
    }

    private bool HasTargetInRange()
    {
        Collider2D[] colliders = FindTargetsInRange();

        foreach (Collider2D targetCollider in colliders)
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
        Collider2D[] colliders = FindTargetsInRange();
        hitTargets.Clear();

        foreach (Collider2D targetCollider in colliders)
        {
            if (!IsValidTarget(targetCollider))
            {
                continue;
            }

            IDamageable target = targetCollider.GetComponentInParent<IDamageable>();

            if (target != null && hitTargets.Add(target))
            {
                target.TakeDamage(CalculateDamage());
            }
        }
    }

    private Collider2D[] FindTargetsInRange()
    {
        int layerMask = swordData.TargetLayers.value == 0
            ? Physics2D.AllLayers
            : swordData.TargetLayers.value;

        return Physics2D.OverlapCircleAll(
            transform.position,
            swordData.AttackRange * WeaponRangeMultiplier,
            layerMask
        );
    }

    private bool IsValidTarget(Collider2D targetCollider)
    {
        if (targetCollider.transform.root == transform.root)
        {
            return false;
        }

        Transform facingRoot = transform.parent != null
            ? transform.parent
            : transform;
        Vector2 attackDirection = facingRoot.lossyScale.x < 0f
            ? Vector2.left
            : Vector2.right;

        Vector2 targetDirection = targetCollider.bounds.center - transform.position;

        if (targetDirection.sqrMagnitude > 0f &&
            Vector2.Angle(attackDirection, targetDirection) > swordData.AttackAngle * 0.5f)
        {
            return false;
        }

        return targetCollider.GetComponentInParent<IDamageable>() != null;
    }

    private void SetVisualVisible(bool isVisible)
    {
        if (visualRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer visualRenderer in visualRenderers)
        {
            visualRenderer.enabled = isVisible;
        }
    }
}
