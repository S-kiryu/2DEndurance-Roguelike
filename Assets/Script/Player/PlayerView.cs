using System.Collections;
using UnityEngine;

/// <summary>
/// Playerの見た目を制御する
/// </summary>
public sealed class PlayerView : MonoBehaviour
{
    /// <summary>
    /// ナタを持った姿に変える
    /// </summary>
    internal void EquipNata()
    {
        if (_nataEquipped) return;
        _nataEquipped = true;
        if (_animator != null && _animator.isActiveAndEnabled)
            _animator.SetBool(_nataEquippedHash, true);
    }

    public bool TryPlayNataAttack(Sword weapon, float speed)
    {
        if (!isActiveAndEnabled || !_nataEquipped || _attackingNata != null ||
            _animator == null || !_animator.isActiveAndEnabled || !_animator.HasState(0, _nataAttackHash))
            return false;

        _attackingNata = weapon;
        _animator.SetFloat(_nataAttackSpeedHash, Mathf.Max(0.01f, speed));
        _animator.Play(_nataAttackHash, 0, 0f);
        _nataAttackRoutine = StartCoroutine(WaitForNataAttack());
        return true;
    }

    // PlayerのAnimatorと同じGameObjectへ送られるAnimation Event。
    public void DealNataDamage()
    {
        if (_attackingNata != null && _attackingNata.isActiveAndEnabled)
            _attackingNata.DealAttackDamage();
    }

    private void FinishNataAttack()
    {
        if (object.ReferenceEquals(_attackingNata, null) && _nataAttackRoutine == null) return;
        if (_nataAttackRoutine != null) StopCoroutine(_nataAttackRoutine);
        _nataAttackRoutine = null;
        Sword weapon = _attackingNata;
        _attackingNata = null;
        if (weapon != null) weapon.FinishAttack();
        if (_animator != null && _animator.isActiveAndEnabled)
        {
            bool moving = _animator.GetBool("Move");
            bool backwards = _animator.GetFloat("Direction") > 0f;
            string state = moving ? (backwards ? "NataPlayerMoveBack" : "NataPlayerMove") : "NataIdol";
            _animator.Play("Base Layer." + state, 0, 0f);
        }
    }

    public void CancelNataAttack(Sword weapon)
    {
        if (_attackingNata == weapon) FinishNataAttack();
    }

    /// <summary>
    /// ダメージを食らったときの処理
    /// </summary>
    public void PlayDamageFlash()
    {
        if (!isActiveAndEnabled || _damageRenderer == null)
        {
            return;
        }

        if (_damageFlash != null)
        {
            StopCoroutine(_damageFlash);
        }

        _damageFlash = StartCoroutine(FlashDamage());
    }

    /// <summary>
    /// Playerと子オブジェクトの向きを変更する
    /// </summary>
    public void SetFacing(float direction)
    {
        if (Mathf.Approximately(direction, 0f))
        {
            return;
        }

        Vector3 scale = _visualRoot.localScale;
        float absoluteScaleX = Mathf.Abs(scale.x);

        scale.x = direction < 0f
            ? -absoluteScaleX
            : absoluteScaleX;

        _visualRoot.localScale = scale;
    }

    /// <summary>
    /// 移動をするアニメーション
    /// </summary>
    /// <param name="moveX"></param>
    public void SetMovement(float moveX)
    {
        bool isMoving = Mathf.Abs(moveX) > 0.01f;

        float facingDirection = Mathf.Sign(_visualRoot.localScale.x);
        // プレイヤーの向きに対して、移動方向がどちらかを計算する
        float relativeDirection = moveX * facingDirection;

        _animator.SetBool("Move", isMoving);
        _animator.SetFloat("Direction", relativeDirection);
    }

    [SerializeField]
    private Transform _visualRoot;

    [SerializeField]
    private Animator _animator;

    [Header("被ダメージ演出")]
    [SerializeField]
    private SpriteRenderer _damageRenderer;

    [SerializeField, Min(0.01f)]
    private float _damageFlashDuration = 0.15f;

    private Color _originalColor;
    private Coroutine _damageFlash;
    private bool _nataEquipped;
    private Sword _attackingNata;
    private Coroutine _nataAttackRoutine;
    private static readonly int _nataEquippedHash = Animator.StringToHash("NataEquipped");
    private static readonly int _nataAttackHash = Animator.StringToHash("Base Layer.NataAttack");
    private static readonly int _nataAttackSpeedHash = Animator.StringToHash("NataAttackSpeed");

    private IEnumerator WaitForNataAttack()
    {
        // Animatorの評価後に終了を確認し、最後のフレームの命中イベントも処理する。
        yield return null;
        while (_animator != null && _animator.isActiveAndEnabled)
        {
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.fullPathHash != _nataAttackHash || state.normalizedTime >= 1f) break;
            yield return null;
        }
        _nataAttackRoutine = null;
        FinishNataAttack();
    }

    private void Awake()
    {
        if (_damageRenderer == null)
        {
            Transform root = _visualRoot != null ? _visualRoot : transform;
            _damageRenderer = root.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (_damageRenderer != null)
        {
            _originalColor = _damageRenderer.color;
        }
    }

    private void OnEnable()
    {
        if (_nataEquipped && _animator != null)
            _animator.SetBool(_nataEquippedHash, true);
    }

    private void OnDisable()
    {
        if (_attackingNata != null) FinishNataAttack();
        if (_damageFlash == null)
        {
            return;
        }

        StopCoroutine(_damageFlash);
        _damageFlash = null;
        RestoreColor();
    }

    /// <summary>
    /// ダメージを食らった表現をする関数
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlashDamage()
    {
        _damageRenderer.color = new Color(1f, 0f, 0f, _originalColor.a);

        yield return new WaitForSeconds(_damageFlashDuration);

        RestoreColor();
        _damageFlash = null;
    }

    /// <summary>
    /// 初期の色に戻す
    /// </summary>
    private void RestoreColor()
    {
        if (_damageRenderer != null)
        {
            _damageRenderer.color = _originalColor;
        }
    }
}
