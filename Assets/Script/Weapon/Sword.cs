using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近接武器のクラス
/// </summary>
public class Sword : Weapon
{
    private readonly HashSet<IDamageable> hitTargets = new();
    private SwordData swordData;

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
        int layerMask = swordData.TargetLayers.value == 0
            ? Physics2D.AllLayers
            : swordData.TargetLayers.value;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            transform.position,
            swordData.AttackRange,
            layerMask
        );

        Vector2 attackDirection = transform.lossyScale.x < 0f
            ? -transform.right
            : transform.right;

        hitTargets.Clear();

        foreach (Collider2D targetCollider in colliders)
        {
            if (targetCollider.transform.root == transform.root)
            {
                continue;
            }

            Vector2 targetDirection = targetCollider.bounds.center - transform.position;

            if (targetDirection.sqrMagnitude > 0f &&
                Vector2.Angle(attackDirection, targetDirection) > swordData.AttackAngle * 0.5f)
            {
                continue;
            }

            IDamageable target = targetCollider.GetComponentInParent<IDamageable>();

            if (target == null || !hitTargets.Add(target))
            {
                continue;
            }

            target.TakeDamage(CalculateDamage());
        }

        return hitTargets.Count > 0;
    }
}
