using UnityEngine;

public class MapLoop : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private GameObject _mapImage1;
    [SerializeField] private GameObject _mapImage2;
    [SerializeField] private float _scrollSpeed = 1f;
    [SerializeField] private float _wrapOffset = 1f;

    private float _imageWidth;

    private void Start()
    {
        _imageWidth = _mapImage1.GetComponent<SpriteRenderer>().bounds.size.x;
    }

    private void Update()
    {
        if (_mapImage1 == null || _mapImage2 == null)
        {
            return;
        }

        float offset = _scrollSpeed * Time.deltaTime;

        _mapImage1.transform.position += Vector3.left * offset;
        _mapImage2.transform.position += Vector3.left * offset;

        float cameraLeft = _camera.transform.position.x
            - _camera.orthographicSize * _camera.aspect;

        Vector3 wrapOffset = Vector3.right * (2f * _imageWidth) + Vector3.right * _wrapOffset;

        if (_mapImage1.GetComponent<SpriteRenderer>().bounds.max.x <= cameraLeft)
        {
            _mapImage1.transform.position += wrapOffset;
        }

        if (_mapImage2.GetComponent<SpriteRenderer>().bounds.max.x <= cameraLeft)
        {
            _mapImage2.transform.position += wrapOffset;
        }
    }
}
