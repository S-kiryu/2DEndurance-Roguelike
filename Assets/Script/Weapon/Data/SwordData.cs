using UnityEngine;

/// <summary>
/// 近接武器固有の設定。
/// </summary>
[CreateAssetMenu(
    fileName = "NewSwordData",
    menuName = "Weapons/Sword"
)]
public sealed class SwordData : WeaponData
{
    [SerializeField, Min(0f)]
    private float attackRange = 1f;

    [SerializeField, Range(0f, 360f)]
    private float attackAngle = 90f;

    [SerializeField]
    private LayerMask targetLayers;

    public float AttackRange => attackRange;
    public float AttackAngle => attackAngle;
    public LayerMask TargetLayers => targetLayers;
}
