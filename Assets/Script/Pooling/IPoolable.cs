/// <summary>
/// 再利用時に状態をリセットするための通知。
/// Spawnは有効化前、Despawnは無効化前に呼ばれる。Awake前の呼び出しにも対応すること。
/// </summary>
public interface IPoolable
{
    void OnPoolSpawned();
    void OnPoolDespawned();
}
