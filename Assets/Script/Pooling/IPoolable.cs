/// <summary>
/// プールを使いたいオブジェクトに実装するインターフェース
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// スポーンする個体を有効化する前に呼ばれる処理
    /// </summary>
    void OnPoolSpawned();
    /// <summary>
    /// 返却する個体を非アクティブ化した後に呼ばれる処理
    /// </summary>
    void OnPoolDespawned();
}
