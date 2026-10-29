using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// スキルのカテゴリー
/// </summary>
public enum SkillCategory
{
    Weapon,
    SelfEnhancement,
    WeaponRarityUpgrade
}

/// <summary>
/// スキル適用先 
/// </summary>
public sealed class SkillContext
{
    public PlayerPresenter Player { get; }
    public WeaponManager Weapons { get; }

    public SkillContext(PlayerPresenter player, WeaponManager weapons)
    {
        Player = player;
        Weapons = weapons;
    }
}

/// <summary>1回の抽選に表示する、適用先まで確定済みの候補。</summary>
public sealed class SkillCandidate
{
    public SkillBase Skill { get; }
    public Weapon TargetWeapon { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string IdentityKey { get; }
    public bool AllowDuplicateInSameSelection => Skill.AllowDuplicateInSameSelection;
    public SkillCategory Category => Skill.Category;
    public Sprite Icon => Skill.Icon;

    public SkillCandidate(
        SkillBase skill,
        string displayName,
        string description,
        string identityKey,
        Weapon targetWeapon = null)
    {
        Skill = skill;
        DisplayName = displayName;
        Description = description;
        IdentityKey = identityKey;
        TargetWeapon = targetWeapon;
    }

    public bool TryApply(SkillContext context)
    {
        return Skill != null && Skill.TryApplyCandidate(context, this);
    }
}

/// <summary>スキルの基底</summary>
public abstract class SkillBase : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField, TextArea(2, 5)] private string description;
    [SerializeField] private Sprite icon;
    [Tooltip("同じ内容の候補を、1回の3択に複数表示してよい場合だけ有効にします。")]
    [SerializeField] private bool allowDuplicateInSameSelection;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public bool AllowDuplicateInSameSelection => allowDuplicateInSameSelection;
    public abstract SkillCategory Category { get; }

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

    public virtual string GetSelectionIdentity(SkillContext context)
    {
        return $"{GetType().FullName}|{DisplayName}|{Description}";
    }

    internal virtual bool TryApplyCandidate(SkillContext context, SkillCandidate candidate)
    {
        return TryApply(context);
    }
}
