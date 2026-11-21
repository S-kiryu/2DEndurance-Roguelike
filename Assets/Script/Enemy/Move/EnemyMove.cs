using System;
using UnityEngine;

/// <summary>
/// 敵の移動
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMove : MonoBehaviour
{
    public bool IsDashing => _dashVelocityX != 0f;

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

    /// <summary>
    /// 指定方向に突進し、物理更新ごとに移動予定の符号付き距離を通知する。
    /// </summary>
    public void StartDash(float direction, float speed, float distance, Action<float> onStep = null)
    {
        if (!isActiveAndEnabled || direction == 0f || speed <= 0f || distance <= 0f)
        {
            return;
        }

        _dashVelocityX = Mathf.Sign(direction) * Mathf.Min(speed, Physics2D.maxTranslationSpeed);
        _dashDistanceRemaining = distance;
        _onDashStep = onStep;
        FaceDirection(direction);
    }

    public void StopDash()
    {
        if (!IsDashing)
        {
            return;
        }

        _dashVelocityX = 0f;
        _dashDistanceRemaining = 0f;
        _onDashStep = null;
        StopMoving();
    }

    public void FaceDirection(float direction)
    {
        if (_spriteRenderer == null || direction == 0f)
        {
            return;
        }

        bool flipX = _spriteFacesLeft ? direction > 0f : direction < 0f;
        _spriteRenderer.flipX = flipX;
        if (_shadowToMirror != null)
        {
            Vector3 shadowPosition = _shadowToMirror.localPosition;
            shadowPosition.x = flipX ? -_unflippedShadowX : _unflippedShadowX;
            _shadowToMirror.localPosition = shadowPosition;
        }
    }

    [SerializeField, Min(0f)]
    private float _moveSpeed = 2f;
    [SerializeField] private bool _spriteFacesLeft;
    [Tooltip("反転していないときの位置を設定した影の Transform")]
    [SerializeField] private Transform _shadowToMirror;
    private SpriteRenderer _spriteRenderer;
    private float _unflippedShadowX;

    private Rigidbody2D _body;
    private Transform _player;
    private bool _movementPaused;
    private float _dashVelocityX;
    private float _dashDistanceRemaining;
    private Action<float> _onDashStep;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_shadowToMirror != null)
        {
            _unflippedShadowX = _shadowToMirror.localPosition.x;
        }
    }

    private void OnDisable()
    {
        StopDash();
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
        if (IsDashing)
        {
            if (_dashDistanceRemaining > 0f)
            {
                AdvanceDash();
                return;
            }

            StopDash();
        }

        if (_player == null || _movementPaused)
        {
            StopMoving();
            return;
        }
        Move(_player);
    }

    private void AdvanceDash()
    {
        float deltaTime = Time.fixedDeltaTime;
        float distance = Mathf.Min(Mathf.Abs(_dashVelocityX) * deltaTime, _dashDistanceRemaining);
        _dashDistanceRemaining -= distance;
        float step = Mathf.Sign(_dashVelocityX) * distance;
        _body.linearVelocity = new Vector2(step / deltaTime, _body.linearVelocity.y);
        _onDashStep?.Invoke(step);
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
