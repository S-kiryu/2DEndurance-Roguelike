using UnityEngine;

/// <summary>
/// 敵のベースクラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private EnemyStatusData statusData;

    public int CurrentHP { get; private set; }

    protected int Damage => statusData.Damage;
    protected float Speed => statusData.Speed;

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