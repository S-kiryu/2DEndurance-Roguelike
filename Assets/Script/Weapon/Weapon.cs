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

    public void Equip(Transform transform)
    {
        this.transform.SetParent(transform);
        gameObject.SetActive(true);    
    }

    public void Unequip()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// インターバルを取得するメソット
    /// </summary>
    /// <returns></returns>
    protected abstract float GetAttackInterval();
    /// <summary>
    /// 攻撃できるかどうかを判定するメソット
    /// </summary>
    /// <returns></returns>
    protected abstract bool CanAttack();
    /// <summary>
    /// 攻撃処理を行うメソット
    /// </summary>
    protected abstract void Attack();
}
