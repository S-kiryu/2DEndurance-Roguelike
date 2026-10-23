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

    [Header("開始時に装備する武器")]
    [SerializeField]
    private List<WeaponData> startingWeapons = new();

    private void Awake()
    {
        if (weaponMount == null)
        {
            weaponMount = transform;
        }

        for (int i = weapons.Count - 1; i >= 0; i--)
        {
            Weapon weapon = weapons[i];

            if (weapon == null || !InitializeWeapon(weapon))
            {
                weapons.RemoveAt(i);
            }
        }

        foreach (WeaponData weaponData in startingWeapons)
        {
            if (weaponData != null)
            {
                EquipWeapon(weaponData);
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
    /// 武器データに登録されたPrefabを生成して装備する
    /// </summary>
    public Weapon EquipWeapon(WeaponData weaponData)
    {
        if (weaponData == null || weaponData.WeaponPrefab == null)
        {
            Debug.LogWarning("WeaponDataまたはWeaponPrefabが設定されていません。", this);
            return null;
        }

        Weapon weapon = Instantiate(weaponData.WeaponPrefab, weaponMount);

        if (!AddWeapon(weapon, weaponData))
        {
            Destroy(weapon.gameObject);
            return null;
        }

        return weapon;
    }

    /// <summary>
    /// 生成済みの武器を管理対象へ追加する
    /// </summary>
    public bool AddWeapon(Weapon weapon, WeaponData weaponData = null)
    {
        if (weapon == null || weapons.Contains(weapon))
            return false;

        if (!InitializeWeapon(weapon, weaponData))
            return false;

        weapons.Add(weapon);
        return true;
    }

    /// <summary>
    /// 武器を外す処理
    /// </summary>
    /// <param name="weapon">外す武器</param>
    /// <param name="destroyInstance">なくすかどうか</param>
    /// <returns></returns>
    public bool UnequipWeapon(Weapon weapon, bool destroyInstance = true)
    {
        if (weapon == null)
            return false;

        if (!weapons.Remove(weapon))
            return false;

        weapon.Unequip();

        if (destroyInstance)
        {
            Destroy(weapon.gameObject);
        }

        return true;
    }

    /// <summary>
    /// 武器の生成
    /// </summary>
    /// <param name="weapon"></param>
    /// <param name="weaponData"></param>
    /// <returns></returns>
    private bool InitializeWeapon(Weapon weapon, WeaponData weaponData = null)
    {
        weapon.Equip(weaponMount);

        if (weapon.Initialize(weaponData))
            return true;

        weapon.Unequip();
        return false;
    }
}
