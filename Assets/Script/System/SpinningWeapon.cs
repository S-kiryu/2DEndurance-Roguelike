using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>WeaponMountを中心に装備中の武器を水平に公転させる。</summary>
[DefaultExecutionOrder(-10)] // 武器の攻撃処理より前に位置を更新する。
public sealed class SpinningWeapon : MonoBehaviour
{
    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private float leftTurnX = -1f;

    [SerializeField] private float rightTurnX = 1f;

    [SerializeField] private float rotationSpeed = 100f;

    [SerializeField] private SpriteRenderer playerRenderer;

    private readonly Dictionary<Weapon, (SortingGroup Group, int Layer, int Order, bool Enabled)> originalSorting = new();
    private readonly List<(SortingGroup Group, float Depth, bool IsBehind)> depthOrder = new();
    private float orbitProgress;

    private void OnEnable()
    {
        if (weaponManager == null)
            return;

        weaponManager.WeaponAdded += OnWeaponAdded;
        weaponManager.WeaponRemoved += OnWeaponRemoved;
    }

    private void Update() => UpdateOrbit(Time.deltaTime);

    private void UpdateOrbit(float deltaTime)
    {
        if (weaponManager == null || Mathf.Approximately(leftTurnX, rightTurnX))
        {
            ResetOrbit();
            return;
        }

        IReadOnlyList<Weapon> weapons = weaponManager.Weapons;
        depthOrder.Clear();
        int weaponCount = 0;
        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i] != null)
                weaponCount++;
        }

        if (weaponCount == 0)
            return;

        // 0～1で右から左へ、1～2で左から右へ移動する。
        orbitProgress = (orbitProgress + rotationSpeed * deltaTime / 180f) % 2f;
        if (orbitProgress < 0f)
            orbitProgress += 2f;

        Vector3 center = transform.position;
        float slotSpacing = 2f / weaponCount;
        int slotIndex = 0;
        for (int i = 0; i < weapons.Count; i++)
        {
            Weapon weapon = weapons[i];
            if (weapon == null)
                continue;

            float progress = (orbitProgress + slotSpacing * slotIndex) % 2f;
            slotIndex++;
            bool isBehind = progress < 1f;
            float rightToLeft = isBehind ? progress : 2f - progress;
            float x = Mathf.SmoothStep(rightTurnX, leftTurnX, rightToLeft);

            // ワールド座標で配置し、プレイヤーの左右反転で軌道が反転するのを防ぐ。
            weapon.transform.position = center + Vector3.right * x;
            float depth = Mathf.Min(rightToLeft, 1f - rightToLeft);
            depthOrder.Add((GetSortingGroup(weapon), depth, isBehind));
        }

        // プレイヤーに近い順に、奥は描画順を下げ、手前は上げる。
        depthOrder.Sort((left, right) => left.Depth.CompareTo(right.Depth));
        int playerLayer = playerRenderer != null ? playerRenderer.sortingLayerID : 0;
        int playerOrder = playerRenderer != null ? playerRenderer.sortingOrder : 0;
        int rearOrder = playerOrder;
        int frontOrder = playerOrder;
        foreach (var slot in depthOrder)
        {
            slot.Group.sortingLayerID = playerLayer;
            slot.Group.sortingOrder = slot.IsBehind ? --rearOrder : ++frontOrder;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    /// <returns></returns>
    private SortingGroup GetSortingGroup(Weapon weapon)
    {
        if (originalSorting.TryGetValue(weapon, out var saved) && saved.Group != null)
            return saved.Group;

        bool hadGroup = weapon.TryGetComponent(out SortingGroup group);
        if (!hadGroup)
            group = weapon.gameObject.AddComponent<SortingGroup>();

        originalSorting[weapon] = (group, group.sortingLayerID, group.sortingOrder, hadGroup && group.enabled);
        group.enabled = true;
        return group;
    }

    private void OnWeaponAdded(Weapon _) => UpdateOrbit(0f);

    private void OnWeaponRemoved(Weapon weapon)
    {
        if (!ReferenceEquals(weapon, null) && originalSorting.Remove(weapon, out var saved))
            RestoreWeapon(weapon, saved);

        UpdateOrbit(0f);
    }

    private void OnDisable()
    {
        if (weaponManager != null)
        {
            weaponManager.WeaponAdded -= OnWeaponAdded;
            weaponManager.WeaponRemoved -= OnWeaponRemoved;
        }

        ResetOrbit();
    }

    private void ResetOrbit()
    {
        foreach (var entry in originalSorting)
            RestoreWeapon(entry.Key, entry.Value);

        originalSorting.Clear();
        depthOrder.Clear();
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
