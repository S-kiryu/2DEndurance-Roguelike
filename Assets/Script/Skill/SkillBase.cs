using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スキルのカテゴリー
/// </summary>
public enum SkillCategory
{
    Weapon = 0,
    SelfEnhancement = 1,
    WeaponRarityUpgrade = 2
}

/// <summary>
/// スキル適用先 
/// </summary>
public sealed class SkillContext
{
    public PlayerPresenter Player { get; }
    public WeaponManager Weapons { get; }

    /// <summary>
    /// 今回のスキルが参照するプレイヤーと所持武器を保持する。
    /// </summary>
    public SkillContext(PlayerPresenter player, WeaponManager weapons)
    {
        Player = player;
        Weapons = weapons;
    }
}

/// <summary>1回の抽選に表示するデータ</summary>
public sealed class SkillCandidate
{
    public SkillBase Skill { get; }
    public Weapon TargetWeapon { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string IdentityKey { get; }
    public bool AllowDuplicateInSameSelection => Skill.AllowDuplicateInSameSelection;
    public SkillCategory Category => Skill.Category;
    public Sprite Icon { get; }
    public int CurrentStarLevel { get; }
    public int OfferedStarLevel { get; }

    /// <summary>抽選時に確定した対象と、カードに表示する武器の状態を保持する。</summary>
    public SkillCandidate(
        SkillBase skill,
        string displayName,
        string description,
        string identityKey,
        Weapon targetWeapon = null,
        Sprite weaponIcon = null,
        int offeredStarLevel = 0)
    {
        Skill = skill;
        DisplayName = displayName;
        Description = description;
        IdentityKey = identityKey;
        TargetWeapon = targetWeapon;
        Sprite defaultIcon = targetWeapon != null ? targetWeapon.WeaponData?.Stats.Icon : weaponIcon;
        Sprite skillIcon = skill != null ? skill.Icon : null;
        Icon = skillIcon != null ? skillIcon : defaultIcon;
        CurrentStarLevel = targetWeapon != null ? targetWeapon.StarLevel : 0;
        OfferedStarLevel = offeredStarLevel;
    }

    /// <summary>確定した対象にこの候補を適用する。</summary>
    public bool TryApply(SkillContext context)
    {
        return Skill != null && Skill.TryApplyCandidate(context, this);
    }
}

/// <summary>スキルの基底</summary>
public abstract class SkillBase : ScriptableObject
{
    public string DisplayName => _displayName;
    public virtual string Description => _description;
    public Sprite Icon => _icon;
    public bool AllowDuplicateInSameSelection => _allowDuplicateInSameSelection;
    public abstract SkillCategory Category { get; }

    /// <summary>現在の状態でスキルを適用できるか調べる。</summary>
    public abstract bool CanApply(SkillContext context);

    /// <summary>適用直前にも条件を再確認し、成功したときだけtrueを返す。</summary>
    public abstract bool TryApply(SkillContext context);

    /// <summary>通常スキルは1候補。武器強化は所持武器ごとに候補を展開する。</summary>
    public virtual void BuildCandidates(SkillContext context, List<SkillCandidate> candidates)
    {
        if (CanApply(context))
        {
            candidates.Add(new SkillCandidate(
                this,
                DisplayName,
                Description,
                GetSelectionIdentity(context)
            ));
        }
    }

    /// <summary>同じカードが存在しているかどうかを確認する</summary>
    public virtual string GetSelectionIdentity(SkillContext context)
    {
        return $"{GetType().FullName}|{DisplayName}|{Description}";
    }

    /// <summary>対象の指定を必要としない候補を適用する。</summary>
    internal virtual bool TryApplyCandidate(SkillContext context, SkillCandidate candidate)
    {
        return TryApply(context);
    }

    [SerializeField] private string _displayName;
    [SerializeField, TextArea(2, 5)] private string _description;
    [SerializeField] private Sprite _icon;
    [Tooltip("同じ内容の候補を、1回の3択に複数表示してよい場合だけ有効にします。")]
    [SerializeField] private bool _allowDuplicateInSameSelection;
}
