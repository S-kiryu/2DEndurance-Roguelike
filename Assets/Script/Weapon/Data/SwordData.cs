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
    public bool UsePlayerAnimation => _usePlayerAnimation;

    public float AttackRange => _attackRange;
    public float AttackAngle => _attackAngle;
    public LayerMask TargetLayers => _targetLayers;

    [Header("ナタ（プレイヤー本体のアニメーション）")]
    [Tooltip("NataAttack ステート")]
    [SerializeField] private bool _usePlayerAnimation;

    [SerializeField, Min(0f)]
    private float _attackRange = 1f;

    [SerializeField, Range(0f, 360f)]
    private float _attackAngle = 90f;

    [SerializeField]
    private LayerMask _targetLayers;
}
