using UnityEngine;

/// <summary>武器1個につき1個の参照窓口。再初期化すると新しいContextに入れ替わる。</summary>
public sealed class WeaponContext
{
    public Weapon Weapon { get; }
    public WeaponData Data => Weapon.WeaponData;
    public GameObject Owner { get; }
    public PlayerModel OwnerStats { get; }
    public GameObjectPool Pool { get; }
    public Transform Transform => Weapon.transform;
    public int StarLevel => Weapon.StarLevel;
    public bool IsActive => Weapon != null && Weapon.Context == this && Weapon.isActiveAndEnabled;

    internal WeaponContext(Weapon weapon, PlayerModel ownerStats, GameObjectPool pool)
    {
        Weapon = weapon;
        Owner = weapon.transform.root.gameObject;
        OwnerStats = ownerStats;
        Pool = pool;
    }

    public Collider2D[] FindTargets(Vector2 center, float radius, LayerMask layers)
    {
        return Physics2D.OverlapCircleAll(center, Mathf.Max(0f, radius),
            layers.value == 0 ? Physics2D.AllLayers : layers.value);
    }

    public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
    {
        return IsActive && Pool != null && Pool.IsReady ? Pool.Spawn(prefab, position, rotation) : null;
    }

    /// <summary>効果からの追加ダメージ。命中効果を再発火せず、無限連鎖を防ぐ。</summary>
    public void DealEffectDamage(Collider2D target, int amount)
    {
        if (IsActive) DealDamage(target, amount, false);
    }

    // 発射済みの弾は武器が無効でも通常ダメージを与える。追加効果は有効なContextだけが処理する。
    internal void DealDamage(Collider2D collider, int amount, bool notifyEffects = true)
    {
        if (collider == null || amount <= 0 || !collider.gameObject.activeInHierarchy ||
            (Owner != null && collider.transform.root.gameObject == Owner)) return;
        IDamageable target = collider.GetComponentInParent<IDamageable>();
        if (target == null) return;
        EnemyBase enemy = target as EnemyBase;
        if (enemy != null && enemy.CurrentHP <= 0) return;
        Vector2 point = collider.bounds.center;
        target.TakeDamage(amount);
        bool killed = enemy != null && enemy.CurrentHP == 0;
        if (notifyEffects && IsActive)
            Weapon.NotifyHit(new WeaponHit(target, collider, point, amount, killed));
    }
}

/// <summary>今回の命中の情報。共有の「最後の敵」変数を使わず通知ごとに渡す。</summary>
public readonly struct WeaponHit
{
    public IDamageable Target { get; }
    public Collider2D Collider { get; }
    public Vector2 Position { get; }
    public int Damage { get; }
    public bool WasKilled { get; }

    public WeaponHit(IDamageable target, Collider2D collider, Vector2 position, int damage, bool wasKilled)
    {
        Target = target;
        Collider = collider;
        Position = position;
        Damage = damage;
        WasKilled = wasKilled;
    }
}
