using UnityEngine;

/// <summary>
/// 武器の抽象データクラス
/// </summary>
public abstract class WeaponData : ScriptableObject
{
    [SerializeField]
    private WeaponStats stats = new();

    public WeaponStats Stats => stats;
}