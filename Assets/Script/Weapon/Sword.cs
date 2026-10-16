using UnityEngine;

/// <summary>
/// 近接武器のクラス
/// </summary>
public class Sword : Weapon
{
    private WeaponData swordData;
    private bool canAttackFlag = true;

    protected override float GetAttackInterval()
    {
        return swordData.Stats.AttackInterval;
    }

    protected override bool CanAttack()
    {
        return canAttackFlag;
    }

    protected override void Attack()
    {
        Debug.Log("Sword: Attack executed");
        canAttackFlag = false;
        Invoke(nameof(ResetAttack), swordData.Stats.AttackInterval);
    }

    private void ResetAttack()
    {
        canAttackFlag = true;
    }
}
