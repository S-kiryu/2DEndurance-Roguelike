using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾のクラス
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class Projectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)]
    private float lifetime = 5f;

    private readonly HashSet<IDamageable> damagedTargets = new();
    private Rigidbody2D body;
    private int damage;
    private int remainingPenetrations;
    private GameObject owner;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        int attackDamage,
        int penetrationCount,
        GameObject projectileOwner)
    {
        damage = attackDamage;
        remainingPenetrations = penetrationCount;
        owner = projectileOwner;
        body.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.transform.root.gameObject == owner)
        {
            return;
        }

        IDamageable target = other.GetComponentInParent<IDamageable>();

        if (target == null || !damagedTargets.Add(target))
        {
            return;
        }

        target.TakeDamage(damage);

        if (remainingPenetrations <= 0)
        {
            Destroy(gameObject);
            return;
        }

        remainingPenetrations--;
    }
}
