using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// 銃のクラス
/// </summary>
public class Gun : Weapon
{
    [SerializeField] private GunData data;
    private GameObject projectilePrefab;
    private Transform firePoint;

    protected override float GetAttackInterval()
    {
        return data.Stats.AttackInterval;
    }

    protected override bool CanAttack()
    {
        return true;
    }

    protected override void Attack()
    {
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Debug.Log("弾を生成");
    }
}
