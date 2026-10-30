using UnityEngine;

/// <summary>
/// 敵の移動
/// </summary>
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
            player = null;
            Debug.LogWarning($"{name}: Playerが設定されていません。");
            return;
        }

        player = playerObject.transform;
        moveSpeed = speed;
    }

    private void OnDisable()
    {
        player = null;
        if (body != null)
        {
            StopMoving();
            body.angularVelocity = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (player == null)
        {
            StopMoving();
            return;
        }
        Move(player);
    }

    /// <summary>
    /// 指定した位置まで移動する
    /// </summary>
    /// <param name="pos"></param>
    private void Move(Transform pos) 
    {
        float difference =
            pos.position.x - transform.position.x;

        float direction = Mathf.Sign(difference);

        body.linearVelocity = new Vector2(
            direction * moveSpeed,
            body.linearVelocity.y
        );
    }

    /// <summary>
    /// 移動の停止処理
    /// </summary>
    private void StopMoving()
    {
        body.linearVelocity = new Vector2(
            0f,
            body.linearVelocity.y
        );
    }
}
