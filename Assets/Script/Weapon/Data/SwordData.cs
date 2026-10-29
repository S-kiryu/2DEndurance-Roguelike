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

    [Header("星5固有効果")]
    [Tooltip("1なら命中時に追加でもう1回ダメージを与えます。")]
    [SerializeField, Min(1)]
    private int star5AdditionalHits = 1;

    public float AttackRange => attackRange;
    public float AttackAngle => attackAngle;
    public LayerMask TargetLayers => targetLayers;
    public int Star5AdditionalHits => Mathf.Max(1, star5AdditionalHits);
}
