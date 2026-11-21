using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponEffect", menuName = "Weapons/Effects/Stat Bonus")]
public sealed class WeaponStatUpgradeEffect : WeaponUpgradeEffect
{
    public WeaponUpgradeStat Stat => _stat;
    public float Amount => Mathf.Max(0f, _amount);

    public override bool CanApplyTo(WeaponData data) => _stat.CanApplyTo(data);

    public override void Apply(Weapon weapon)
    {
        if (weapon != null && CanApplyTo(weapon.WeaponData))
        {
            weapon.ApplyStatBonus(_stat, Amount);
        }
    }

    [SerializeField, InspectorName("効果の種類")] private WeaponUpgradeStat _stat;
    [Tooltip("倍率は0.2で+20%。クリティカル率は0.05で+5ポイント。回数・弾数は整数。拡散角は度数。")]
    [SerializeField, Min(0f), InspectorName("加算量")] private float _amount = 1f;
}
