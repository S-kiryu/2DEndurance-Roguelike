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

/// <summary>スキルの基底</summary>
public abstract class SkillBase : ScriptableObject
{
    [SerializeField] private string displayName;
    [SerializeField, TextArea(2, 5)] private string description;
    [SerializeField] private Sprite icon;

    public string DisplayName => displayName;
    public string Description => description;
    public Sprite Icon => icon;
    public abstract SkillCategory Category { get; }

    public abstract bool CanApply(SkillContext context);

    /// <summary>適用直前にも条件を再確認し、成功したときだけtrueを返す。</summary>
    public abstract bool TryApply(SkillContext context);
}
