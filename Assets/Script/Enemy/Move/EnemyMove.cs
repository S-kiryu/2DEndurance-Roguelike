using UnityEngine;

/// <summary>
/// 敵の移動
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMove : MonoBehaviour
{
    public void Initialize(GameObject playerObject, float speed)
    {
        if (playerObject == null)
        {
            _player = null;
            Debug.LogWarning($"{name}: Playerが設定されていません。");
            return;
        }

        _player = playerObject.transform;
        _moveSpeed = speed;
    }

    public void SetMovementPaused(bool paused)
    {
        if (_movementPaused == paused)
        {
            return;
        }

        _movementPaused = paused;
        if (paused)
        {
            StopMoving();
        }
    }

    public void FaceDirection(float direction)
    {
        if (_spriteRenderer != null && direction != 0f)
        {
            _spriteRenderer.flipX = _spriteFacesLeft
                ? direction > 0f
                : direction < 0f;
        }
    }

    [SerializeField, Min(0f)]
    private float _moveSpeed = 2f;
    [SerializeField] private bool _spriteFacesLeft;
    private SpriteRenderer _spriteRenderer;

    private Rigidbody2D _body;
    private Transform _player;
    private bool _movementPaused;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDisable()
    {
        _player = null;
        _movementPaused = false;
        if (_body != null)
        {
            StopMoving();
            _body.angularVelocity = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (_player == null || _movementPaused)
        {
            StopMoving();
            return;
        }
        Move(_player);
    }

    /// <summary>
    /// 指定した位置まで移動する
    /// </summary>
    /// <param name="pos"></param>
    private void Move(Transform pos)
    {
        float difference =
            pos.position.x - transform.position.x;

        float direction = Mathf.Sign(difference);

        _body.linearVelocity = new Vector2(
            direction * _moveSpeed,
            _body.linearVelocity.y
        );

        FaceDirection(difference);
    }

    /// <summary>
    /// 移動の停止処理
    /// </summary>
    private void StopMoving()
    {
        _body.linearVelocity = new Vector2(
            0f,
            _body.linearVelocity.y
        );
    }
}
