using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 武器のベースクラス
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    private float Interval = 0f;

    /// <summary>
    /// クールタイムが経過したかどうかを確認し、攻撃可能であれば攻撃を行う
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Tick(float deltaTime)
    {
        Interval -= deltaTime;

        if (Interval <= 0f && CanAttack())
        {
            Attack();
            Interval = GetAttackInterval();
        }
    }

    protected abstract float GetAttackInterval();
    protected abstract bool CanAttack();
    protected abstract void Attack();
}
