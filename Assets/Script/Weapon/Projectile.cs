using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾のクラス
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class Projectile : MonoBehaviour
{
    private readonly HashSet<IDamageable> damagedTargets = new();
    private Rigidbody2D body;
    private Camera mainCamera;
    private int damage;
    private int remainingPenetrations;
    private GameObject owner;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        body.gravityScale = 0f;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                return;
            }
        }

        // Sceneビューの表示範囲ではなく、ゲーム用カメラの画面内かを判定する。
        Vector3 viewportPosition =
            mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPosition.x < 0f || viewportPosition.x > 1f ||
            viewportPosition.y < 0f || viewportPosition.y > 1f ||
            viewportPosition.z < mainCamera.nearClipPlane ||
            viewportPosition.z > mainCamera.farClipPlane)
        {
            Destroy(gameObject);
        }
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
