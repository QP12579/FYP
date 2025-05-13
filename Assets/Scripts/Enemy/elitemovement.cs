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
    private int patrolCount = 0;
    private bool isDashing = false;
    private Vector3 dashTargetPos;
    private EliteEnemy eliteEnemy;

    protected override void Start()
    {
        base.Start();
        SetRandomTargetPosition();
        eliteEnemy = GetComponent<EliteEnemy>();
    }

    protected override void Update()
    {
        if (isStopped) return;

        if (isDashing)
        {
            DashToPlayer();
            return;
        }

        if (player != null && Vector3.Distance(transform.position, player.transform.position) < chaseRange)
        {
            PatrolWithDash();
        }
        else
        {
            PatrolWithDash();
        }
    }

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
            if (player != null && dashAttackType != DashAttackType.None)
            {
                Vector3 dir = (player.transform.position - transform.position).normalized;
                dashTargetPos = transform.position + dir * dashDistance;
                Debug.Log($"Dash to player: {dir}, dashTargetPos: {dashTargetPos}");
                isDashing = true;
            }
            patrolCount = 0;
        }
    }

    private void DashToPlayer()
    {
        MoveTowards(dashTargetPos, dashSpeed);

        if (Vector3.Distance(transform.position, dashTargetPos) < 0.1f)
        {
            isDashing = false;
            isMoving = false;

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

    public void DashToPlayerWithAttack(DashAttackType attackType)
    {
        if (player != null)
        {
            Vector3 dir = (player.transform.position - transform.position).normalized;
            dashTargetPos = transform.position + dir * dashDistance;
            isDashing = true;
            dashAttackType = attackType;
            patrolCount = 0;
        }
    }

    protected virtual void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0;
        transform.position += direction * speed * Time.deltaTime;
    }

    private void SetRandomTargetPosition()
    {
        float angle = Random.Range(0f, 360f);
        Vector3 randomDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        targetPosition = transform.position + randomDirection * moveDistance;
        isMoving = true;
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
}
