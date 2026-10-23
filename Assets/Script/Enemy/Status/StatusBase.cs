using UnityEngine;

/// <summary>
/// ステータスのベースクラス
/// </summary>
public class StatusBase
{
    public int MaxHP { get; }
    public int HP { get; private set; }
    public int Damage { get; }
    public float Speed { get; }

    public StatusBase(int hp, int damage, float speed)
    {
        MaxHP = hp;
        HP = hp;
        Damage = damage;
        Speed = speed;
    }

    public void TakeDamage(int damage)
    {
        HP = Mathf.Max(HP - damage, 0);
    }

    public bool IsDead => HP <= 0;
}