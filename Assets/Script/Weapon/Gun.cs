using UnityEngine;

/// <summary>
/// 銃のクラス
/// </summary>
public class Gun : Weapon
{
    [SerializeField]
    private Transform firePoint;

    private GunData gunData;

    protected override bool RequiresPool => true;

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
        if (gunData.ProjectilePrefab == null || ObjectPool == null || !ObjectPool.IsReady)
        {
            return false;
        }

        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Vector2 baseDirection = transform.lossyScale.x < 0f
            ? -spawnPoint.right
            : spawnPoint.right;

        int projectileCount = IsAtMaxStar
            ? 1 + gunData.Star5AdditionalProjectiles
            : 1;
        float spread = IsAtMaxStar ? gunData.Star5SpreadAngle : 0f;
        bool fired = false;

        for (int i = 0; i < projectileCount; i++)
        {
            float normalized = projectileCount <= 1
                ? 0.5f
                : (float)i / (projectileCount - 1);
            float angle = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, normalized);
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * baseDirection;

            Projectile projectile = ObjectPool.Spawn(
                gunData.ProjectilePrefab,
                spawnPoint.position,
                Quaternion.FromToRotation(Vector3.right, direction)
            );

            if (projectile == null)
            {
                continue;
            }

            projectile.Initialize(
                direction,
                gunData.ProjectileSpeed * WeaponProjectileSpeedMultiplier,
                CalculateDamage(),
                gunData.PenetrationCount + WeaponPenetrationBonus,
                transform.root.gameObject
            );
            fired = true;
        }

        return fired;
    }
}
