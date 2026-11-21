using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponUpgradeSkill", menuName = "Skill/Weapon Upgrade")]
public sealed class WeaponRarityUpgradeSkill : SkillBase
{
    public override SkillCategory Category => SkillCategory.WeaponRarityUpgrade;

    /// <summary>強化できる所持武器があるか調べる。</summary>
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

    /// <summary>所持武器ごとに、次の星段階か上限到達後の攻撃強化を候補に加える。</summary>
    public override void BuildCandidates(SkillContext context, List<SkillCandidate> candidates)
    {
        if (context == null || context.Weapons == null)
        {
            return;
        }

        foreach (Weapon weapon in context.Weapons.Weapons)
        {
            if (weapon == null)
            {
                continue;
            }

            WeaponUpgradeStage stage = weapon.GetNextStarUpgrade();
            if (stage != null)
            {
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
                    weapon,
                    offeredStarLevel: stage.StarLevel
                ));
            }
            else if (weapon.CanUpgradePostMaxDamage)
            {
                float bonus = weapon.WeaponData.PostMaxDamageMultiplierBonus;
                candidates.Add(new SkillCandidate(
                    this,
                    $"{weapon.DisplayName} 攻撃強化",
                    $"対象: {weapon.DisplayName}\n★{weapon.MaxStarLevel}後の攻撃力を+{bonus:P0}（何度でも取得可能）",
                    $"weapon-post-max:{weapon.WeaponData.GetEntityId()}:{bonus:R}",
                    weapon
                ));
            }
        }
    }

    /// <summary>強化できる所持武器を先頭から探して適用する。</summary>
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

    /// <summary>候補に表示した星段階の強化を、同じ装備に適用する。</summary>
    internal override bool TryApplyCandidate(SkillContext context, SkillCandidate candidate)
    {
        Weapon weapon = candidate?.TargetWeapon;
        if (context?.Weapons == null || !context.Weapons.OwnsWeapon(weapon) ||
            weapon.StarLevel != candidate.CurrentStarLevel)
        {
            return false;
        }

        if (candidate.OfferedStarLevel == 0)
            return weapon.ApplyPostMaxDamageUpgrade();

        WeaponUpgradeStage stage = weapon.GetNextStarUpgrade();
        return stage != null && stage.StarLevel == candidate.OfferedStarLevel && weapon.ApplyNextStarUpgrade();
    }

    /// <summary>武器に適用できる強化があるか調べる。</summary>
    private static bool CanUpgrade(Weapon weapon)
    {
        return weapon != null && (weapon.CanUpgradeStar || weapon.CanUpgradePostMaxDamage);
    }

    /// <summary>武器の現在の星段階に対応する強化を適用する。</summary>
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
