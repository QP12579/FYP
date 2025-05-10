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
    public float restDuration = 2f; // 休息時間

    [Header("Hand Prefab")]
    public Mechhand mechhandPrefab; // 指定手臂預製件

    private elitemovement movement;
    public Player player;
    private bool lastAttackWasFast = false;
    public bool LastAttackWasFast => lastAttackWasFast;

    private void Start()
    {
        movement = GetComponent<elitemovement>();

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
        lastAttackWasFast = true;

        // 建議：在這裡觸發動畫
        // GetComponent<Animator>().SetTrigger("FastAttack");
    }

    public void SlowMeleeAttack()
    {
        Debug.Log("Performing slow melee attack!");
        if (player != null && IsPlayerInRange(attackRange))
        {
            player.TakeDamage(30); // Slow melee attack deals高傷害
        }
        lastAttackWasFast = false;

        // 建議：在這裡觸發動畫
        // GetComponent<Animator>().SetTrigger("SlowAttack");
    }

    public void ShootHandAttack()
    {
        Debug.Log("Performing shoot hand attack!");
        StopMovement();

        if (mechhandPrefab != null && player != null)
        {
            // 生成手臂於本體位置與旋轉
            Mechhand hand = Instantiate(mechhandPrefab, transform.position, transform.rotation);
            // 射出手臂，回收後恢復移動
            hand.ShootAt(player.transform.position, ResumeMovement);
        }
        else
        {
            // 若未設置 prefab 或找不到玩家，直接恢復移動避免卡死
            ResumeMovement();
        }

        // 建議：在這裡觸發動畫
        // GetComponent<Animator>().SetTrigger("ShootHand");
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
        enemy.ResumeMovement();
    }

    public void UpdateState()
    {
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
                enemy.ShootHandAttack();
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
