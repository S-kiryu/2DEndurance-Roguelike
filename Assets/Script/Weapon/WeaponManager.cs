using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器の生成、追加、削除、更新を管理する
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("装備位置")]
    [SerializeField]
    private Transform weaponMount;

    [Header("装備中の武器")]
    [SerializeField]
    private List<Weapon> weapons = new();

    private void Awake()
    {
        if (weaponMount == null)
        {
            weaponMount = transform;
        }

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null)
            {
                InitializeWeapon(weapon);
            }
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        for (int i = weapons.Count - 1; i >= 0; i--)
        {
            Weapon weapon = weapons[i];

            if (weapon == null)
            {
                weapons.RemoveAt(i);
                continue;
            }

            weapon.Tick(deltaTime);
        }
    }

    /// <summary>
    /// 武器Prefabを生成して装備する
    /// </summary>
    public Weapon EquipWeapon(Weapon weaponPrefab)
    {
        if (weaponPrefab == null)
            return null;

        Weapon weapon = Instantiate(weaponPrefab, weaponMount);

        weapon.transform.SetLocalPositionAndRotation(
            Vector3.zero,
            Quaternion.identity
        );

        AddWeapon(weapon);
        return weapon;
    }

    /// <summary>
    /// 生成済みの武器を管理対象へ追加する
    /// </summary>
    public void AddWeapon(Weapon weapon)
    {
        if (weapon == null || weapons.Contains(weapon))
            return;

        weapons.Add(weapon);
        InitializeWeapon(weapon);
    }

    public void RemoveWeapon(Weapon weapon)
    {
        if (weapon == null)
            return;

        if (!weapons.Remove(weapon))
            return;

        weapon.Unequip();
        Destroy(weapon.gameObject);
    }

    private void InitializeWeapon(Weapon weapon)
    {
        weapon.transform.SetParent(weaponMount, false);
        weapon.Equip(transform);
    }
}
