using UnityEngine;

/// <summary>設定アセットには実行中の状態を保持しない。状態は武器インスタンスに適用する。</summary>
public abstract class WeaponUpgradeEffect : ScriptableObject
{
    public abstract bool CanApplyTo(WeaponData data);
    public abstract void Apply(Weapon weapon);
}
