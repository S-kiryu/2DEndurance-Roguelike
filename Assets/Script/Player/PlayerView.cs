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
        if (damageFlash == null)
        {
            return;
        }

        StopCoroutine(damageFlash);
        damageFlash = null;
        RestoreColor();
    }

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

    private IEnumerator FlashDamage()
    {
        damageRenderer.color = new Color(1f, 0f, 0f, originalColor.a);

        yield return new WaitForSeconds(damageFlashDuration);

        RestoreColor();
        damageFlash = null;
    }

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
}
