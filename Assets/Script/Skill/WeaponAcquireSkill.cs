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
            context.Weapons.HasCapacity && weaponData != null && weaponData.WeaponPrefab != null &&
            !context.Weapons.HasWeapon(weaponData);
    }

    public override string GetSelectionIdentity(SkillContext context)
    {
        return $"weapon-acquire:{(weaponData != null ? weaponData.GetEntityId().ToString() : "none")}";
    }

    public override bool TryApply(SkillContext context)
    {
        if (!CanApply(context))
        {
            return false;
        }

        Weapon weapon = context.Weapons.EquipWeapon(weaponData);
        if (weapon == null)
        {
            return false;
        }

        Debug.Log($"スキルで武器獲得: {weapon.DisplayName}", context.Weapons);
        return true;
    }
}
