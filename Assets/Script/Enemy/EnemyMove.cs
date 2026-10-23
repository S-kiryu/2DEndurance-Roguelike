using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed class EnemyMove : MonoBehaviour
{
    [SerializeField, Min(0f)]
    private float moveSpeed = 2f;

    private Rigidbody2D body;
    private Transform player;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Initialize(GameObject playerObject, float speed)
    {
        if (playerObject == null)
        {
            Debug.LogWarning($"{name}: Playerが設定されていません。");
            return;
        }

        player = playerObject.transform;
        moveSpeed = speed;
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            StopMoving();
            return;
        }

        float difference =
            player.position.x - transform.position.x;

        float direction = Mathf.Sign(difference);

        body.linearVelocity = new Vector2(
            direction * moveSpeed,
            body.linearVelocity.y
        );
    }

    private void StopMoving()
    {
        body.linearVelocity = new Vector2(
            0f,
            body.linearVelocity.y
        );
    }
}