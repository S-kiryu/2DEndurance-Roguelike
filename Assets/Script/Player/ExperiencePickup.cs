using UnityEngine;

/// <summary>敵が落とし、プレイヤーとの接触で回収される経験値。</summary>
[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public sealed class ExperiencePickup : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayers;
    [SerializeField, Min(0.01f)] private float fallSpeed = 7f;

    private Rigidbody2D body;
    private float landingHeight;
    private int experience;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int amount)
    {
        experience = amount;
        Vector2 position = body.position;
        float rayStartY = position.y + 0.5f;
        Camera camera = Camera.main;
        if (camera != null && camera.orthographic)
        {
            Vector2 center = camera.transform.position;
            float horizontalRange = camera.orthographicSize * camera.aspect * 0.9f;
            position.x = Mathf.Clamp(position.x, center.x - horizontalRange, center.x + horizontalRange);
            rayStartY = center.y + camera.orthographicSize;
            position.y = Mathf.Min(position.y, rayStartY - 0.35f);
        }

        RaycastHit2D ground = Physics2D.Raycast(new Vector2(position.x, rayStartY), Vector2.down,
            Mathf.Infinity, groundLayers);
        landingHeight = ground.collider != null ? ground.point.y + 0.35f : position.y;
        position.y = Mathf.Max(position.y, landingHeight);
        body.position = position;
    }

    private void FixedUpdate()
    {
        Vector2 position = body.position;
        if (position.y > landingHeight)
        {
            position.y = Mathf.MoveTowards(position.y, landingHeight, fallSpeed * Time.fixedDeltaTime);
            body.MovePosition(position);
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

    private void TryCollect(Collider2D other)
    {
        if (experience <= 0 || Time.timeScale <= 0f)
        {
            return;
        }

        PlayerPresenter player = other.GetComponentInParent<PlayerPresenter>();
        if (player != null && player.isActiveAndEnabled && player.Model.TryGainExperience(experience))
        {
            experience = 0;
            Destroy(gameObject);
        }
    }
}
