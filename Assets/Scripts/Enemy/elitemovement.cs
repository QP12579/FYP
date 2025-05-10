using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elitemovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;           // 巡邏移動速度
    public float moveDistance = 5f;        // 單次巡邏距離
    public float chaseSpeed = 5f;          // 追蹤玩家時速度
    public float teleportDistance = 10f;   // 瞬間傳送距離
    public float chaseRange = 10f;         // 追蹤玩家的觸發距離

    [Header("Player Reference")]
    public Transform player;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isStopped = false;
    private bool isChasing = false;

    private void Start()
    {
        SetRandomTargetPosition();
    }

    private void Update()
    {
        if (isStopped) return;

        if (player != null && Vector3.Distance(transform.position, player.position) < chaseRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    // 巡邏行為
    private void Patrol()
    {
        if (!isMoving)
        {
            SetRandomTargetPosition();
        }

        MoveTowards(targetPosition, moveSpeed);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    // 追蹤玩家
    private void ChasePlayer()
    {
        if (player == null) return;
        MoveTowards(player.position, chaseSpeed);
    }

    // 移動到目標點
    private void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * speed * Time.deltaTime;
    }

    // 設置新的隨機巡邏目標
    private void SetRandomTargetPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        targetPosition = transform.position + randomDirection.normalized * moveDistance;
        isMoving = true;
    }

    // 瞬間傳送到玩家附近
    public void TeleportNearPlayer()
    {
        if (player != null)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 2f;
            randomOffset.y = 0;
            transform.position = player.position + randomOffset;
            Debug.Log("Teleported near the player!");
        }
    }

    // 瞬間傳送遠離玩家
    public void TeleportAwayFromPlayer()
    {
        if (player != null)
        {
            Vector3 directionAway = (transform.position - player.position).normalized;
            transform.position += directionAway * teleportDistance;
            Debug.Log("Teleported away from the player!");
        }
    }

    // 停止移動
    public void Stop()
    {
        isStopped = true;
        Debug.Log("Movement stopped.");
    }

    // 恢復移動
    public void Move()
    {
        isStopped = false;
        Debug.Log("Movement resumed.");
    }

    // 碰撞時自動換方向
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            SetRandomTargetPosition();
        }
    }
}
