using System;
using UnityEngine;

[Serializable]
public class WeaponStats
{
    [Header("基本情報")]
    public string WeaponName;
    public Sprite Icon;

    [TextArea(2, 5)]
    public string Description;

    [Header("戦闘性能")]
    [Min(0f)]
    public float Damage;

    [Range(0f, 1f)]
    public float CriticalRate;

    [Min(1f)]
    public float CriticalMultiplier = 1.5f;

    [Min(0.01f)]
    public float AttackInterval = 1f;

    [Min(0)]
    public int AttackSpeed = 0;

    [Header("強化")]
    [Min(0)]
    public int MaxEnchantLevel = 5;
}