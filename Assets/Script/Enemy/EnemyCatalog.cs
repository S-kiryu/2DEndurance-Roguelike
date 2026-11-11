using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の識別IDとPrefabの対応データ
/// </summary>
[Serializable]
public sealed class EnemyCatalogEntry
{
    [SerializeField] private string id;
    [SerializeField] private GameObject prefab;

    public string Id => id;
    public GameObject Prefab => prefab;
}

/// <summary>
/// 敵のPrefabをまとめて登録し、各Waveから識別IDで参照する共通カタログ。
/// </summary>
[CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Enemy/Catalog")]
public sealed class EnemyCatalog : ScriptableObject
{
    [SerializeField] private List<EnemyCatalogEntry> enemies = new();

    public IReadOnlyList<EnemyCatalogEntry> Enemies => enemies;

    /// <summary>
    ///  指定した識別IDに対応する敵のPrefabを取得する。
    /// </summary>
    /// <param name="id">敵の固有のID</param>
    /// <param name="prefab">敵のプレハブ</param>
    /// <returns>プレハブを獲得できたかどうか</returns>
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
