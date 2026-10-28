using UnityEngine;

/// <summary>
/// 敵のベースクラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private EnemyStatusData statusData;

    private IDamageable playerDamageable;

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
        playerDamageable = player != null
            ? player.GetComponentInParent<IDamageable>()
            : null;

        if (statusData == null)
        {
            return;
        }

        if (TryGetComponent(out EnemyMove enemyMove))
        {
            enemyMove.Initialize(player, statusData.Speed);
        }
    }

    public virtual void OnPoolSpawned()
    {
        CurrentHP = statusData != null ? statusData.MaxHP : 0;
        playerDamageable = null;
    }

    public virtual void OnPoolDespawned()
    {
        playerDamageable = null;
        CancelInvoke();
        StopAllCoroutines();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDealContactDamage(collision.collider);
    }

    private void TryDealContactDamage(Collider2D other)
    {
        if (!isActiveAndEnabled || statusData == null ||
            CurrentHP <= 0 || playerDamageable == null)
        {
            return;
        }

        // 追跡対象のプレイヤーだけを攻撃する。子のColliderにも対応する。
        IDamageable target = other.GetComponentInParent<IDamageable>();

        if (ReferenceEquals(target, playerDamageable))
        {
            target.TakeDamage(Damage);
        }
    }

    public virtual void TakeDamage(int amount)
    {
        if (!isActiveAndEnabled || amount <= 0 || CurrentHP <= 0)
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
        GameObjectPool.Despawn(gameObject);
    }
}
