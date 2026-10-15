using UnityEngine;

/// <summary>
/// playerの移動を制御するクラス
/// </summary>
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerInputReader input;

    [SerializeField]
    private Transform groundCheck;

    [Header("移動関連")]
    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float jumpSpeed = 10f;

    [Header("地面判定")]
    [SerializeField]
    private float groundCheckRadius = 0.15f;

    [SerializeField]
    private LayerMask groundLayer;

    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        MoveHorizontally();
        TryJump();
    }

    /// <summary>
    /// 横方向の移動を制御する
    /// </summary>
    private void MoveHorizontally()
    {
        body.linearVelocity = new Vector2(
            input.MoveX * moveSpeed,
            body.linearVelocity.y
        );
    }

    /// <summary>
    /// ジャンプができるか試す
    /// </summary>
    private void TryJump()
    {
        bool jumpRequested = input.ConsumeJump();

        if (jumpRequested && IsGrounded())
        {
            body.linearVelocity = new Vector2(
                body.linearVelocity.x,
                jumpSpeed
            );
        }
    }
    
    /// <summary>
    /// 地面に立っているかどうかを判定する
    /// </summary>
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }
}