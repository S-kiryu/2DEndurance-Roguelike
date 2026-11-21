using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

/// <summary>
/// 自身を強化するスキル
/// </summary>
[CreateAssetMenu(fileName = "NewSelfEnhancementSkill", menuName = "Skill/Self Enhancement")]
public sealed class SelfEnhancementSkill : SkillBase
{
    public override SkillCategory Category => SkillCategory.SelfEnhancement;

    public override string Description
    {
        get
        {
            var bonuses = new List<string>();
            if (_additionalMaxHealth > 0)
                bonuses.Add($"最大HP +{_additionalMaxHealth}");
            if (_additionalMoveSpeed > 0f)
                bonuses.Add($"移動速度 +{_additionalMoveSpeed:0.##}");
            if (_additionalAttackMultiplier > 0f)
                bonuses.Add($"攻撃力 +{_additionalAttackMultiplier * 100f:0.##}%");
            if (_additionalAttackSpeedMultiplier > 0f)
                bonuses.Add($"攻撃速度 +{_additionalAttackSpeedMultiplier * 100f:0.##}%");
            if (_additionalCriticalRate > 0f)
                bonuses.Add($"クリティカル率 +{_additionalCriticalRate * 100f:0.##}ポイント");
            return string.Join("\n", bonuses);
        }
    }

    public override bool CanApply(SkillContext context)
    {
        return context != null && context.Player != null && context.Player.Model.IsAlive &&
            _additionalMaxHealth >= 0 && _additionalMoveSpeed >= 0f &&
            _additionalAttackMultiplier >= 0f && _additionalAttackSpeedMultiplier >= 0f && _additionalCriticalRate >= 0f &&
            (_additionalMaxHealth > 0 || _additionalMoveSpeed > 0f || _additionalAttackMultiplier > 0f ||
                _additionalAttackSpeedMultiplier > 0f || _additionalCriticalRate > 0f);
    }

    public override bool TryApply(SkillContext context)
    {
        return CanApply(context) &&
            context.Player.ApplyEnhancement(_additionalMaxHealth, _additionalMoveSpeed,
                _additionalAttackMultiplier, _additionalAttackSpeedMultiplier, _additionalCriticalRate);
    }

    public override string GetSelectionIdentity(SkillContext context)
    {
        CultureInfo culture = CultureInfo.InvariantCulture;
        return string.Join("|",
            "self-enhancement",
            _additionalMaxHealth.ToString(culture),
            _additionalMoveSpeed.ToString("R", culture),
            _additionalAttackMultiplier.ToString("R", culture),
            _additionalAttackSpeedMultiplier.ToString("R", culture),
            _additionalCriticalRate.ToString("R", culture));
    }

    [SerializeField, Min(0)] private int _additionalMaxHealth = 20;
    [SerializeField, Min(0f)] private float _additionalMoveSpeed = 1f;

    [Header("戦闘ステータスの増加量")]
    [Tooltip("0.2で攻撃倍率+20%。同種の強化は加算します。")]
    [SerializeField, Min(0f)] private float _additionalAttackMultiplier;
    [Tooltip("0.2で攻撃速度+20%。攻撃間隔とナタの攻撃モーションに反映します。")]
    [SerializeField, Min(0f)] private float _additionalAttackSpeedMultiplier;
    [Tooltip("0.05でクリティカル率+5ポイント。武器の確率に加算します。")]
    [SerializeField, Min(0f)] private float _additionalCriticalRate;
}
