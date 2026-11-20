using UnityEngine;

/// <summary>敵が落とし、プレイヤーとの接触で回収される経験値。</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class ExperiencePickup : MonoBehaviour
{
    public void Initialize(int amount)
    {
        _experience = amount;
        Vector2 position = transform.position;

        RaycastHit2D ground = Physics2D.Raycast(position + Vector2.up * 0.5f, Vector2.down,
            Mathf.Infinity, _groundLayers);
        _landingHeight = ground.collider != null ? ground.point.y + 0.35f : position.y;
        _body.position = new Vector2(position.x, Mathf.Max(position.y, _landingHeight));
    }

    [SerializeField] private LayerMask _groundLayers;
    [SerializeField, Min(0.01f)] private float _fallSpeed = 7f;

    private Rigidbody2D _body;
    private float _landingHeight;
    private int _experience;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector2 position = _body.position;
        if (position.y > _landingHeight)
        {
            position.y = Mathf.MoveTowards(position.y, _landingHeight, _fallSpeed * Time.fixedDeltaTime);
            _body.MovePosition(position);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    /// <summary>
    /// 取った時の処理
    /// </summary>
    /// <param name="other"></param>
    private void TryCollect(Collider2D other)
    {
        if (_experience <= 0 || Time.timeScale <= 0f)
        {
            return;
        }

        PlayerPresenter player = other.GetComponentInParent<PlayerPresenter>();
        if (player != null && player.isActiveAndEnabled && player.Model.TryGainExperience(_experience))
        {
            _experience = 0;
            GameObjectPool.Despawn(gameObject);
        }
    }
}
