using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 弾のクラス
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public sealed class Projectile : MonoBehaviour, IPoolable
{
    public void OnPoolSpawned()
    {
        // 初回貸出はAwake前の場合がある。
        _body = GetComponent<Rigidbody2D>();
        _damagedTargets.Clear();
        _damage = 0;
        _remainingPenetrations = 0;
        _owner = null;
        _weaponContext = null;
    }

    public void OnPoolDespawned()
    {
        _damagedTargets.Clear();
        _owner = null;
        _weaponContext = null;
        _body.linearVelocity = Vector2.zero;
        _body.angularVelocity = 0f;
    }

    public void Initialize(
        Vector2 direction,
        float speed,
        int attackDamage,
        int penetrationCount,
        GameObject projectileOwner,
        WeaponContext context = null)
    {
        _damage = attackDamage;
        _remainingPenetrations = penetrationCount;
        _owner = projectileOwner;
        _weaponContext = context;
        _body.linearVelocity = direction.normalized * speed;
    }

    private readonly HashSet<IDamageable> _damagedTargets = new();
    private Rigidbody2D _body;
    private Camera _mainCamera;
    private int _damage;
    private int _remainingPenetrations;
    private GameObject _owner;
    private WeaponContext _weaponContext;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
        _body.gravityScale = 0f;
        _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void LateUpdate()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;

            if (_mainCamera == null)
            {
                return;
            }
        }

        // Sceneビューの表示範囲ではなく、ゲーム用カメラの画面内かを判定する。
        Vector3 viewportPosition =
            _mainCamera.WorldToViewportPoint(transform.position);

        if (viewportPosition.x < 0f || viewportPosition.x > 1f ||
            viewportPosition.y < 0f || viewportPosition.y > 1f ||
            viewportPosition.z < _mainCamera.nearClipPlane ||
            viewportPosition.z > _mainCamera.farClipPlane)
        {
            GameObjectPool.Despawn(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        if (_owner != null && other.transform.root.gameObject == _owner)
        {
            return;
        }

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null || !_damagedTargets.Add(target))
        {
            return;
        }

        if (_weaponContext != null)
            _weaponContext.DealDamage(other, _damage);
        else
            target.TakeDamage(_damage);

        if (_remainingPenetrations <= 0)
        {
            GameObjectPool.Despawn(gameObject);
            return;
        }

        _remainingPenetrations--;
    }
}
