using System.Collections;
using UnityEngine;

/// <summary>
/// 銃のクラス
/// </summary>
public class Gun : Weapon
{
    [SerializeField]
    private Transform firePoint;

    private GunData gunData;
    private bool isBursting;

    protected override bool RequiresPool => true;

    protected override bool IsCompatibleData(WeaponData weaponData)
    {
        return weaponData is GunData;
    }

    protected override void OnInitialized()
    {
        StopAllCoroutines();
        isBursting = false;
        gunData = (GunData)Data;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isBursting = false;
    }

    protected override bool TryAttack()
    {
        if (isBursting || gunData.ProjectilePrefab == null || ObjectPool == null || !ObjectPool.IsReady)
        {
            return false;
        }

        bool fired = FireVolley();
        if (fired && gunData.BurstCount > 1)
        {
            StartCoroutine(FireRemainingBurst(gunData.BurstCount - 1));
        }
        return fired;
    }

    private IEnumerator FireRemainingBurst(int remaining)
    {
        isBursting = true;
        for (int i = 0; i < remaining; i++)
        {
            yield return new WaitForSeconds(gunData.BurstInterval);
            if (ObjectPool == null || !ObjectPool.IsReady || !FireVolley())
            {
                break;
            }
        }
        isBursting = false;
    }

    private bool FireVolley()
    {
        Transform spawnPoint = firePoint != null ? firePoint : transform;
        Vector2 baseDirection = transform.lossyScale.x < 0f
            ? -spawnPoint.right
            : spawnPoint.right;

        int projectileCount = gunData.ProjectileCount + AdditionalProjectiles;
        float spread = Mathf.Clamp(gunData.SpreadAngle + SpreadAngleBonus, 0f, 90f);
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
