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

    public Projectile ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    /// <summary>
    /// 貫通回数
    /// </summary>
    public int PenetrationCount => penetrationCount;
}
