using System.Collections;
using UnityEngine;

/// <summary>
/// Playerの見た目を制御する
/// </summary>
public sealed class PlayerView : MonoBehaviour
{
    [SerializeField]
    private Transform visualRoot;

    [SerializeField]
    private Animator animator;

    [Header("被ダメージ演出")]
    [SerializeField]
    private SpriteRenderer damageRenderer;

    [SerializeField, Min(0.01f)]
    private float damageFlashDuration = 0.15f;

    private Color originalColor;
    private Coroutine damageFlash;
    private bool nataEquipped;
    private Sword attackingNata;
    private Coroutine nataAttackRoutine;
    private AnimatorOverrideController nataController;
    private RuntimeAnimatorController originalController;
    private static readonly int NataEquipped = Animator.StringToHash("NataEquipped");
    private static readonly int NataAttack = Animator.StringToHash("Base Layer.NataAttack");
    private static readonly int NataAttackSpeed = Animator.StringToHash("NataAttackSpeed");

    public void SetNataEquipped(bool equipped)
    {
        if (animator == null) return;
        nataEquipped = equipped;
        animator.SetBool(NataEquipped, equipped);
        if (!equipped && attackingNata != null) FinishNataAttack();
    }

    public bool TryPlayNataAttack(Sword weapon, AnimationClip clip, float speed)
    {
        if (!isActiveAndEnabled || !nataEquipped || attackingNata != null ||
            animator == null || !animator.isActiveAndEnabled || clip == null ||
            !animator.HasState(0, NataAttack)) return false;

        if (nataController == null)
        {
            originalController = animator.runtimeAnimatorController;
            nataController = new AnimatorOverrideController(originalController);
            animator.runtimeAnimatorController = nataController;
        }
        nataController["NataAttackPlaceholder"] = clip;
        animator.SetBool(NataEquipped, nataEquipped);
        attackingNata = weapon;
        speed = Mathf.Max(0.01f, speed);
        animator.SetFloat(NataAttackSpeed, speed);
        animator.Play(NataAttack, 0, 0f);
        nataAttackRoutine = StartCoroutine(WaitForNataAttack());
        return true;
    }

    private IEnumerator WaitForNataAttack()
    {
        // Animatorの評価後に終了を確認し、最後のフレームの命中イベントも処理する。
        yield return null;
        while (animator != null && animator.isActiveAndEnabled)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.fullPathHash != NataAttack || state.normalizedTime >= 1f) break;
            yield return null;
        }
        nataAttackRoutine = null;
        FinishNataAttack();
    }

    // PlayerのAnimatorと同じGameObjectへ送られるAnimation Event。
    public void DealNataDamage()
    {
        if (attackingNata != null && attackingNata.isActiveAndEnabled)
            attackingNata.DealAttackDamage();
    }

    public void FinishNataAttack()
    {
        if (object.ReferenceEquals(attackingNata, null) && nataAttackRoutine == null) return;
        if (nataAttackRoutine != null) StopCoroutine(nataAttackRoutine);
        nataAttackRoutine = null;
        Sword weapon = attackingNata;
        attackingNata = null;
        if (weapon != null) weapon.FinishAttack();
        if (animator != null && animator.isActiveAndEnabled)
        {
            bool moving = animator.GetBool("Move");
            bool backwards = animator.GetFloat("Direction") > 0f;
            string state = nataEquipped
                ? (moving ? (backwards ? "NataPlayerMoveBack" : "NataPlayerMove") : "NataIdol")
                : (moving ? (backwards ? "PlayerMoveBack" : "PlayerMove") : "Idol");
            animator.Play("Base Layer." + state, 0, 0f);
        }
    }

    public void CancelNataAttack(Sword weapon)
    {
        if (attackingNata == weapon) FinishNataAttack();
    }

    private void OnDestroy()
    {
        if (nataController == null) return;
        if (animator != null) animator.runtimeAnimatorController = originalController;
        Destroy(nataController);
    }

    private void Awake()
    {
        if (damageRenderer == null)
        {
            Transform root = visualRoot != null ? visualRoot : transform;
            damageRenderer = root.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (damageRenderer != null)
        {
            originalColor = damageRenderer.color;
        }
    }

    private void OnDisable()
    {
        if (attackingNata != null) FinishNataAttack();
        if (damageFlash == null)
        {
            return;
        }

        StopCoroutine(damageFlash);
        damageFlash = null;
        RestoreColor();
    }

    /// <summary>
    /// ダメージを食らったときの処理
    /// </summary>
    public void PlayDamageFlash()
    {
        if (!isActiveAndEnabled || damageRenderer == null)
        {
            return;
        }

        if (damageFlash != null)
        {
            StopCoroutine(damageFlash);
        }

        damageFlash = StartCoroutine(FlashDamage());
    }

    /// <summary>
    /// ダメージを食らった表現をする関数
    /// </summary>
    /// <returns></returns>
    private IEnumerator FlashDamage()
    {
        damageRenderer.color = new Color(1f, 0f, 0f, originalColor.a);

        yield return new WaitForSeconds(damageFlashDuration);

        RestoreColor();
        damageFlash = null;
    }

    /// <summary>
    /// 初期の色に戻す
    /// </summary>
    private void RestoreColor()
    {
        if (damageRenderer != null)
        {
            damageRenderer.color = originalColor;
        }
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

        Vector3 scale = visualRoot.localScale;
        float absoluteScaleX = Mathf.Abs(scale.x);

        scale.x = direction < 0f
            ? -absoluteScaleX
            : absoluteScaleX;

        visualRoot.localScale = scale;
    }

    /// <summary>
    /// 移動をするアニメーション
    /// </summary>
    /// <param name="moveX"></param>
    public void SetMovement(float moveX)
    {
        bool isMoving = Mathf.Abs(moveX) > 0.01f;

        float facingDirection = Mathf.Sign(visualRoot.localScale.x);
        // プレイヤーの向きに対して、移動方向がどちらかを計算する
        float relativeDirection = moveX * facingDirection;

        animator.SetBool("Move", isMoving);
        animator.SetFloat("Direction", relativeDirection);
    }
}
