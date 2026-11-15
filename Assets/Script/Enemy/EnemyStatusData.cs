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
    [SerializeField, Min(0)] private int experienceReward = 1;
    [SerializeField] private ExperiencePickup experiencePickupPrefab;

    public int MaxHP => maxHP;
    public int Damage => damage;
    public float Speed => speed;
    public int ExperienceReward => experienceReward;
    public ExperiencePickup ExperiencePickupPrefab => experiencePickupPrefab;
}
