
using UnityEngine;

/// <summary>
/// オブジェクトを上下に往復移動させるクラス
/// </summary>
public class UpDownMove : MonoBehaviour
{
    [SerializeField] private float _moveDistance = 2f;
    [SerializeField] private float _moveSpeed = 2f;

    private float _startY;

    private void Start()
    {
        _startY = transform.position.y;
    }

    private void LateUpdate()
    {
        Vector3 position = transform.position;

        position.y = _startY +
            Mathf.Sin(Time.time * _moveSpeed) * _moveDistance;

        transform.position = position;
    }
}
