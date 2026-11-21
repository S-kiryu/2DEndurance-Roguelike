using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器の生成、追加、削除、更新を管理する
/// </summary>
public class WeaponManager : MonoBehaviour
{
    public event Action<Weapon> WeaponAdded;
    public event Action<Weapon> WeaponRemoved;

    public IReadOnlyList<Weapon> Weapons => _weapons;
    public int MaxWeaponCount => Mathf.Max(1, _maxWeaponCount);
    public bool HasCapacity
    {
        get
        {
            int equippedCount = 0;
            foreach (Weapon weapon in _weapons)
            {
                if (weapon != null)
                    equippedCount++;
            }
            return equippedCount < MaxWeaponCount;
        }
    }

    public bool CanEquipWeapon(WeaponData weaponData) =>
        GetEquipError(weaponData != null ? weaponData.WeaponPrefab : null, weaponData) == null;

    /// <summary>
    /// 武器データに登録されたPrefabを生成して装備する
    /// </summary>
    public Weapon EquipWeapon(WeaponData weaponData)
    {
        Weapon prefab = weaponData != null ? weaponData.WeaponPrefab : null;
        string error = GetEquipError(prefab, weaponData);
        if (error != null)
        {
            Debug.LogWarning(error, this);
            return null;
        }

        Weapon weapon = Instantiate(prefab, _weaponMount);

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
        if (weapon == null || _weapons.Contains(weapon))
            return false;

        WeaponData targetData = weaponData != null ? weaponData : weapon.WeaponData;
        string error = GetEquipError(weapon, targetData);
        if (error != null)
        {
            Debug.LogWarning(error, this);
            return false;
        }

        if (!InitializeWeapon(weapon, targetData))
            return false;

        _weapons.Add(weapon);
        if (targetData is SwordData { UsePlayerAnimation: true } && _playerView != null)
            _playerView.EquipNata();

        WeaponAdded?.Invoke(weapon);
        return true;
    }

    /// <summary>同じWeaponDataを使う武器は、1つだけ所持できる。</summary>
    public bool HasWeapon(WeaponData weaponData) => GetWeapon(weaponData) != null;

    public bool OwnsWeapon(Weapon weapon) => weapon != null && _weapons.Contains(weapon);

    public Weapon GetWeapon(WeaponData weaponData)
    {
        if (weaponData == null)
        {
            return null;
        }

        foreach (Weapon weapon in _weapons)
        {
            if (weapon != null && weapon.WeaponData == weaponData)
            {
                return weapon;
            }
        }

        return null;
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

        if (!_weapons.Remove(weapon))
            return false;

        weapon.Unequip();

        if (destroyInstance)
        {
            Destroy(weapon.gameObject);
        }

        WeaponRemoved?.Invoke(weapon);
        return true;
    }

    [Header("シーン上の共通プール")]
    [SerializeField] private GameObjectPool _objectPool;

    [Header("武器の所有者")]
    [SerializeField] private PlayerPresenter _player;

    [Header("装備位置")]
    [SerializeField]
    private Transform _weaponMount;

    [Header("装備中の武器")]
    [SerializeField]
    private List<Weapon> _weapons = new();

    [Header("開始時に装備する武器")]
    [SerializeField]
    private List<WeaponData> _startingWeapons = new();

    [Header("所持上限")]
    [SerializeField, Min(1)]
    private int _maxWeaponCount = 6;

    private readonly List<Weapon> _weaponsToUpdate = new();
    private PlayerView _playerView;

    private void Awake()
    {
        if (_weaponMount == null)
        {
            _weaponMount = transform;
        }

        if (_player == null)
        {
            _player = _weaponMount.GetComponentInParent<PlayerPresenter>();
        }
        _playerView = _player != null ? _player.GetComponent<PlayerView>() : null;

        var initialWeapons = new List<Weapon>(_weapons);
        _weapons.Clear();
        foreach (Weapon weapon in initialWeapons)
        {
            if (weapon == null || _weapons.Contains(weapon))
            {
                continue;
            }
            if (!AddWeapon(weapon))
            {
                weapon.Unequip();
            }
        }

        foreach (WeaponData weaponData in _startingWeapons)
        {
            if (weaponData != null && !HasWeapon(weaponData))
            {
                EquipWeapon(weaponData);
            }
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        // 攻撃・削除の通知先で装備が変わっても、更新中のリストを壊さない。
        _weaponsToUpdate.AddRange(_weapons);
        for (int i = _weaponsToUpdate.Count - 1; i >= 0; i--)
        {
            Weapon weapon = _weaponsToUpdate[i];

            if (weapon == null)
            {
                if (_weapons.Remove(weapon))
                    WeaponRemoved?.Invoke(weapon);
            }
            else if (OwnsWeapon(weapon))
            {
                weapon.Tick(deltaTime);
            }
        }
        _weaponsToUpdate.Clear();
    }

    /// <summary>
    /// 武器の生成
    /// </summary>
    /// <param name="weapon"></param>
    /// <param name="weaponData"></param>
    /// <returns></returns>
    private bool InitializeWeapon(Weapon weapon, WeaponData weaponData)
    {
        weapon.Equip(_weaponMount);

        if (weapon.Initialize(weaponData, _player != null ? _player.Model : null, _objectPool))
            return true;

        weapon.Unequip();
        return false;
    }

    private string GetEquipError(Weapon weapon, WeaponData weaponData)
    {
        if (weapon == null || weaponData == null)
            return "WeaponDataまたはWeaponPrefabが設定されていません。";
        if (HasWeapon(weaponData))
            return $"{weaponData.name}は既に装備しています。";
        if (!HasCapacity)
            return $"武器所持数が上限（{MaxWeaponCount}）です。";

        return weapon.GetInitializationError(weaponData, _objectPool);
    }
}
