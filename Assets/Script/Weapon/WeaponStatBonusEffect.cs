using UnityEngine;

[AddComponentMenu("Weapons/Effects/ステータス強化")]
public sealed class WeaponStatBonusEffect : WeaponEffect
{
    public override bool Supports(WeaponData data) => _stat.CanApplyTo(data);

    // 永続加算せず参照時に合成するので、チェックOFF/Remove Componentで確実に外れる。
    public override float GetStatBonus(WeaponUpgradeStat requested) => requested == _stat ? Mathf.Max(0f, _amount) : 0f;

    [SerializeField, InspectorName("効果の種類")] private WeaponUpgradeStat _stat;
    [SerializeField, Min(0f), InspectorName("加算量")]
    [Tooltip("倍率は0.2で+20%。弾数・ヒット数・貫通回数は整数、拡散角は度数。")]
    private float _amount = 0.2f;
}
