using UnityEngine;

/// <summary>2Dカメラの表示範囲にPlayerの体全体を収める。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(100)]
public sealed class PlayerScreenBounds : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Collider2D bodyCollider;
    [SerializeField] private SpriteRenderer bodyRenderer;
    [SerializeField, Min(0f)] private float padding = 0.02f;

    private Rigidbody2D body;
    private Vector2 screenMin;
    private Vector2 screenMax;

    private void Start()
    {
        Camera screenCamera = targetCamera != null ? targetCamera : Camera.main;
        if (screenCamera == null)
        {
            enabled = false;
            return;
        }

        // 固定の2Dカメラなので、画面端は開始時に一度だけ求める。
        float halfHeight = screenCamera.orthographicSize;
        Vector2 halfSize = new Vector2(halfHeight * screenCamera.aspect, halfHeight);
        Vector2 center = screenCamera.transform.position;
        Vector2 margin = Vector2.one * padding;
        screenMin = center - halfSize + margin;
        screenMax = center + halfSize - margin;

        body = GetComponent<Rigidbody2D>();
        if (bodyCollider == null)
        {
            bodyCollider = GetComponent<Collider2D>();
        }
        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void LateUpdate()
    {
        Vector3 position = transform.position;
        Bounds bounds = new Bounds(position, Vector3.zero);
        if (bodyCollider != null && bodyCollider.enabled)
        {
            bounds.Encapsulate(bodyCollider.bounds);
        }
        if (bodyRenderer != null && bodyRenderer.enabled)
        {
            bounds.Encapsulate(bodyRenderer.bounds);
        }

        Vector2 min = screenMin + (Vector2)(position - bounds.min);
        Vector2 max = screenMax - (Vector2)(bounds.max - position);
        Vector2 clamped = new Vector2(
            ClampAxis(position.x, min.x, max.x),
            ClampAxis(position.y, min.y, max.y)
        );

        Vector2 velocity = body.linearVelocity;
        // 端に向かう速度だけを止め、内側への移動・落下は妨げない。
        if ((position.x <= min.x && velocity.x < 0f) ||
            (position.x >= max.x && velocity.x > 0f))
        {
            velocity.x = 0f;
        }
        if ((position.y <= min.y && velocity.y < 0f) ||
            (position.y >= max.y && velocity.y > 0f))
        {
            velocity.y = 0f;
        }
        body.linearVelocity = velocity;

        if (clamped.x != position.x || clamped.y != position.y)
        {
            body.position = clamped;
        }
    }

    private static float ClampAxis(float value, float min, float max)
    {
        // 体より画面が小さい場合は中央に寄せる。
        return min <= max ? Mathf.Clamp(value, min, max) : (min + max) * 0.5f;
    }
}
