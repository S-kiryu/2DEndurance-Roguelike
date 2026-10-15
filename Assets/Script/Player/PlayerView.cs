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