using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近接武器のクラス
/// </summary>
public class Sword : Weapon
{
    /// <summary>
    /// 剣が敵に当たるフレームから Animation Event で呼ぶ。
    /// </summary>
    public void DealAttackDamage()
    {
        if (_isAttacking)
        {
            if (_swordData.UsePlayerAnimation)
            {
                if (_dealtPlayerAttackDamage) return;
                _dealtPlayerAttackDamage = true;
            }
            int hitCount = 1 + AdditionalHits;

            for (int i = 0; i < hitCount; i++)
            {
                DealDamage();
            }
        }
    }

    /// <summary>
    /// 攻撃アニメーションの最終フレームから Animation Event で呼ぶ。
    /// </summary>
    public void FinishAttack()
    {
        bool finished = _isAttacking;
        if (_visualRoot != null)
        {
            _visualRoot.localRotation = _idleRotation;
        }

        SetVisualVisible(false);
        _isAttacking = false;
        if (finished) NotifyAttackFinished();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_ownerView != null) _ownerView.CancelNataAttack(this);
        _isAttacking = false;

        if (_animator != null)
        {
            _animator.ResetTrigger(_attackTriggerHash);
        }

        if (_visualRoot != null)
        {
            _visualRoot.localRotation = _idleRotation;
        }

        SetVisualVisible(false);
    }

    protected override bool IsCompatibleData(WeaponData weaponData)
    {
        return weaponData is SwordData;
    }

    protected override string GetDataError(WeaponData weaponData)
    {
        return _attackHitbox == null ? "武器Prefabに攻撃判定用のColliderを設定してください。" : null;
    }

    protected override void OnInitialized()
    {
        _swordData = (SwordData)Data;
        _ownerView = GetComponentInParent<PlayerView>();
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <returns></returns>
    protected override bool TryAttack()
    {
        if (_isAttacking || !HasTargetInRange())
        {
            return false;
        }

        if (_swordData.UsePlayerAnimation)
        {
            if (_ownerView == null || !_ownerView.TryPlayNataAttack(this, EffectiveAttackSpeedMultiplier))
                return false;
            _isAttacking = true;
            _dealtPlayerAttackDamage = false;
            NotifyAttackStarted();
            return true;
        }

        if (_animator == null) return false;
        _isAttacking = true;
        NotifyAttackStarted();
        SetVisualVisible(true);
        _animator.ResetTrigger(_attackTriggerHash);
        _animator.SetTrigger(_attackTriggerHash);
        return true;
    }

    [Header("攻撃モーション")]
    [SerializeField]
    private Transform _visualRoot;

    [Header("攻撃判定")]
    [Tooltip("武器Prefab直下の攻撃判定。Edit Colliderで形を調整できます。")]
    [SerializeField]
    private PolygonCollider2D _attackHitbox;

    private Animator _animator;

    private static readonly int _attackTriggerHash = Animator.StringToHash("Attack");
    private readonly HashSet<IDamageable> _hitTargets = new();
    private readonly List<Collider2D> _targetsInRange = new();
    private Vector3 _hitboxBasePosition;
    private Vector3 _hitboxBaseScale;
    private float _hitboxRangeMultiplier = 1f;
    private SpriteRenderer[] _visualRenderers;
    private SwordData _swordData;
    private Quaternion _idleRotation;
    private bool _isAttacking;
    private PlayerView _ownerView;
    private bool _dealtPlayerAttackDamage;

    private void Awake()
    {
        if (_visualRoot == null)
        {
            _visualRoot = transform;
        }

        if (_animator == null)
        {
            _animator = GetComponent<Animator>();
        }

        if (_attackHitbox != null)
        {
            _attackHitbox.enabled = false;
            _hitboxBasePosition = _attackHitbox.transform.localPosition;
            _hitboxBaseScale = _attackHitbox.transform.localScale;
        }

        _idleRotation = _visualRoot.localRotation;
        _visualRenderers = _visualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        SetVisualVisible(false);
    }

    private bool HasTargetInRange()
    {
        FindTargetsInRange();

        foreach (Collider2D targetCollider in _targetsInRange)
        {
            if (GetDamageable(targetCollider) != null)
            {
                return true;
            }
        }

        return false;
    }

    private void DealDamage()
    {
        FindTargetsInRange();
        _hitTargets.Clear();

        foreach (Collider2D targetCollider in _targetsInRange)
        {
            IDamageable target = GetDamageable(targetCollider);

            if (target != null && _hitTargets.Add(target))
            {
                Context.DealDamage(targetCollider, CalculateDamage());
            }
        }
    }

    private void FindTargetsInRange()
    {
        _targetsInRange.Clear();
        float rangeMultiplier = WeaponRangeMultiplier;
        if (rangeMultiplier <= 0f)
        {
            return;
        }

        if (_hitboxRangeMultiplier != rangeMultiplier)
        {
            Transform hitbox = _attackHitbox.transform;
            hitbox.localPosition = _hitboxBasePosition * rangeMultiplier;
            hitbox.localScale = Vector3.Scale(_hitboxBaseScale, new Vector3(rangeMultiplier, rangeMultiplier, 1f));
            _hitboxRangeMultiplier = rangeMultiplier;
        }

        int layerMask = _swordData.TargetLayers.value == 0
            ? Physics2D.AllLayers
            : _swordData.TargetLayers.value;

        var filter = new ContactFilter2D { useTriggers = Physics2D.queriesHitTriggers };
        filter.SetLayerMask(layerMask);
        // 今フレームの公転位置や左右反転を反映し、問い合わせ中だけ判定を有効にする。
        _attackHitbox.enabled = true;
        try
        {
            Physics2D.SyncTransforms();
            _attackHitbox.Overlap(filter, _targetsInRange);
        }
        finally
        {
            _attackHitbox.enabled = false;
        }
    }

    private IDamageable GetDamageable(Collider2D targetCollider)
    {
        return targetCollider != null && targetCollider.transform.root != transform.root
            ? targetCollider.GetComponentInParent<IDamageable>()
            : null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_attackHitbox == null)
        {
            return;
        }

        Matrix4x4 previousMatrix = Gizmos.matrix;
        Color previousColor = Gizmos.color;
        Gizmos.matrix = _attackHitbox.transform.localToWorldMatrix;
        Gizmos.color = Color.cyan;
        for (int pathIndex = 0; pathIndex < _attackHitbox.pathCount; pathIndex++)
        {
            Vector2[] path = _attackHitbox.GetPath(pathIndex);
            for (int i = 0; i < path.Length; i++)
            {
                Gizmos.DrawLine(path[i] + _attackHitbox.offset, path[(i + 1) % path.Length] + _attackHitbox.offset);
            }
        }
        Gizmos.matrix = previousMatrix;
        Gizmos.color = previousColor;
    }
#endif

    private void SetVisualVisible(bool isVisible)
    {
        if (_visualRenderers == null)
        {
            return;
        }

        foreach (SpriteRenderer visualRenderer in _visualRenderers)
        {
            visualRenderer.enabled = isVisible;
        }
    }
}
