using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EnemyCatalogEntry
{
    [Tooltip("Waveで選択する識別名。一度使用した名前は変更しないでください。")]
    [SerializeField] private string id;
    [SerializeField] private GameObject prefab;

    public string Id => id;
    public GameObject Prefab => prefab;
}

/// <summary>Prefabを一度だけ登録し、各Waveから識別名で参照する共通カタログ。</summary>
[CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Enemy/Catalog")]
public sealed class EnemyCatalog : ScriptableObject
{
    [SerializeField] private List<EnemyCatalogEntry> enemies = new();

    public IReadOnlyList<EnemyCatalogEntry> Enemies => enemies;

    public bool TryGetPrefab(string id, out GameObject prefab)
    {
        prefab = null;
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        foreach (EnemyCatalogEntry enemy in enemies)
        {
            if (enemy != null && enemy.Id == id)
            {
                prefab = enemy.Prefab;
                return prefab != null;
            }
        }

        return false;
    }

    private void OnValidate()
    {
        var ids = new HashSet<string>();
        foreach (EnemyCatalogEntry enemy in enemies)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.Id) ||
                !ids.Add(enemy.Id) || enemy.Prefab == null)
            {
                Debug.LogWarning("敵カタログに空のID・重複ID・未設定のPrefabがあります。", this);
            }
        }
    }
}
