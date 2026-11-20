using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の識別IDとPrefabの対応データ
/// </summary>
[Serializable]
public sealed class EnemyCatalogEntry
{
    public string Id => _id;
    public GameObject Prefab => _prefab;

    [SerializeField] private string _id;
    [SerializeField] private GameObject _prefab;
}

/// <summary>
/// 敵のPrefabをまとめて登録し、各Waveから識別IDで参照する共通カタログ。
/// </summary>
[CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Enemy/Catalog")]
public sealed class EnemyCatalog : ScriptableObject
{
    public IReadOnlyList<EnemyCatalogEntry> Enemies => _enemies;

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

        foreach (EnemyCatalogEntry enemy in _enemies)
        {
            if (enemy != null && enemy.Id == id)
            {
                prefab = enemy.Prefab;
                return prefab != null;
            }
        }

        return false;
    }

    [SerializeField] private List<EnemyCatalogEntry> _enemies = new();

    private void OnValidate()
    {
        // 入れ間違いなどがないかを検証する
        var ids = new HashSet<string>();
        foreach (EnemyCatalogEntry enemy in _enemies)
        {
            if (enemy == null || string.IsNullOrWhiteSpace(enemy.Id) ||
                !ids.Add(enemy.Id) || enemy.Prefab == null)
            {
                Debug.LogWarning("敵カタログに空のID・重複ID・未設定のPrefabがあります。", this);
            }
        }
    }
}
