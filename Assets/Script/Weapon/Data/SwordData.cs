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

    public LayerMask TargetLayers => _targetLayers;

    [Header("ナタ（プレイヤー本体のアニメーション）")]
    [Tooltip("NataAttack ステート")]
    [SerializeField] private bool _usePlayerAnimation;

    [SerializeField]
    private LayerMask _targetLayers;
}
