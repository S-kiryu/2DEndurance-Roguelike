using System.Collections;
using UnityEngine;

/// <summary>敵の被ダメージ時の見た目を制御する。</summary>
[DisallowMultipleComponent]
public sealed class EnemyView : MonoBehaviour
{
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

    [SerializeField] private SpriteRenderer _damageRenderer;
    [SerializeField] private SpriteRenderer _damageFlashOverlay;
    [SerializeField, Min(0.01f)] private float _damageFlashDuration = 0.15f;

    private Color _originalColor;
    private Coroutine _damageFlash;

    private void Awake()
    {
        if (_damageRenderer == null)
        {
            _damageRenderer = GetComponentInChildren<SpriteRenderer>(true);
        }
        if (_damageRenderer != null)
        {
            _originalColor = _damageRenderer.color;
        }
    }

    private IEnumerator FlashDamage()
    {
        if (_damageFlashOverlay == null)
        {
            _damageRenderer.color = new Color(1f, 0f, 0f, _originalColor.a);
        }
        yield return new WaitForSeconds(_damageFlashDuration);
        ClearDamageFlash();
        _damageFlash = null;
    }

    private void LateUpdate()
    {
        if (_damageFlash == null || _damageFlashOverlay == null || _damageRenderer == null)
        {
            return;
        }

        // Animatorが更新したコマと移動で変わった向きを描画前に反映する。
        _damageFlashOverlay.sprite = _damageRenderer.sprite;
        _damageFlashOverlay.flipX = _damageRenderer.flipX;
        _damageFlashOverlay.flipY = _damageRenderer.flipY;
        Color overlayColor = _damageFlashOverlay.color;
        overlayColor.a = _damageRenderer.color.a;
        _damageFlashOverlay.color = overlayColor;
    }

    private void OnDisable()
    {
        if (_damageFlash != null)
        {
            StopCoroutine(_damageFlash);
            _damageFlash = null;
        }
        // プールに戻った個体が、赤いまま再出現しないようにする。
        ClearDamageFlash();
    }

    private void ClearDamageFlash()
    {
        if (_damageFlashOverlay != null)
        {
            Color overlayColor = _damageFlashOverlay.color;
            overlayColor.a = 0f;
            _damageFlashOverlay.color = overlayColor;
        }
        else if (_damageRenderer != null)
        {
            _damageRenderer.color = _originalColor;
        }
    }
}
