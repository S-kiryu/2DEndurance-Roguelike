using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// playerの入力を受け取るクラス
/// </summary>
public sealed class PlayerInputReader : MonoBehaviour
{
    [SerializeField]
    private InputActionReference moveAction;

    [SerializeField]
    private InputActionReference jumpAction;

    private bool jumpRequested;

    public float MoveX { get; private set; }

    private void OnEnable()
    {
        moveAction.action.performed += OnMove;
        moveAction.action.canceled += OnMove;
        jumpAction.action.performed += OnJump;

        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.performed -= OnMove;
        moveAction.action.canceled -= OnMove;
        jumpAction.action.performed -= OnJump;

        moveAction.action.Disable();
        jumpAction.action.Disable();

        MoveX = 0f;
        jumpRequested = false;
    }

    /// <summary>移動入力を受け取る</summary>
    private void OnMove(InputAction.CallbackContext context)
    {
        MoveX = context.ReadValue<float>();
    }

    /// <summary>ジャンプ入力を受け取る</summary>
    private void OnJump(InputAction.CallbackContext context)
    {
        jumpRequested = true;
    }

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
}