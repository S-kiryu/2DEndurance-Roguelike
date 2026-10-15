using System;
using UnityEngine;

[Serializable]
public class WeaponStats
{
    [Header("基本情報")]
    public string weaponName;
    public Sprite icon;

    [TextArea(2, 5)]
    public string description;

    [Header("戦闘性能")]
    [Min(0f)]
    public float damage;

    [Range(0f, 1f)]
    public float criticalRate;

    [Min(1f)]
    public float criticalMultiplier = 1.5f;

    [Min(0.01f)]
    public float attackInterval = 1f;

    [Min(0)]
    public int attackSpeed = 0;

    [Header("強化")]
    [Min(0)]
    public int maxEnchantLevel = 5;
}