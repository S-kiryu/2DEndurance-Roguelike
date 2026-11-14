using System.Collections;
using UnityEngine;

/// <summary>敵の被ダメージ時の見た目を制御する。</summary>
[DisallowMultipleComponent]
public sealed class EnemyView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer damageRenderer;
    [SerializeField] private SpriteRenderer damageFlashOverlay;
    [SerializeField, Min(0.01f)] private float damageFlashDuration = 0.15f;

    private Color originalColor;
    private Coroutine damageFlash;

    private void Awake()
    {
        if (damageRenderer == null)
        {
            damageRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
        if (damageRenderer != null)
        {
            originalColor = damageRenderer.color;
        }
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
        if (damageFlashOverlay == null)
        {
            damageRenderer.color = new Color(1f, 0f, 0f, originalColor.a);
        }
        yield return new WaitForSeconds(damageFlashDuration);
        ClearDamageFlash();
        damageFlash = null;
    }

    private void LateUpdate()
    {
        if (damageFlash == null || damageFlashOverlay == null || damageRenderer == null)
        {
            return;
        }

        // Animatorが更新したコマと移動で変わった向きを描画前に反映する。
        damageFlashOverlay.sprite = damageRenderer.sprite;
        damageFlashOverlay.flipX = damageRenderer.flipX;
        damageFlashOverlay.flipY = damageRenderer.flipY;
        Color overlayColor = damageFlashOverlay.color;
        overlayColor.a = damageRenderer.color.a;
        damageFlashOverlay.color = overlayColor;
    }

    private void OnDisable()
    {
        if (damageFlash != null)
        {
            StopCoroutine(damageFlash);
            damageFlash = null;
        }
        // プールに戻った個体が、赤いまま再出現しないようにする。
        ClearDamageFlash();
    }

    private void ClearDamageFlash()
    {
        if (damageFlashOverlay != null)
        {
            Color overlayColor = damageFlashOverlay.color;
            overlayColor.a = 0f;
            damageFlashOverlay.color = overlayColor;
        }
        else if (damageRenderer != null)
        {
            damageRenderer.color = originalColor;
        }
    }
}
