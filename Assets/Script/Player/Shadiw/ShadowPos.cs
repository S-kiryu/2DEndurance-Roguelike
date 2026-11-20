using UnityEngine;

public class ShadowPos : MonoBehaviour
{
    [SerializeField] private Transform _playerTransform;


    private void Update()
    {
        if (_playerTransform != null)
        {
            Vector3 playerPosition = _playerTransform.position;
            transform.position = new Vector3(playerPosition.x, transform.position.y, transform.position.z);
        }
    }
}
