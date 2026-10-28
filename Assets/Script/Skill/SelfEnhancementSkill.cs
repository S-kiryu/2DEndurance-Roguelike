using UnityEngine;

/// <summary>
/// 自身を強化するスキル
/// </summary>
[CreateAssetMenu(fileName = "NewSelfEnhancementSkill", menuName = "Skill/Self Enhancement")]
public sealed class SelfEnhancementSkill : SkillBase
{
    [SerializeField, Min(0)] private int additionalMaxHealth = 20;
    [SerializeField, Min(0f)] private float additionalMoveSpeed = 1f;

    [Header("戦闘ステータスの増加量")]
    [Tooltip("0.2で攻撃倍率+20%。同種の強化は加算します。")]
    [SerializeField, Min(0f)] private float additionalAttackMultiplier;
    [Tooltip("0.2で攻撃速度+20%。アニメーション速度には影響しません。")]
    [SerializeField, Min(0f)] private float additionalAttackSpeedMultiplier;
    [Tooltip("0.05でクリティカル率+5ポイント。武器の確率に加算します。")]
    [SerializeField, Min(0f)] private float additionalCriticalRate;

    public override SkillCategory Category => SkillCategory.SelfEnhancement;

    public override bool CanApply(SkillContext context)
    {
        return context != null && context.Player != null && context.Player.Model.IsAlive &&
            additionalMaxHealth >= 0 && additionalMoveSpeed >= 0f &&
            additionalAttackMultiplier >= 0f && additionalAttackSpeedMultiplier >= 0f && additionalCriticalRate >= 0f &&
            (additionalMaxHealth > 0 || additionalMoveSpeed > 0f || additionalAttackMultiplier > 0f ||
                additionalAttackSpeedMultiplier > 0f || additionalCriticalRate > 0f);
    }

    public override bool TryApply(SkillContext context)
    {
        return CanApply(context) &&
            context.Player.ApplyEnhancement(additionalMaxHealth, additionalMoveSpeed,
                additionalAttackMultiplier, additionalAttackSpeedMultiplier, additionalCriticalRate);
    }
}
