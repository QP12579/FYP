using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elitemovement : EnemyMovement
{
    [Header("Elite Movement Settings")]
    public float moveDistance = 5f;
    public float dashDistance = 8f;
    public float chaseRange = 10f;

    [Header("Dash Settings")]
    public float dashSpeed = 10f;
    public float chaseSpeed = 5f;

    public enum DashAttackType { None, Fast, Slow }
    [HideInInspector] public DashAttackType dashAttackType = DashAttackType.None;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isStopped = false;
    private bool isDashing = false;
    private Vector3 dashTargetPos;
    private EliteEnemy eliteEnemy;

    // 巡邏次數與目標次數
    private int patrolsDone = 0;
    private int patrolsBeforeDash = 2;

    protected override void Start()
    {
        base.Start();
        eliteEnemy = GetComponent<EliteEnemy>();
        ResetPatrolsBeforeDash();
        SetRandomTargetPosition();
        // 確保有 Rigidbody
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    protected override void Update()
    {
        if (isStopped) return;

        if (isDashing)
        {
            DashToPlayer();
            return;
        }

        PatrolLogic();
    }

    private void PatrolLogic()
    {
        if (!isMoving)
        {
            SetRandomTargetPosition();
        }

        MoveTowards(targetPosition, moveSpeed);

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.1f)
        {
            isMoving = false;
            patrolsDone++;

            if (patrolsDone >= patrolsBeforeDash)
            {
                StartDashToPlayer();
            }
        }
    }

    private void StartDashToPlayer()
    {
        if (player != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            dir.y = 0;
            dashTargetPos = transform.position + dir * dashDistance;
            isDashing = true;
            patrolsDone = 0;
            ResetPatrolsBeforeDash();
        }
    }

    private void DashToPlayer()
    {
        MoveTowards(dashTargetPos, dashSpeed);

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(dashTargetPos.x, 0, dashTargetPos.z)) < 0.1f)
        {
            isDashing = false;
            isMoving = false;

            // Dash 結束後隨機攻擊
            if (eliteEnemy != null)
            {
                int attackType = Random.Range(0, 3);
                if (attackType == 0)
                    eliteEnemy.FastMeleeAttack();
                else if (attackType == 1)
                    eliteEnemy.SlowMeleeAttack();
                else
                    eliteEnemy.EarthquakeAttack();
            }
        }
    }

    private void ResetPatrolsBeforeDash()
    {
        patrolsBeforeDash = Random.Range(2, 4); // 2~3 次
    }

    public void SetRandomTargetPosition()
    {
        float angle = Random.Range(0f, 360f);
        Vector3 randomDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        targetPosition = transform.position + randomDirection * moveDistance;
        isMoving = true;
    }

    public bool IsAtPatrolPoint()
    {
        return !isMoving && Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z)) < 0.1f;
    }

    public void Stop()
    {
        isStopped = true;
        Debug.Log("Movement stopped.");
    }

    public void Move()
    {
        isStopped = false;
        Debug.Log("Movement resumed.");
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            SetRandomTargetPosition();
        }
    }

    // 只移動XZ平面，Y軸交給物理引擎
    protected void MoveTowards(Vector3 destination, float speed)
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        Vector3 currentPos = transform.position;
        Vector3 targetXZ = new Vector3(destination.x, currentPos.y, destination.z);
        Vector3 direction = (targetXZ - currentPos);
        direction.y = 0;
        if (direction.sqrMagnitude > 0.001f)
        {
            direction = direction.normalized;
            Vector3 move = direction * speed * Time.deltaTime;
            Vector3 newPosition = currentPos + move;
            newPosition.y = currentPos.y; // 保持Y軸不變
            rb.MovePosition(newPosition);
        }
    }
}
