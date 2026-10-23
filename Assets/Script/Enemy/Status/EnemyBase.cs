using UnityEngine;

/// <summary>
/// 敵のベースクラス
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 2f;

    protected StatusBase Status { get; private set; }

    protected virtual void Awake()
    {
        Status = new StatusBase(maxHP, damage, speed);
    }

    public virtual void TakeDamage(int amount)
    {
        Status.TakeDamage(amount);

        if (Status.IsDead)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}