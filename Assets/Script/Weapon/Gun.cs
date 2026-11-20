using System.Collections;
using UnityEngine;

/// <summary>
/// 銃のクラス
/// </summary>
public class Gun : Weapon
{
    protected override bool RequiresPool => true;

    protected override bool IsCompatibleData(WeaponData weaponData)
    {
        return weaponData is GunData;
    }

    protected override string GetDataError(WeaponData weaponData)
    {
        return ((GunData)weaponData).ProjectilePrefab == null ? "弾のPrefabが設定されていません。" : null;
    }

    protected override void OnInitialized()
    {
        ResetAttackState();
        _gunData = (GunData)Data;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ResetAttackState();
    }

    protected override bool TryAttack()
    {
        if (_isBursting || _gunData.ProjectilePrefab == null || ObjectPool == null || !ObjectPool.IsReady)
        {
            return false;
        }

        bool fired = FireVolley();
        if (fired && _gunData.BurstCount > 1)
        {
            StartCoroutine(FireRemainingBurst(_gunData.BurstCount - 1));
        }

        return fired;
    }

    [SerializeField]
    private Transform _firePoint;
    [SerializeField]
    private GameObject _effect;
    [SerializeField, Min(0f)]
    private float _muzzleEffectDuration = 0.05f;

    private GunData _gunData;
    private bool _isBursting;
    private Coroutine _muzzleEffectRoutine;

    private void ResetAttackState()
    {
        StopAllCoroutines();
        _isBursting = false;
        _muzzleEffectRoutine = null;
        if (_effect != null)
        {
            _effect.SetActive(false);
        }
    }

    private IEnumerator FireRemainingBurst(int remaining)
    {
        _isBursting = true;
        for (int i = 0; i < remaining; i++)
        {
            yield return new WaitForSeconds(_gunData.BurstInterval);
            if (ObjectPool == null || !ObjectPool.IsReady || !FireVolley())
            {
                break;
            }
        }
        _isBursting = false;
    }

    private bool FireVolley()
    {
        NotifyAttackStarted();
        Transform spawnPoint = _firePoint != null ? _firePoint : transform;
        Vector2 baseDirection = transform.lossyScale.x < 0f
            ? -spawnPoint.right
            : spawnPoint.right;

        int projectileCount = _gunData.ProjectileCount + AdditionalProjectiles;
        float spread = Mathf.Clamp(_gunData.SpreadAngle + SpreadAngleBonus, 0f, 90f);
        bool fired = false;

        for (int i = 0; i < projectileCount; i++)
        {
            float normalized = projectileCount <= 1
                ? 0.5f
                : (float)i / (projectileCount - 1);
            float angle = Mathf.Lerp(-spread * 0.5f, spread * 0.5f, normalized);
            Vector2 direction = Quaternion.Euler(0f, 0f, angle) * baseDirection;

            Projectile projectile = ObjectPool.Spawn(
                _gunData.ProjectilePrefab,
                spawnPoint.position,
                Quaternion.FromToRotation(Vector3.right, direction)
            );

            if (projectile == null)
            {
                continue;
            }

            projectile.Initialize(
                direction,
                _gunData.ProjectileSpeed * WeaponProjectileSpeedMultiplier,
                CalculateDamage(),
                _gunData.PenetrationCount + WeaponPenetrationBonus,
                Context.Owner,
                Context
            );
            fired = true;
        }

        if (fired && _effect != null && isActiveAndEnabled)
        {
            if (_muzzleEffectRoutine != null)
            {
                StopCoroutine(_muzzleEffectRoutine);
            }

            float shotInterval = _gunData.BurstCount > 1
                ? Mathf.Min(EffectiveAttackInterval, _gunData.BurstInterval)
                : EffectiveAttackInterval;
            // 次の発射までにフラッシュが消えている時間を残す。
            float duration = Mathf.Min(_muzzleEffectDuration, shotInterval * 0.5f);
            _muzzleEffectRoutine = StartCoroutine(PlayMuzzleEffect(duration));
        }

        NotifyAttackFinished();
        return fired;
    }

    /// <summary>
    /// フラッシュ演出の関数
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator PlayMuzzleEffect(float duration)
    {
        _effect.SetActive(true);
        yield return new WaitForSeconds(duration);
        if (_effect != null)
        {
            _effect.SetActive(false);
        }
        _muzzleEffectRoutine = null;
    }
}
