using UnityEngine;

/// <summary>
/// playerの移動を制御するクラス
/// </summary>
[RequireComponent(typeof(PlayerScreenBounds))]
public sealed class PlayerMovement : MonoBehaviour
{

    public void Initialize(PlayerModel playerModel)
    {
        _stats = playerModel;
    }

    [SerializeField]
    private PlayerInputReader _input;

    [SerializeField]
    private PlayerView _view;

    [SerializeField]
    private Transform _groundCheck;

    [Header("地面判定")]
    [SerializeField]
    private float _groundCheckRadius = 0.15f;

    [SerializeField]
    private LayerMask _groundLayer;

    private Rigidbody2D _body;
    private PlayerModel _stats;
    private Camera _mainCamera;

    private void Awake()
    {
        _body = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
    }

    private void FixedUpdate()
    {
        MoveHorizontally();
        TryJump();
        UpdateView();
    }

    private void UpdateView()
    {
        float mouseDirection = _input.GetMouseDirectionX(
            transform.position,
            _mainCamera
        );

        _view.SetFacing(mouseDirection);
    }

    /// <summary>
    /// 横方向の移動を制御する
    /// </summary>
    private void MoveHorizontally()
    {
        _body.linearVelocity = new Vector2(
            _input.MoveX * _stats.MoveSpeed,
            _body.linearVelocity.y
        );
    }

    /// <summary>
    /// ジャンプができるか試す
    /// </summary>
    private void TryJump()
    {
        bool jumpRequested = _input.ConsumeJump();

        if (jumpRequested && IsGrounded())
        {
            _body.linearVelocity = new Vector2(
                _body.linearVelocity.x,
                _stats.JumpSpeed
            );
        }
    }
    
    /// <summary>
    /// 地面に立っているかどうかを判定する
    /// </summary>
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );
    }
}
