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

    [Header("通常時の発射パターン")]
    [SerializeField, Min(1)]
    [InspectorName("同時発射弾数")] private int projectileCount = 1;

    [Tooltip("追加弾を含む全弾の端から端までの角度")]
    [SerializeField, Range(0f, 90f)]
    [InspectorName("拡散角（度）")] private float spreadAngle;

    [Tooltip("1なら通常射撃。3なら短い間隔で3回発射します。")]
    [SerializeField, Min(1), InspectorName("バースト回数")] private int burstCount = 1;
    [SerializeField, Min(0.01f), InspectorName("バースト内の発射間隔（秒）")] private float burstInterval = 0.08f;

    public Projectile ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    /// <summary>
    /// 貫通回数
    /// </summary>
    public int PenetrationCount => Mathf.Max(0, penetrationCount);
    public int ProjectileCount => Mathf.Max(1, projectileCount);
    public float SpreadAngle => Mathf.Clamp(spreadAngle, 0f, 90f);
    public int BurstCount => Mathf.Max(1, burstCount);
    public float BurstInterval => Mathf.Max(0.01f, burstInterval);
}
