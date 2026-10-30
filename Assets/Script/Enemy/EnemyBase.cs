using UnityEngine;

/// <summary>
/// 敵のベースクラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private EnemyStatusData statusData;

    private IDamageable playerDamageable;
    private int currentMaxHP;
    private int currentDamage;

    public int CurrentHP { get; private set; }
    public int CurrentMaxHP => currentMaxHP;

    public int Damage => currentDamage;
    public float Speed => statusData != null ? statusData.Speed : 0f;

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

        ResetCombatStats();
    }

    public virtual void Initialize(GameObject player)
    {
        Initialize(player, EnemyStatScale.Default);
    }

    public virtual void Initialize(GameObject player, EnemyStatScale statScale)
    {
        playerDamageable = player != null
            ? player.GetComponentInParent<IDamageable>()
            : null;

        if (statusData == null)
        {
            return;
        }

        currentMaxHP = statScale.ScaleHealth(statusData.MaxHP);
        currentDamage = statScale.ScaleDamage(statusData.Damage);
        CurrentHP = currentMaxHP;

        if (TryGetComponent(out EnemyMove enemyMove))
        {
            enemyMove.Initialize(player, statusData.Speed);
        }
    }

    public virtual void OnPoolSpawned()
    {
        ResetCombatStats();
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

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealContactDamage(other);
    }

    /// <summary>
    /// 接触時に攻撃を試す関数
    /// </summary>
    /// <param name="other"></param>
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

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <param name="amount"></param>
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

    /// <summary>
    /// 死亡処置
    /// </summary>
    protected virtual void Die()
    {
        GameObjectPool.Despawn(gameObject);
    }

    /// <summary>
    /// ステータスの初期化
    /// </summary>
    private void ResetCombatStats()
    {
        currentMaxHP = statusData != null ? Mathf.Max(1, statusData.MaxHP) : 0;
        currentDamage = statusData != null ? Mathf.Max(0, statusData.Damage) : 0;
        CurrentHP = currentMaxHP;
    }
}
