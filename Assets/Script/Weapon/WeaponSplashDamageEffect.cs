using System.Collections.Generic;
using UnityEngine;

/// <summary>Contextを使った効果の実装例。命中した相手以外へ追加の範囲ダメージを与える。</summary>
[AddComponentMenu("Weapons/Effects/命中時の範囲ダメージ")]
public sealed class WeaponSplashDamageEffect : WeaponEffect
{
    public override void OnHit(WeaponHit hit)
    {
        int damage = Mathf.RoundToInt(hit.Damage * Mathf.Max(0f, _damageRatio));
        if (damage <= 0) return;
        _targets.Clear();
        _targets.Add(hit.Target);
        foreach (Collider2D collider in Context.FindTargets(hit.Position, _radius, _targetLayers))
        {
            IDamageable target = collider.GetComponentInParent<IDamageable>();
            if (target != null && _targets.Add(target)) Context.DealEffectDamage(collider, damage);
        }
    }

    [SerializeField, Min(0f), InspectorName("範囲")] private float _radius = 2f;
    [SerializeField, Min(0f), InspectorName("今回のダメージに対する倍率")] private float _damageRatio = 0.5f;
    [SerializeField, InspectorName("対象レイヤー")] private LayerMask _targetLayers;
    private readonly HashSet<IDamageable> _targets = new();
}
