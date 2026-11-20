using UnityEngine;

/// <summary>
/// 倒されたとき、口を開くアニメーションに合わせてちびかぼちゃを吐き出す。
/// </summary>
[RequireComponent(typeof(Animator), typeof(EnemyMove), typeof(BoxCollider2D))]
public sealed class kabotya : EnemyBase
{
    [SerializeField] private ChibiKabotya _chibiPrefab;
    [SerializeField, Min(0)] private int _chibiCount = 3;
    [SerializeField] private Transform _mouthSpawnPoint;

    private static readonly int Move = Animator.StringToHash("Base Layer.Move");
    private static readonly int Dead = Animator.StringToHash("Base Layer.Dead");

    private Animator _animator;
    private EnemyMove _movement;
    private BoxCollider2D _bodyCollider;
    private SpriteRenderer _spriteRenderer;
    private WaveEnemyHandle _waveHandle;
    private EnemyStatScale _statScale = EnemyStatScale.Default;
    private bool _dying;
    private bool _hasSpatOut;

    public override void Initialize(GameObject player, EnemyStatScale statScale)
    {
        base.Initialize(player, statScale);
        _statScale = statScale;
        _movement.SetMovementPaused(false);
        _bodyCollider.enabled = true;
        if (_animator.isInitialized)
        {
            _animator.Play(Move, 0, 0f);
        }
    }

    public override void OnPoolSpawned()
    {
        base.OnPoolSpawned();
        _statScale = EnemyStatScale.Default;
        _dying = false;
        _hasSpatOut = false;
    }

    /// <summary>Dead の口が開いたコマから呼ぶ。</summary>
    public void SpitOutChibis()
    {
        if (!_dying || _hasSpatOut)
        {
            return;
        }

        _hasSpatOut = true;
        if (_chibiPrefab == null || _chibiCount <= 0)
        {
            return;
        }

        GameObject player = PlayerTarget != null ? PlayerTarget.gameObject : null;
        if (player == null && FindAnyObjectByType<PlayerPresenter>() is PlayerPresenter presenter)
        {
            player = presenter.gameObject;
        }

        GameObjectPool pool = GameObjectPool.FindPoolFor(gameObject);
        Vector3 mouth = _mouthSpawnPoint != null ? _mouthSpawnPoint.position : transform.position;
        float direction = -Mathf.Sign(transform.lossyScale.x);
        Quaternion rotation = _chibiPrefab.transform.rotation;
        for (int i = 0; i < _chibiCount; i++)
        {
            float spread = _chibiCount == 1 ? 0.5f : (float)i / (_chibiCount - 1);
            float distance = Mathf.Lerp(0.65f, 1.75f, spread);
            Vector3 landingPosition = new(mouth.x + direction * distance, _chibiPrefab.transform.position.y, mouth.z);
            ChibiKabotya chibi = pool != null
                ? pool.Spawn(_chibiPrefab, mouth, rotation)
                : Instantiate(_chibiPrefab, mouth, rotation);
            if (chibi == null)
            {
                continue;
            }

            if (player != null)
            {
                chibi.Initialize(player, _statScale);
            }
            chibi.Launch(landingPosition);
            if (_waveHandle != null && chibi.TryGetComponent(out WaveEnemyHandle childHandle))
            {
                _waveHandle.NotifyChildSpawned(childHandle);
            }
        }
    }

    /// <summary>Dead の最後から呼び、親だけを死亡処理に進める。</summary>
    public void FinishDeath()
    {
        if (!_dying)
        {
            return;
        }

        SpitOutChibis();
        _dying = false;
        base.Die();
    }

    protected override void Awake()
    {
        _animator = GetComponent<Animator>();
        _movement = GetComponent<EnemyMove>();
        _bodyCollider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _waveHandle = GetComponent<WaveEnemyHandle>();
        base.Awake();
    }

    protected override void Die()
    {
        _dying = true;
        _movement.SetMovementPaused(true);
        _bodyCollider.enabled = false;

        // 死亡アニメーションは左向きのパーツで構成される。右向きなら口ごと反転する。
        if (_spriteRenderer != null && _spriteRenderer.flipX)
        {
            Vector3 scale = transform.localScale;
            scale.x = -scale.x;
            transform.localScale = scale;
        }

        if (_animator.isActiveAndEnabled && _animator.HasState(0, Dead))
        {
            _animator.Play(Dead, 0, 0f);
        }
        else
        {
            FinishDeath();
        }
    }
}
