using UnityEngine;

/// <summary>
/// 指定した武器を生成するスキル
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponSkill", menuName = "Skill/Weapon Acquisition")]
public sealed class WeaponAcquireSkill : SkillBase
{
    [SerializeField] private WeaponData weaponData;

    public override SkillCategory Category => SkillCategory.Weapon;

    public override bool CanApply(SkillContext context)
    {
        return context != null && context.Weapons != null &&
            weaponData != null && weaponData.WeaponPrefab != null;
    }

    public override bool TryApply(SkillContext context)
    {
        if (!CanApply(context))
        {
            return false;
        }

        // 仮仕様では同じ武器も別インスタンスとして追加できる。
        Weapon weapon = context.Weapons.EquipWeapon(weaponData);
        if (weapon == null)
        {
            return false;
        }

        Debug.Log($"スキルで武器獲得: {weapon.DisplayName}", context.Weapons);
        return true;
    }
}
