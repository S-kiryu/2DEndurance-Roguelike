using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponUpgradeSkill", menuName = "Skill/Weapon Upgrade")]
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
            if (CanUpgrade(weapon))
            {
                return true;
            }
        }

        return false;
    }

    public override void BuildCandidates(SkillContext context, List<SkillCandidate> candidates)
    {
        if (context == null || context.Weapons == null)
        {
            return;
        }

        foreach (Weapon weapon in context.Weapons.Weapons)
        {
            if (!CanUpgrade(weapon))
            {
                continue;
            }

            if (weapon.CanUpgradeStar)
            {
                WeaponUpgradeStage stage = weapon.GetNextStarUpgrade();
                string stageName = string.IsNullOrWhiteSpace(stage.DisplayName)
                    ? $"{weapon.DisplayName} ★{stage.StarLevel}"
                    : stage.DisplayName;
                string description = string.IsNullOrWhiteSpace(stage.Description)
                    ? $"{weapon.DisplayName}を★{stage.StarLevel}へ強化"
                    : $"対象: {weapon.DisplayName}\n{stage.Description}";

                candidates.Add(new SkillCandidate(
                    this,
                    stageName,
                    description,
                    $"weapon-star:{weapon.WeaponData.GetEntityId()}:{stage.StarLevel}",
                    weapon
                ));
            }
            else
            {
                float bonus = weapon.WeaponData.PostMaxDamageMultiplierBonus;
                candidates.Add(new SkillCandidate(
                    this,
                    $"{weapon.DisplayName} 攻撃強化",
                    $"対象: {weapon.DisplayName}\n★5後の攻撃力を+{bonus:P0}（何度でも取得可能）",
                    $"weapon-post-max:{weapon.WeaponData.GetEntityId()}:{bonus:R}",
                    weapon
                ));
            }
        }
    }

    public override bool TryApply(SkillContext context)
    {
        if (context == null || context.Weapons == null)
        {
            return false;
        }

        foreach (Weapon weapon in context.Weapons.Weapons)
        {
            if (TryUpgrade(weapon))
            {
                return true;
            }
        }

        return false;
    }

    internal override bool TryApplyCandidate(SkillContext context, SkillCandidate candidate)
    {
        if (context == null || context.Weapons == null || candidate == null ||
            candidate.TargetWeapon == null)
        {
            return false;
        }

        bool isOwned = false;
        foreach (Weapon weapon in context.Weapons.Weapons)
        {
            if (ReferenceEquals(weapon, candidate.TargetWeapon))
            {
                isOwned = true;
                break;
            }
        }

        return isOwned && TryUpgrade(candidate.TargetWeapon);
    }

    private static bool CanUpgrade(Weapon weapon)
    {
        return weapon != null && (weapon.CanUpgradeStar || weapon.CanUpgradePostMaxDamage);
    }

    private static bool TryUpgrade(Weapon weapon)
    {
        if (weapon == null)
        {
            return false;
        }

        return weapon.CanUpgradeStar
            ? weapon.ApplyNextStarUpgrade()
            : weapon.ApplyPostMaxDamageUpgrade();
    }
}
