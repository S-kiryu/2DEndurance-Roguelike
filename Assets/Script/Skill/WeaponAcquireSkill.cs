using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 指定した武器を生成するスキル
/// </summary>
[CreateAssetMenu(fileName = "NewWeaponSkill", menuName = "Skill/Weapon Acquisition")]
public sealed class WeaponAcquireSkill : SkillBase
{
    public override SkillCategory Category => SkillCategory.Weapon;
    public Sprite NewWeaponIndicator => _newWeaponIndicator;

    /// <summary>
    /// 装備枠に空きがあり、対象武器を未所持か確認する。
    /// </summary>
    public override bool CanApply(SkillContext context)
    {
        return context?.Weapons != null && context.Weapons.CanEquipWeapon(_weaponData);
    }

    /// <summary>
    /// 獲得する武器データから候補の重複判定キーを作る。
    /// </summary>
    public override string GetSelectionIdentity(SkillContext context)
    {
        return $"weapon-acquire:{(_weaponData != null ? _weaponData.GetEntityId().ToString() : "none")}";
    }

    /// <summary>
    /// 未所持の武器を、武器データのアイコン付きで候補に加える。
    /// </summary>
    public override void BuildCandidates(SkillContext context, List<SkillCandidate> candidates)
    {
        if (CanApply(context))
        {
            candidates.Add(new SkillCandidate(
                this, DisplayName, Description, GetSelectionIdentity(context),
                weaponIcon: _weaponData.Stats.Icon
            ));
        }
    }

    /// <summary>
    /// 条件を再確認して対象武器を装備する。
    /// </summary>
    public override bool TryApply(SkillContext context)
    {
        if (!CanApply(context))
        {
            return false;
        }

        Weapon weapon = context.Weapons.EquipWeapon(_weaponData);
        if (weapon == null)
        {
            return false;
        }

        Debug.Log($"スキルで武器獲得: {weapon.DisplayName}", context.Weapons);
        return true;
    }

    [SerializeField] private WeaponData _weaponData;
    [SerializeField] private Sprite _newWeaponIndicator;
}
