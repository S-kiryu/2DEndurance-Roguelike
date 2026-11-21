using UnityEngine;

/// <summary>新しい効果はこれを継承し、武器PrefabへAdd Componentで取り付ける。</summary>
public abstract class WeaponEffect : MonoBehaviour
{
    public int ActivationStar => _activationStar;
    public bool IsEffectActive => Context != null && Context.IsActive && isActiveAndEnabled &&
        Context.StarLevel >= _activationStar && Supports(Context.Data);

    public virtual bool Supports(WeaponData data) => data != null;
    public virtual float GetStatBonus(WeaponUpgradeStat stat) => 0f;
    public virtual void OnTick(float deltaTime) { }
    public virtual void OnAttackStarted() { }
    public virtual void OnAttackFinished() { }
    public virtual void OnHit(WeaponHit hit) { }
    public virtual void OnKill(WeaponHit hit) { }

    protected WeaponContext Context { get; private set; }

    protected virtual void OnEnable() => Register();
    protected virtual void OnDisable() => Unregister();
    protected virtual void OnTransformParentChanged()
    {
        Unregister();
        if (isActiveAndEnabled) Register();
    }

    internal void Bind(WeaponContext context)
    {
        if (ReferenceEquals(Context, context)) return;
        if (Context != null)
        {
            if (_wasActive) OnDeactivated();
            _wasActive = false;
            OnDetached();
        }
        Context = context;
        if (Context != null) OnAttached();
        RefreshActivation();
    }

    internal void RefreshActivation()
    {
        bool active = IsEffectActive;
        if (active == _wasActive) return;
        _wasActive = active;
        if (active) OnActivated(); else OnDeactivated();
    }

    // 型ごとのGetComponent/Findは各効果へ散らさず、Context経由で必要な参照を使う。
    protected virtual void OnAttached() { }
    protected virtual void OnDetached() { }
    protected virtual void OnActivated() { }
    protected virtual void OnDeactivated() { }

    [SerializeField, Range(1, 5), InspectorName("解放する星")]
    private int _activationStar = 1;

    private Weapon _registeredWeapon;
    private bool _wasActive;

    private void Register()
    {
        _registeredWeapon = GetComponentInParent<Weapon>();
        if (_registeredWeapon != null) _registeredWeapon.RegisterEffect(this);
    }

    private void Unregister()
    {
        if (_registeredWeapon != null) _registeredWeapon.UnregisterEffect(this);
        _registeredWeapon = null;
        Bind(null);
    }
}
