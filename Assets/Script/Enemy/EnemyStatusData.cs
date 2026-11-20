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
    public int MaxHP => _maxHP;
    public int Damage => _damage;
    public float Speed => _speed;
    public int ExperienceReward => _experienceReward;
    public ExperiencePickup ExperiencePickupPrefab => _experiencePickupPrefab;

    [SerializeField] private int _maxHP = 100;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _speed = 2f;
    [SerializeField, Min(0)] private int _experienceReward = 1;
    [SerializeField] private ExperiencePickup _experiencePickupPrefab;
}
