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
    public Projectile ProjectilePrefab => _projectilePrefab;
    public float ProjectileSpeed => _projectileSpeed;
    /// <summary>
    /// 貫通回数
    /// </summary>
    public int PenetrationCount => Mathf.Max(0, _penetrationCount);
    public int ProjectileCount => Mathf.Max(1, _projectileCount);
    public float SpreadAngle => Mathf.Clamp(_spreadAngle, 0f, 90f);
    public int BurstCount => Mathf.Max(1, _burstCount);
    public float BurstInterval => Mathf.Max(0.01f, _burstInterval);

    [Header("遠距離武器の設定")]
    [SerializeField]
    private Projectile _projectilePrefab;

    [SerializeField, Min(0f)]
    private float _projectileSpeed = 10f;

    // 貫通する敵の数
    [SerializeField, Min(0)]
    private int _penetrationCount;

    [Header("通常時の発射パターン")]
    [SerializeField, Min(1)]
    [InspectorName("同時発射弾数")] private int _projectileCount = 1;

    [Tooltip("追加弾を含む全弾の端から端までの角度")]
    [SerializeField, Range(0f, 90f)]
    [InspectorName("拡散角（度）")] private float _spreadAngle;

    [Tooltip("1なら通常射撃。3なら短い間隔で3回発射します。")]
    [SerializeField, Min(1), InspectorName("バースト回数")] private int _burstCount = 1;
    [SerializeField, Min(0.01f), InspectorName("バースト内の発射間隔（秒）")] private float _burstInterval = 0.08f;
}
