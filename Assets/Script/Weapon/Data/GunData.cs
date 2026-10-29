using UnityEngine;

/// <summary>
/// 遠距離の武器のデータを格納するクラス
/// </summary>
[CreateAssetMenu(
    fileName = "NewGunData",
    menuName = "Weapons/Gun"
)]
public class GunData : WeaponData
{
    [Header("遠距離武器の設定")]
    [SerializeField]
    private Projectile projectilePrefab;

    [SerializeField, Min(0f)]
    private float projectileSpeed = 10f;

    // 貫通する敵の数
    [SerializeField, Min(0)]
    private int penetrationCount;

    [Header("星5固有効果")]
    [SerializeField, Min(1)]
    private int star5AdditionalProjectiles = 2;

    [Tooltip("追加弾を含む全弾の端から端までの角度")]
    [SerializeField, Range(0f, 90f)]
    private float star5SpreadAngle = 12f;

    public Projectile ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    /// <summary>
    /// 貫通回数
    /// </summary>
    public int PenetrationCount => Mathf.Max(0, penetrationCount);
    public int Star5AdditionalProjectiles => Mathf.Max(1, star5AdditionalProjectiles);
    public float Star5SpreadAngle => Mathf.Clamp(star5SpreadAngle, 0f, 90f);
}
