using UnityEngine;

/// <summary>
/// 敵のベースクラス
/// </summary>
[RequireComponent(typeof(EnemyView))]
public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
{
    public int CurrentHP { get; private set; }
    public int CurrentMaxHP => _currentMaxHP;

    public int Damage => _currentDamage;
    public float Speed => _statusData != null ? _statusData.Speed : 0f;

    protected Transform PlayerTarget { get; private set; }
    protected virtual bool DealsContactDamage => true;

    public virtual void Initialize(GameObject player)
    {
        Initialize(player, EnemyStatScale.Default);
    }

    public virtual void Initialize(GameObject player, EnemyStatScale statScale)
    {
        PlayerTarget = player != null ? player.transform : null;
        _playerDamageable = player != null
            ? player.GetComponentInParent<IDamageable>()
            : null;

        if (_statusData == null)
        {
            return;
        }

        _currentMaxHP = statScale.ScaleHealth(_statusData.MaxHP);
        _currentDamage = statScale.ScaleDamage(_statusData.Damage);
        CurrentHP = _currentMaxHP;

        if (TryGetComponent(out EnemyMove enemyMove))
        {
            enemyMove.Initialize(player, _statusData.Speed);
        }
    }

    public virtual void OnPoolSpawned()
    {
        ResetCombatStats();
        PlayerTarget = null;
        _playerDamageable = null;
    }

    public virtual void OnPoolDespawned()
    {
        PlayerTarget = null;
        _playerDamageable = null;
        CancelInvoke();
        StopAllCoroutines();
    }

    /// <summary>
    /// 攻撃を受ける関数
    /// </summary>
    /// <param name="amount">攻撃力</param>
    public virtual void TakeDamage(int amount)
    {
        if (!isActiveAndEnabled || amount <= 0 || CurrentHP <= 0)
        {
            return;
        }

        CurrentHP = Mathf.Max(CurrentHP - amount, 0);

        if (_view != null)
        {
            _view.PlayDamageFlash();
        }

        if (CurrentHP == 0)
        {
            Die();
        }
    }

    protected virtual void Awake()
    {
        _view = GetComponent<EnemyView>();
        if (_statusData == null)
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

    /// <summary>
    /// 死亡処置
    /// </summary>
    protected virtual void Die()
    {
        if (_statusData != null && _statusData.ExperienceReward > 0 && _statusData.ExperiencePickupPrefab != null)
        {
            ExperiencePickup prefab = _statusData.ExperiencePickupPrefab;
            GameObjectPool pool = GameObjectPool.FindPoolFor(gameObject);
            ExperiencePickup pickup = pool != null
                ? pool.Spawn(prefab, transform.position, prefab.transform.rotation)
                : Instantiate(prefab, transform.position, prefab.transform.rotation);

            if (pickup != null)
            {
                pickup.Initialize(_statusData.ExperienceReward);
            }
        }

        GameObjectPool.Despawn(gameObject);
    }

    protected void DamagePlayer()
    {
        _playerDamageable?.TakeDamage(Damage);
    }

    protected bool IsPlayerCollider(Collider2D other)
    {
        return _playerDamageable != null &&
            ReferenceEquals(other.GetComponentInParent<IDamageable>(), _playerDamageable);
    }

    [SerializeField] private EnemyStatusData _statusData;

    private IDamageable _playerDamageable;
    private int _currentMaxHP;
    private int _currentDamage;
    private EnemyView _view;

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
        if (!DealsContactDamage || !isActiveAndEnabled || _statusData == null || CurrentHP <= 0)
        {
            return;
        }

        // 追跡対象のプレイヤーだけを攻撃する。子のColliderにも対応する。
        if (IsPlayerCollider(other))
        {
            DamagePlayer();
        }
    }

    /// <summary>
    /// ステータスの初期化
    /// </summary>
    private void ResetCombatStats()
    {
        _currentMaxHP = _statusData != null ? Mathf.Max(1, _statusData.MaxHP) : 0;
        _currentDamage = _statusData != null ? Mathf.Max(0, _statusData.Damage) : 0;
        CurrentHP = _currentMaxHP;
    }
}
