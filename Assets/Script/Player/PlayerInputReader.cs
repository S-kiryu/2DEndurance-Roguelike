using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// playerの入力を受け取るクラス
/// </summary>
public sealed class PlayerInputReader : MonoBehaviour
{
    /// <summary>ジャンプ入力を消費する</summary>
    public bool ConsumeJump()
    {
        if (!jumpRequested)
        {
            return false;
        }

        jumpRequested = false;
        return true;
    }

    [SerializeField]
    private InputActionReference moveLeftAction;

    [SerializeField]
    private InputActionReference moveRightAction;

    [SerializeField]
    private InputActionReference jumpAction;
    private bool leftPressed;
    private bool rightPressed;
    private bool jumpRequested;
    private float lastPressedDirection;

    public float MoveX
    {
        get
        {
            if (leftPressed && rightPressed)
            {
                return lastPressedDirection;
            }

            if (leftPressed)
            {
                return -1f;
            }

            if (rightPressed)
            {
                return 1f;
            }

            return 0f;
        }
    }

    private void OnEnable()
    {
        moveLeftAction.action.performed += OnMoveLeft;
        moveLeftAction.action.canceled += OnMoveLeft;

        moveRightAction.action.performed += OnMoveRight;
        moveRightAction.action.canceled += OnMoveRight;

        jumpAction.action.performed += OnJump;

        moveLeftAction.action.Enable();
        moveRightAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        moveLeftAction.action.performed -= OnMoveLeft;
        moveLeftAction.action.canceled -= OnMoveLeft;

        moveRightAction.action.performed -= OnMoveRight;
        moveRightAction.action.canceled -= OnMoveRight;

        jumpAction.action.performed -= OnJump;

        moveLeftAction.action.Disable();
        moveRightAction.action.Disable();
        jumpAction.action.Disable();

        leftPressed = false;
        rightPressed = false;
        jumpRequested = false;
        lastPressedDirection = 0f;
    }

    private void OnMoveLeft(InputAction.CallbackContext context)
    {
        leftPressed = context.ReadValueAsButton();

        if (context.performed)
        {
            lastPressedDirection = -1f;
        }
    }

    private void OnMoveRight(InputAction.CallbackContext context)
    {
        rightPressed = context.ReadValueAsButton();

        if (context.performed)
        {
            lastPressedDirection = 1f;
        }
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }
}