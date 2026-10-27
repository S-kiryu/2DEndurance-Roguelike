using UnityEngine;

/// <summary>
/// 自身を強化するスキル
/// </summary>
[CreateAssetMenu(fileName = "NewSelfEnhancementSkill", menuName = "Skill/Self Enhancement")]
public sealed class SelfEnhancementSkill : SkillBase
{
    [SerializeField, Min(0)] private int additionalMaxHealth = 20;
    [SerializeField, Min(0f)] private float additionalMoveSpeed = 1f;

    public override SkillCategory Category => SkillCategory.SelfEnhancement;

    public override bool CanApply(SkillContext context)
    {
        return context != null && context.Player != null && context.Player.Model.IsAlive &&
            additionalMaxHealth >= 0 && additionalMoveSpeed >= 0f &&
            (additionalMaxHealth > 0 || additionalMoveSpeed > 0f);
    }

    public override bool TryApply(SkillContext context)
    {
        return CanApply(context) &&
            context.Player.ApplyEnhancement(additionalMaxHealth, additionalMoveSpeed);
    }
}
