using UnityEngine;

/// <summary>
/// PlayerのModelとViewを仲介する
/// </summary>
public sealed class PlayerPresenter : MonoBehaviour
{
    [SerializeField]
    private PlayerView _view;

    [SerializeField]
    private PlayerMovement _movement;
    [SerializeField]
    private PlayerModel _model = new PlayerModel();

    private void Awake()
    {
        _model.Initialize();
        _movement.Initialize(_model);
    }

    public void ReceiveDamage(int damage)
    {
        _model.TakeDamage(damage);
    }

    public void Heal(int amount)
    {
        _model.Heal(amount);
    }
}