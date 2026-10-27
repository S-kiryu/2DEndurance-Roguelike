using UnityEngine;

[CreateAssetMenu(fileName = "NewRarityUpgradeSkill", menuName = "Skill/Weapon Rarity Upgrade")]
public sealed class WeaponRarityUpgradeSkill : SkillBase
{
    public override SkillCategory Category => SkillCategory.WeaponRarityUpgrade;

    public override bool CanApply(SkillContext context)
    {
        if (context == null || context.Weapons == null)
        {
            return false;
        }

        foreach (Weapon weapon in context.Weapons.Weapons)
        {
            if (weapon != null)
            {
                return true;
            }
        }

        return false;
    }

    public override bool TryApply(SkillContext context)
    {
        if (!CanApply(context))
        {
            return false;
        }

        // nullを除いた所持武器を等確率で1つ選ぶ。
        Weapon target = null;
        int count = 0;
        foreach (Weapon weapon in context.Weapons.Weapons)
        {
            if (weapon != null && Random.Range(0, ++count) == 0)
            {
                target = weapon;
            }
        }

        target.UpgradeDebugRarity();
        return true;
    }
}
