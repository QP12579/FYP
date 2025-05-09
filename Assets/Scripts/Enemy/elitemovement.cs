using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elitemovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; // 隨機移動的速度
    public float moveDistance = 5f; // 隨機移動的距離
    public float teleportDistance = 10f; // 瞬間傳送的距離

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isStopped = false; // 用於控制是否停止移動

    [Header("Player Reference")]
    public Transform player; // 玩家對象的引用

    // Start is called before the first frame update
    void Start()
    {
        SetRandomTargetPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isStopped) // 如果未停止，則執行隨機移動
        {
            RandomMove();
        }
    }

    // 隨機移動
    private void RandomMove()
    {
        if (!isMoving)
        {
            SetRandomTargetPosition();
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    // 設置隨機目標位置
    private void SetRandomTargetPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere; // 隨機方向
        randomDirection.y = 0; // 保持在平面上
        targetPosition = transform.position + randomDirection.normalized * moveDistance;
        isMoving = true;
    }

    // 瞬間傳送到玩家附近
    public void TeleportNearPlayer()
    {
        if (player != null)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 2f; // 在玩家附近隨機偏移
            randomOffset.y = 0; // 保持在平面上
            transform.position = player.position + randomOffset;
            Debug.Log("Teleported near the player!");
        }
    }

    // 瞬間傳送遠離玩家
    public void TeleportAwayFromPlayer()
    {
        if (player != null)
        {
            Vector3 directionAwayFromPlayer = (transform.position - player.position).normalized;
            transform.position = transform.position + directionAwayFromPlayer * teleportDistance;
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

    // 碰撞檢測：當碰撞到牆壁時改變方向
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            SetRandomTargetPosition(); // 碰撞到牆壁時重新設置目標位置
        }
    }
}
