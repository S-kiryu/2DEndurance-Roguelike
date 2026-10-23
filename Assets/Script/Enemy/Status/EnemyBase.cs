using UnityEngine;

/// <summary>
/// 敵のベースクラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [SerializeField] private EnemyStatusData statusData;

    public int CurrentHP { get; private set; }

    public int Damage => statusData.Damage;
    public float Speed => statusData.Speed;

    protected virtual void Awake()
    {
        if (statusData == null)
        {
            Debug.LogError(
                $"{name} に EnemyStatusData が設定されていません。",
                this
            );

            enabled = false;
            return;
        }

        CurrentHP = statusData.MaxHP;
    }

    public virtual void Initialize(GameObject player)
    {
        if (statusData == null)
        {
            return;
        }

        if (TryGetComponent(out EnemyMove enemyMove))
        {
            enemyMove.Initialize(player, statusData.Speed);
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHP <= 0)
        {
            return;
        }

        CurrentHP = Mathf.Max(CurrentHP - amount, 0);

        if (CurrentHP == 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}