using UnityEngine;

/// <summary>2Dカメラの表示範囲にPlayerの体全体を収める。</summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
[DefaultExecutionOrder(100)]
public sealed class PlayerScreenBounds : MonoBehaviour
{
    [SerializeField] private Camera _targetCamera;
    [SerializeField] private Collider2D _bodyCollider;
    [SerializeField] private SpriteRenderer _bodyRenderer;
    [SerializeField, Min(0f)] private float _padding = 0.02f;

    private Rigidbody2D _body;
    private Vector2 _screenMin;
    private Vector2 _screenMax;

    private void Start()
    {
        Camera screenCamera = _targetCamera != null ? _targetCamera : Camera.main;
        if (screenCamera == null)
        {
            enabled = false;
            return;
        }

        // 固定の2Dカメラなので、画面端は開始時に一度だけ求める。
        float halfHeight = screenCamera.orthographicSize;
        Vector2 halfSize = new Vector2(halfHeight * screenCamera.aspect, halfHeight);
        Vector2 center = screenCamera.transform.position;
        Vector2 margin = Vector2.one * _padding;
        _screenMin = center - halfSize + margin;
        _screenMax = center + halfSize - margin;

        _body = GetComponent<Rigidbody2D>();
        if (_bodyCollider == null)
        {
            _bodyCollider = GetComponent<Collider2D>();
        }
        if (_bodyRenderer == null)
        {
            _bodyRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void LateUpdate()
    {
        Vector3 position = transform.position;
        Bounds bounds = new Bounds(position, Vector3.zero);
        if (_bodyCollider != null && _bodyCollider.enabled)
        {
            bounds.Encapsulate(_bodyCollider.bounds);
        }
        if (_bodyRenderer != null && _bodyRenderer.enabled)
        {
            bounds.Encapsulate(_bodyRenderer.bounds);
        }

        Vector2 min = _screenMin + (Vector2)(position - bounds.min);
        Vector2 max = _screenMax - (Vector2)(bounds.max - position);
        Vector2 clamped = new Vector2(
            ClampAxis(position.x, min.x, max.x),
            ClampAxis(position.y, min.y, max.y)
        );

        Vector2 velocity = _body.linearVelocity;
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
        _body.linearVelocity = velocity;

        if (clamped.x != position.x || clamped.y != position.y)
        {
            _body.position = clamped;
        }
    }

    private static float ClampAxis(float value, float min, float max)
    {
        // 体より画面が小さい場合は中央に寄せる。
        return min <= max ? Mathf.Clamp(value, min, max) : (min + max) * 0.5f;
    }
}
