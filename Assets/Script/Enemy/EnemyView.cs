using System.Collections;
using UnityEngine;

/// <summary>敵の被ダメージ時の見た目を制御する。</summary>
[DisallowMultipleComponent]
public sealed class EnemyView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer damageRenderer;
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
        damageRenderer.color = new Color(1f, 0f, 0f, originalColor.a);
        yield return new WaitForSeconds(damageFlashDuration);
        RestoreColor();
        damageFlash = null;
    }

    private void OnDisable()
    {
        if (damageFlash != null)
        {
            StopCoroutine(damageFlash);
            damageFlash = null;
        }
        // プールに戻った個体が、赤いまま再出現しないようにする。
        RestoreColor();
    }

    private void RestoreColor()
    {
        if (damageRenderer != null)
        {
            damageRenderer.color = originalColor;
        }
    }
}
