using System.Collections.Generic;
using System.Globalization;
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

    public override string Description
    {
        get
        {
            var bonuses = new List<string>();
            if (additionalMaxHealth > 0)
                bonuses.Add($"最大HP +{additionalMaxHealth}");
            if (additionalMoveSpeed > 0f)
                bonuses.Add($"移動速度 +{additionalMoveSpeed:0.##}");
            if (additionalAttackMultiplier > 0f)
                bonuses.Add($"攻撃力 +{additionalAttackMultiplier * 100f:0.##}%");
            if (additionalAttackSpeedMultiplier > 0f)
                bonuses.Add($"攻撃速度 +{additionalAttackSpeedMultiplier * 100f:0.##}%");
            if (additionalCriticalRate > 0f)
                bonuses.Add($"クリティカル率 +{additionalCriticalRate * 100f:0.##}ポイント");
            return string.Join("\n", bonuses);
        }
    }

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

    public override string GetSelectionIdentity(SkillContext context)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        return string.Join("|",
            "self-enhancement",
            additionalMaxHealth.ToString(culture),
            additionalMoveSpeed.ToString("R", culture),
            additionalAttackMultiplier.ToString("R", culture),
            additionalAttackSpeedMultiplier.ToString("R", culture),
            additionalCriticalRate.ToString("R", culture));
    }
}
