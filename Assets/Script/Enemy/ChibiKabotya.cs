using System.Collections;
using UnityEngine;

/// <summary>口から飛び出して着地したあとはプレイヤーを追いかける。</summary>
[RequireComponent(typeof(EnemyMove))]
public sealed class ChibiKabotya : EnemyBase
{
    private const float LaunchDuration = 0.28f;
    private const float LaunchHeight = 0.45f;

    [SerializeField] private GameObject _shadow;

    private EnemyMove _movement;

    protected override bool DealsContactDamage => false;

    public void Launch(Vector3 landingPosition)
    {
        _movement.SetMovementPaused(true);
        _movement.FaceDirection(landingPosition.x - transform.position.x);
        if (_shadow != null)
        {
            _shadow.SetActive(false);
        }
        StartCoroutine(FlyTo(landingPosition));
    }

    public override void OnPoolSpawned()
    {
        base.OnPoolSpawned();
        if (_shadow != null)
        {
            _shadow.SetActive(true);
        }
    }

    protected override void Awake()
    {
        _movement = GetComponent<EnemyMove>();
        base.Awake();
    }

    private IEnumerator FlyTo(Vector3 landingPosition)
    {
        Vector3 origin = transform.position;
        float elapsed = 0f;
        while (elapsed < LaunchDuration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / LaunchDuration);
            float arc = 4f * LaunchHeight * progress * (1f - progress);
            transform.position = Vector3.Lerp(origin, landingPosition, progress) + Vector3.up * arc;
        }

        if (_shadow != null)
        {
            _shadow.SetActive(true);
        }
        _movement.SetMovementPaused(false);
    }
}
