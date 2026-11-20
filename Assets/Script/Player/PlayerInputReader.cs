using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// playerの入力を受け取るクラス
/// </summary>
public sealed class PlayerInputReader : MonoBehaviour
{
    /// <summary>
    /// 保留中のジャンプ入力を一度だけ取り出す。停止中は入力を破棄する。
    /// </summary>
    public bool ConsumeJump()
    {
        if (Time.timeScale <= 0f)
        {
            _jumpRequested = false;
            return false;
        }

        if (!_jumpRequested)
        {
            return false;
        }

        _jumpRequested = false;
        return true;
    }

    /// <summary>
    /// 移動の数値　1右、-1左、0待機
    /// </summary>
    public float MoveX
    {
        get
        {
            if (_leftPressed && _rightPressed)
            {
                return _lastPressedDirection;
            }

            if (_leftPressed)
            {
                return -1f;
            }

            if (_rightPressed)
            {
                return 1f;
            }

            return 0f;
        }
    }

    /// <summary>
    /// 指定したワールド座標を基準にしたマウスの方向。右なら1、左なら-1、
    /// マウスやカメラが取得できない場合、同じX座標にある場合は0を返す。
    /// </summary>
    public float GetMouseDirectionX(Vector3 targetWorldPosition, Camera worldCamera)
    {
        if (Mouse.current == null || worldCamera == null)
        {
            return 0f;
        }

        float mouseX = Mouse.current.position.ReadValue().x;
        float targetScreenX = worldCamera.WorldToScreenPoint(targetWorldPosition).x;

        if (Mathf.Approximately(mouseX, targetScreenX))
        {
            return 0f;
        }

        return mouseX < targetScreenX ? -1f : 1f;
    }

    /// <summary>
    /// ジャンプのキャンセル処理
    /// </summary>
    public void ClearJumpRequest()
    {
        _jumpRequested = false;
    }

    [SerializeField,Tooltip("左移動の入力アクション")]
    private InputActionReference _moveLeftAction;

    [SerializeField,Tooltip("右移動の入力アクション")]
    private InputActionReference _moveRightAction;

    [SerializeField,Tooltip("ジャンプの入力アクション")]
    private InputActionReference _jumpAction;

    private bool _leftPressed;
    private bool _rightPressed;
    private bool _jumpRequested;
    private float _lastPressedDirection;

    private void OnEnable()
    {
        _moveLeftAction.action.performed += OnMoveLeft;
        _moveLeftAction.action.canceled += OnMoveLeft;

        _moveRightAction.action.performed += OnMoveRight;
        _moveRightAction.action.canceled += OnMoveRight;

        _jumpAction.action.performed += OnJump;

        _moveLeftAction.action.Enable();
        _moveRightAction.action.Enable();
        _jumpAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveLeftAction.action.performed -= OnMoveLeft;
        _moveLeftAction.action.canceled -= OnMoveLeft;

        _moveRightAction.action.performed -= OnMoveRight;
        _moveRightAction.action.canceled -= OnMoveRight;

        _jumpAction.action.performed -= OnJump;

        _moveLeftAction.action.Disable();
        _moveRightAction.action.Disable();
        _jumpAction.action.Disable();

        _leftPressed = false;
        _rightPressed = false;
        _jumpRequested = false;
        _lastPressedDirection = 0f;
    }

    /// <summary>
    /// 左側に移動した際の処理
    /// </summary>
    /// <param name="context"></param>
    private void OnMoveLeft(InputAction.CallbackContext context)
    {
        _leftPressed = context.ReadValueAsButton();

        if (context.performed)
        {
            _lastPressedDirection = -1f;
        }
    }

    /// <summary>
    /// 右側に移動した際の処理
    /// </summary>
    private void OnMoveRight(InputAction.CallbackContext context)
    {
        _rightPressed = context.ReadValueAsButton();

        if (context.performed)
        {
            _lastPressedDirection = 1f;
        }
    }

    /// <summary>
    /// ジャンプ処理
    /// </summary>
    /// <param name="context"></param>
    private void OnJump(InputAction.CallbackContext context)
    {
        _jumpRequested = Time.timeScale > 0f;
    }
}
