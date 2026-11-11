using UnityEngine;

[AddComponentMenu("Weapons/Effects/ステータス強化")]
public sealed class WeaponStatBonusEffect : WeaponEffect
{
    [SerializeField, InspectorName("効果の種類")] private WeaponUpgradeStat stat;
    [SerializeField, Min(0f), InspectorName("加算量")]
    [Tooltip("倍率は0.2で+20%。弾数・ヒット数・貫通回数は整数、拡散角は度数。")]
    private float amount = 0.2f;

    public override bool Supports(WeaponData data)
    {
        switch (stat)
        {
            case WeaponUpgradeStat.Range:
            case WeaponUpgradeStat.AdditionalHits: return data is SwordData;
            case WeaponUpgradeStat.ProjectileSpeed:
            case WeaponUpgradeStat.Penetration:
            case WeaponUpgradeStat.AdditionalProjectiles:
            case WeaponUpgradeStat.SpreadAngle: return data is GunData;
            default: return base.Supports(data);
        }
    }

    // 永続加算せず参照時に合成するので、チェックOFF/Remove Componentで確実に外れる。
    public override float GetStatBonus(WeaponUpgradeStat requested) => requested == stat ? Mathf.Max(0f, amount) : 0f;
}
