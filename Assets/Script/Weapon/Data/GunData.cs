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
    public GameObject projectilePrefab;

    [Min(0f)]
    public float projectileSpeed = 10f;

    // 貫通する敵の数
    [Min(0)]
    public int penetrationCount;
}
