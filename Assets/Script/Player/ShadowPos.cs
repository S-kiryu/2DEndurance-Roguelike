using UnityEngine;

public class ShadowPos : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;


    private void Update()
    {
        if (playerTransform != null)
        {
            Vector3 playerPosition = playerTransform.position;
            transform.position = new Vector3(playerPosition.x, transform.position.y, transform.position.z);
        }
    }
}
