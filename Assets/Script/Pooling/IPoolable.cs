/// <summary>
/// 再利用時に状態をリセットするための通知。
/// Spawnは有効化前、Despawnは無効化前に呼ばれる。Awake前の呼び出しにも対応すること。
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// スポーン時に呼ばれる処理
    /// </summary>
    void OnPoolSpawned();
    /// <summary>
    /// プールに戻る際に呼ばれる処理
    /// </summary>
    void OnPoolDespawned();
}
