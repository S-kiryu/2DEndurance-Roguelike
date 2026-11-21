using UnityEngine;

/// <summary>武器の強化で加算できる項目。</summary>
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

/// <summary>
/// 指定した武器につかえる強化項目かどうかを判定する拡張メソッド。
/// </summary>
internal static class WeaponUpgradeStatExtensions
{
    public static bool CanApplyTo(this WeaponUpgradeStat stat, WeaponData data)
    {
        return stat switch
        {
            WeaponUpgradeStat.Range or
            WeaponUpgradeStat.AdditionalHits => data is SwordData,
            WeaponUpgradeStat.ProjectileSpeed or 
            WeaponUpgradeStat.Penetration or
            WeaponUpgradeStat.AdditionalProjectiles or 
            WeaponUpgradeStat.SpreadAngle => data is GunData,
            _ => data != null
        };
    }
}
