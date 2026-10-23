using UnityEngine;

/// <summary>
/// 銃のクラス
/// </summary>
public class Gun : Weapon
{
    [SerializeField]
    private Transform firePoint;

    private GunData gunData;

    protected override bool IsCompatibleData(WeaponData weaponData)
    {
        return weaponData is GunData;
    }

    protected override void OnInitialized()
    {
        gunData = (GunData)Data;
    }

    protected override bool TryAttack()
    {
        if (gunData.ProjectilePrefab == null)
        {
            return false;
        }

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Vector2 direction = transform.lossyScale.x < 0f
            ? -spawnPoint.right
            : spawnPoint.right;

        Projectile projectile = Instantiate(
            gunData.ProjectilePrefab,
            spawnPoint.position,
            Quaternion.FromToRotation(Vector3.right, direction)
        );

        projectile.Initialize(
            direction,
            gunData.ProjectileSpeed,
            CalculateDamage(),
            gunData.PenetrationCount,
            transform.root.gameObject
        );

        return true;
    }
}
