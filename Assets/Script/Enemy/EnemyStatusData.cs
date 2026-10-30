using UnityEngine;

/// <summary>
/// 敵ステータスデータ
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyStatus",
    menuName = "Enemy/Status Data"
)]
public class EnemyStatusData : ScriptableObject
{
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 2f;

    public int MaxHP => maxHP;
    public int Damage => damage;
    public float Speed => speed;
}