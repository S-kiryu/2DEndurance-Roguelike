using UnityEngine;

public enum WeaponUpgradeStat
{
    [InspectorName("攻撃力倍率を加算")] Damage = 0,
    [InspectorName("攻撃速度倍率を加算")] AttackSpeed = 1,
    [InspectorName("クリティカル率を加算")] CriticalRate = 2,
    [InspectorName("剣の範囲倍率を加算")] Range = 3,
    [InspectorName("弾速倍率を加算")] ProjectileSpeed = 4,
    [InspectorName("貫通回数を加算")] Penetration = 5,
    [InspectorName("剣の追加ヒット数を加算")] AdditionalHits = 6,
    [InspectorName("同時発射弾数を加算")] AdditionalProjectiles = 7,
    [InspectorName("拡散角を加算（度）")] SpreadAngle = 8
}

[CreateAssetMenu(fileName = "NewWeaponEffect", menuName = "Weapons/Effects/Stat Bonus")]
public sealed class WeaponStatUpgradeEffect : WeaponUpgradeEffect
{
    [SerializeField, InspectorName("効果の種類")] private WeaponUpgradeStat stat;
    [Tooltip("倍率は0.2で+20%。クリティカル率は0.05で+5ポイント。回数・弾数は整数。拡散角は度数。")]
    [SerializeField, Min(0f), InspectorName("加算量")] private float amount = 1f;

    public WeaponUpgradeStat Stat => stat;
    public float Amount => Mathf.Max(0f, amount);

    public override bool CanApplyTo(WeaponData data)
    {
        switch (stat)
        {
            case WeaponUpgradeStat.Range:
            case WeaponUpgradeStat.AdditionalHits:
                return data is SwordData;
            case WeaponUpgradeStat.ProjectileSpeed:
            case WeaponUpgradeStat.Penetration:
            case WeaponUpgradeStat.AdditionalProjectiles:
            case WeaponUpgradeStat.SpreadAngle:
                return data is GunData;
            default:
                return data != null;
        }
    }

    public override void Apply(Weapon weapon)
    {
        if (weapon != null && CanApplyTo(weapon.WeaponData))
        {
            weapon.ApplyStatBonus(stat, Amount);
        }
    }
}
