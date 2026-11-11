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

    private void Awake()
    {
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
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
        if (targetCamera == null)
        {
            return;
        }

        Vector3 position = transform.position;
        float depth = targetCamera.WorldToViewportPoint(position).z;
        if (depth <= 0f)
        {
            return;
        }

        // 毎フレーム計算し、解像度・アスペクト比・ズームの変更にも追従する。
        Vector3 bottomLeft = targetCamera.ViewportToWorldPoint(new Vector3(0f, 0f, depth));
        Vector3 topRight = targetCamera.ViewportToWorldPoint(new Vector3(1f, 1f, depth));
        Bounds bounds = new Bounds(position, Vector3.zero);
        if (bodyCollider != null && bodyCollider.enabled)
        {
            bounds.Encapsulate(bodyCollider.bounds);
        }
        if (bodyRenderer != null && bodyRenderer.enabled)
        {
            bounds.Encapsulate(bodyRenderer.bounds);
        }

        float minX = bottomLeft.x + position.x - bounds.min.x + padding;
        float maxX = topRight.x - bounds.max.x + position.x - padding;
        float minY = bottomLeft.y + position.y - bounds.min.y + padding;
        float maxY = topRight.y - bounds.max.y + position.y - padding;
        Vector2 clamped = new Vector2(
            ClampAxis(position.x, minX, maxX),
            ClampAxis(position.y, minY, maxY)
        );

        Vector2 velocity = body.linearVelocity;
        // 端に向かう速度だけを止め、内側への移動・落下は妨げない。
        if ((position.x <= minX && velocity.x < 0f) ||
            (position.x >= maxX && velocity.x > 0f))
        {
            velocity.x = 0f;
        }
        if ((position.y <= minY && velocity.y < 0f) ||
            (position.y >= maxY && velocity.y > 0f))
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
