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
    [Header("ナタ（プレイヤー本体のアニメーション）")]
    [SerializeField] private bool usePlayerAnimation;
    [Tooltip("未設定の間は移動だけ切り替わり、攻撃しません。命中フレームにDealNataDamageイベントを追加してください。")]
    [SerializeField] private AnimationClip playerAttackClip;

    public bool UsePlayerAnimation => usePlayerAnimation;
    public AnimationClip PlayerAttackClip => playerAttackClip;

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
