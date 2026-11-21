using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>公転が有効な武器をWeaponMountの周りで水平に移動させる。</summary>
[DefaultExecutionOrder(-10)] // 武器の攻撃処理より前に位置を更新する。
public sealed class SpinningWeapon : MonoBehaviour
{
    [SerializeField] private WeaponManager _weaponManager;

    [SerializeField] private float _leftTurnX = -1f;

    [SerializeField] private float _rightTurnX = 1f;

    [SerializeField] private float _rotationSpeed = 100f;

    [SerializeField] private SpriteRenderer _playerRenderer;

    private readonly Dictionary<Weapon, (SortingGroup Group, int Layer, int Order, bool Enabled)> _originalSorting = new();
    private readonly List<(SortingGroup Group, float Depth, bool IsBehind)> _depthOrder = new();
    private float _orbitProgress;

    private void OnEnable()
    {
        if (_weaponManager == null)
            return;

        _weaponManager.WeaponAdded += OnWeaponAdded;
        _weaponManager.WeaponRemoved += OnWeaponRemoved;
    }

    private void Update() => UpdateOrbit(Time.deltaTime);

    private void UpdateOrbit(float deltaTime)
    {
        if (_weaponManager == null || Mathf.Approximately(_leftTurnX, _rightTurnX))
        {
            ResetOrbit();
            return;
        }

        IReadOnlyList<Weapon> weapons = _weaponManager.Weapons;
        _depthOrder.Clear();
        int weaponCount = 0;
        for (int i = 0; i < weapons.Count; i++)
        {
            Weapon weapon = weapons[i];
            if (CanOrbit(weapon))
                weaponCount++;
            else
                ReleaseFromOrbit(weapon);
        }

        if (weaponCount == 0)
            return;

        // 0～1で右から左へ、1～2で左から右へ移動する。
        _orbitProgress = (_orbitProgress + _rotationSpeed * deltaTime / 180f) % 2f;
        if (_orbitProgress < 0f)
            _orbitProgress += 2f;

        Vector3 center = transform.position;
        float slotSpacing = 2f / weaponCount;
        int slotIndex = 0;
        for (int i = 0; i < weapons.Count; i++)
        {
            Weapon weapon = weapons[i];
            if (!CanOrbit(weapon))
                continue;

            float progress = (_orbitProgress + slotSpacing * slotIndex) % 2f;
            slotIndex++;
            bool isBehind = progress < 1f;
            float rightToLeft = isBehind ? progress : 2f - progress;
            float x = Mathf.SmoothStep(_rightTurnX, _leftTurnX, rightToLeft);

            // ワールド座標で配置し、プレイヤーの左右反転で軌道が反転するのを防ぐ。
            weapon.transform.position = center + Vector3.right * x;
            float depth = Mathf.Min(rightToLeft, 1f - rightToLeft);
            _depthOrder.Add((GetSortingGroup(weapon), depth, isBehind));
        }

        // プレイヤーに近い順に、奥は描画順を下げ、手前は上げる。
        _depthOrder.Sort((left, right) => left.Depth.CompareTo(right.Depth));
        int playerLayer = _playerRenderer != null ? _playerRenderer.sortingLayerID : 0;
        int playerOrder = _playerRenderer != null ? _playerRenderer.sortingOrder : 0;
        int rearOrder = playerOrder;
        int frontOrder = playerOrder;
        foreach (var slot in _depthOrder)
        {
            slot.Group.sortingLayerID = playerLayer;
            slot.Group.sortingOrder = slot.IsBehind ? --rearOrder : ++frontOrder;
        }
    }

    private static bool CanOrbit(Weapon weapon)
    {
        return weapon != null && weapon.WeaponData != null && weapon.WeaponData.OrbitEnabled;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    /// <returns></returns>
    private SortingGroup GetSortingGroup(Weapon weapon)
    {
        if (_originalSorting.TryGetValue(weapon, out var saved) && saved.Group != null)
            return saved.Group;

        bool hadGroup = weapon.TryGetComponent(out SortingGroup group);
        if (!hadGroup)
            group = weapon.gameObject.AddComponent<SortingGroup>();

        _originalSorting[weapon] = (group, group.sortingLayerID, group.sortingOrder, hadGroup && group.enabled);
        group.enabled = true;
        return group;
    }

    private void OnWeaponAdded(Weapon _) => UpdateOrbit(0f);

    private void OnWeaponRemoved(Weapon weapon)
    {
        ReleaseFromOrbit(weapon);
        UpdateOrbit(0f);
    }

    private void ReleaseFromOrbit(Weapon weapon)
    {
        if (!ReferenceEquals(weapon, null) && _originalSorting.Remove(weapon, out var saved))
            RestoreWeapon(weapon, saved);
    }

    private void OnDisable()
    {
        if (_weaponManager != null)
        {
            _weaponManager.WeaponAdded -= OnWeaponAdded;
            _weaponManager.WeaponRemoved -= OnWeaponRemoved;
        }

        ResetOrbit();
    }

    private void ResetOrbit()
    {
        foreach (var entry in _originalSorting)
            RestoreWeapon(entry.Key, entry.Value);

        _originalSorting.Clear();
        _depthOrder.Clear();
    }

    private void RestoreWeapon(Weapon weapon, (SortingGroup Group, int Layer, int Order, bool Enabled) saved)
    {
        var (group, layer, order, enabled) = saved;
        if (group != null)
        {
            group.sortingLayerID = layer;
            group.sortingOrder = order;
            group.enabled = enabled;
        }

        if (weapon != null && weapon.transform.parent == transform)
            weapon.transform.localPosition = Vector3.zero;
    }
}
