
using UnityEngine;

/// <summary>
/// オブジェクトを上下に往復移動させるクラス
/// </summary>
public class UpDownMove : MonoBehaviour
{
    [SerializeField] private float _moveDistance = 2f;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private Transform _shadow;

    private float _startY;
    private Vector3 _shadowLocalPosition;
    private float _phaseOffset;

    private void Awake()
    {
        if (_shadow != null)
        {
            _shadowLocalPosition = _shadow.localPosition;
        }
        _phaseOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    private void OnEnable()
    {
        _startY = transform.position.y;
    }

    private void LateUpdate()
    {
        float offset = Mathf.Sin(Time.time * _moveSpeed + _phaseOffset) * _moveDistance;
        Vector3 position = transform.position;
        position.y = _startY + offset;
        transform.position = position;

        if (_shadow != null)
        {
            // 親の上下動だけを打ち消し、影は横移動に追従させる。
            _shadow.position = transform.TransformPoint(_shadowLocalPosition) - Vector3.up * offset;
        }
    }

    private void OnDisable()
    {
        if (_shadow != null)
        {
            _shadow.localPosition = _shadowLocalPosition;
        }
    }
}
