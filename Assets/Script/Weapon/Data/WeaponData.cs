using UnityEngine;

/// <summary>
/// 武器の抽象データクラス
/// </summary>
public abstract class WeaponData : ScriptableObject
{
    [SerializeField]
    private WeaponStats stats = new();

    [SerializeField]
    private Weapon weaponPrefab;

    public WeaponStats Stats => stats;
    public Weapon WeaponPrefab => weaponPrefab;
}
