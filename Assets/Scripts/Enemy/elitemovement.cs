using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elitemovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float moveDistance = 5f;
    public float chaseSpeed = 5f;
    public float dashSpeed = 10f;
    public float dashDistance = 8f;
    public float chaseRange = 10f;

    [Header("Player Reference")]
    public Transform player;

    public enum DashAttackType { None, Fast, Slow }
    [HideInInspector] public DashAttackType dashAttackType = DashAttackType.None;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isStopped = false;
    private bool isChasing = false;
    private int patrolCount = 0;
    private bool isDashing = false;
    private Vector3 dashStartPos;
    private Vector3 dashTargetPos;
    private EliteEnemy eliteEnemy;

    private void Start()
    {
        SetRandomTargetPosition();
        eliteEnemy = GetComponent<EliteEnemy>();
    }

    private void Update()
    {
        if (isStopped) return;

        if (isDashing)
        {
            DashToPlayer();
            return;
        }

        if (player != null && Vector3.Distance(transform.position, player.position) < chaseRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        PatrolWithDash();
    }

    // 巡邏行為：隨機移動2次，第3次衝刺玩家
    private void PatrolWithDash()
    {
        if (patrolCount < 2)
        {
            if (!isMoving)
            {
                SetRandomTargetPosition();
            }

            MoveTowards(targetPosition, moveSpeed);

            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                patrolCount++;
            }
        }
        else
        {
            // 開始 dash，攻擊型態由外部指定
            if (player != null && dashAttackType != DashAttackType.None)
            {
                dashStartPos = transform.position;
                Vector3 dir = (player.position - transform.position).normalized;
                dashTargetPos = transform.position + dir * dashDistance;
                isDashing = true;
            }
            patrolCount = 0;
        }
    }

    // 衝刺到玩家方向
    private void DashToPlayer()
    {
        MoveTowards(dashTargetPos, dashSpeed);

        if (Vector3.Distance(transform.position, dashTargetPos) < 0.1f)
        {
            isDashing = false;
            isMoving = false;

            // Dash 結束時觸發攻擊
            if (eliteEnemy != null)
            {
                if (dashAttackType == DashAttackType.Fast)
                    eliteEnemy.FastMeleeAttack();
                else if (dashAttackType == DashAttackType.Slow)
                    eliteEnemy.SlowMeleeAttack();
            }
            dashAttackType = DashAttackType.None;
        }
    }

    // 讓外部呼叫 dash 並指定攻擊型態
    public void DashToPlayerWithAttack(DashAttackType attackType)
    {
        if (player != null)
        {
            dashStartPos = transform.position;
            Vector3 dir = (player.position - transform.position).normalized;
            dashTargetPos = transform.position + dir * dashDistance;
            isDashing = true;
            dashAttackType = attackType;
            patrolCount = 0; // 重置巡邏計數
        }
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
        float angle = Random.Range(0f, 360f);
        Vector3 randomDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        targetPosition = transform.position + randomDirection * moveDistance;
        isMoving = true;
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
