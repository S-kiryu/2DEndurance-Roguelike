using System;
using UnityEngine;

/// <summary>
/// Waveによって生成された敵の生存状態を追跡する。
/// </summary>
public sealed class WaveEnemyHandle : MonoBehaviour
{
    public event Action<WaveEnemyHandle> Removed;
    public event Action<WaveEnemyHandle> ChildSpawned;

    internal void NotifyChildSpawned(WaveEnemyHandle enemy)
    {
        ChildSpawned?.Invoke(enemy);
    }

    private bool _removed;

    private void OnEnable()
    {
        _removed = false;
    }

    private void OnDisable()
    {
        NotifyRemoved();
    }

    private void OnDestroy()
    {
        NotifyRemoved();
    }

    private void NotifyRemoved()
    {
        if (_removed)
        {
            return;
        }

        _removed = true;
        Removed?.Invoke(this);
        Removed = null;
        ChildSpawned = null;
    }
}
