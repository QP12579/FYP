using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(elitemovement))]
public class EliteEnemy : MonoBehaviour
{
    private IEnemyState currentState;

    [Header("Elite Enemy Settings")]
    public float patrolSpeed = 3f; // 巡邏速度
    public float attackRange = 2f; // 攻擊範圍
    public float laserRange = 15f; // 激光範圍
    public float restDuration = 2f; // 休息時間

    private elitemovement movement;
    public Player player;
    private bool lastAttackWasFast = false;
    public bool LastAttackWasFast => lastAttackWasFast;

    private Renderer enemyRenderer; // 用於控制顏色的 Renderer
    private Color originalColor; // 儲存原始顏色

    private void Start()
    {
        movement = GetComponent<elitemovement>();
        enemyRenderer = GetComponent<Renderer>();

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color; // 儲存原始顏色
        }

        if (movement == null)
        {
            Debug.LogError("elitemovement component is missing on EliteEnemy!");
            enabled = false;
            return;
        }

        player = FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogError("Player object not found in the scene!");
            enabled = false;
            return;
        }

        ChangeState(new PatrolState(this));
    }

    private void Update()
    {
        currentState?.UpdateState();
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }

    public void StopMovement()
    {
        movement.Stop();
    }

    public void ResumeMovement()
    {
        movement.Move();
    }

    public void FastMeleeAttack()
    {
        Debug.Log("Performing fast melee attack!");
        if (player != null && IsPlayerInRange(attackRange))
        {
            player.TakeDamage(10); // Fast melee attack deals低傷害
        }

        ChangeColor(Color.red); // 改變顏色為紅色
        Invoke(nameof(ResetColor), 0.5f); // 0.5秒後恢復顏色

        lastAttackWasFast = true;
    }

    public void SlowMeleeAttack()
    {
        Debug.Log("Performing slow melee attack!");
        if (player != null && IsPlayerInRange(attackRange))
        {
            player.TakeDamage(30); // Slow melee attack deals高傷害
        }

        ChangeColor(Color.green); // 改變顏色為綠色
        Invoke(nameof(ResetColor), 1.5f); // 1.5秒後恢復顏色

        lastAttackWasFast = false;
    }

    public void LaserAttack()
    {
        Debug.Log("Preparing laser attack!");
        StopMovement(); // 停止移動

        // 開始激光準備過程
        StartCoroutine(LaserReadyCoroutine());
    }

    private IEnumerator LaserReadyCoroutine()
    {
        float readyTime = 1.5f; // 激光準備時間
        ChangeColor(Color.yellow); // 在準備期間將顏色改為黃色
        yield return new WaitForSeconds(readyTime); // 等待準備時間

        Debug.Log("Performing laser attack!");
        if (player != null && IsPlayerInRange(laserRange))
        {
            player.TakeDamage(20); // 激光攻擊造成中等傷害
        }

        ChangeColor(Color.blue); // 激光攻擊時改為藍色
        Invoke(nameof(ResetColor), 0.5f); // 0.5秒後恢復顏色

        ResumeMovement(); // 恢復移動
    }

    private void ChangeColor(Color color)
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = color;
        }
    }

    private void ResetColor()
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = originalColor; // 恢復原始顏色
        }
    }

    public bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= range;
    }
}

public interface IEnemyState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public class PatrolState : IEnemyState
{
    private EliteEnemy enemy;

    public PatrolState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        Debug.Log("Entering Patrol State");
        enemy.ResumeMovement(); // 開始隨機移動
    }

    public void UpdateState()
    {
        // 如果玩家進入攻擊範圍，切換到攻擊狀態
        if (enemy.IsPlayerInRange(enemy.attackRange))
        {
            enemy.ChangeState(new AttackState(enemy));
        }
    }

    public void ExitState()
    {
        Debug.Log("Exiting Patrol State");
        enemy.StopMovement();
    }
}

public class ChaseState : IEnemyState
{
    private EliteEnemy enemy;

    public ChaseState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        Debug.Log("Chasing the player!");
    }

    public void UpdateState()
    {
        if (enemy.IsPlayerInRange(enemy.attackRange))
        {
            enemy.ChangeState(new AttackState(enemy));
            return;
        }

        Vector3 directionToPlayer = (enemy.player.transform.position - enemy.transform.position).normalized;
        enemy.transform.position += directionToPlayer * Time.deltaTime;
    }

    public void ExitState()
    {
        enemy.StopMovement();
    }
}

public class AttackState : IEnemyState
{
    private EliteEnemy enemy;
    private float attackCooldown = 2f;
    private float attackTimer;

    public AttackState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        attackTimer = 0f;
        Debug.Log("Entering Attack State");
    }

    public void UpdateState()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            PerformAttack();
            attackTimer = 0f;
        }

        // 如果玩家離開攻擊範圍，切換回巡邏狀態
        if (!enemy.IsPlayerInRange(enemy.attackRange))
        {
            enemy.ChangeState(new PatrolState(enemy));
        }
    }

    private void PerformAttack()
    {
        if (enemy.LastAttackWasFast)
        {
            enemy.SlowMeleeAttack();
        }
        else
        {
            int attackType = Random.Range(0, 2);
            if (attackType == 0)
            {
                enemy.FastMeleeAttack();
            }
            else
            {
                enemy.LaserAttack();
            }
        }
    }

    public void ExitState()
    {
        Debug.Log("Exiting Attack State");
    }
}

public class RestState : IEnemyState
{
    private EliteEnemy enemy;
    private float restTimer;

    public RestState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        restTimer = 0f;
        Debug.Log("Resting...");
    }

    public void UpdateState()
    {
        restTimer += Time.deltaTime;

        if (restTimer >= enemy.restDuration)
        {
            enemy.ChangeState(new PatrolState(enemy));
        }
    }

    public void ExitState()
    {
        Debug.Log("Finished resting.");
    }
}
