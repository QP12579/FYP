using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(elitemovement))]
public class EliteEnemy : MonoBehaviour
{
    private IEnemyState currentState;

    [Header("Elite Enemy Settings")]
    public float patrolSpeed = 3f;
    public float chaseSpeed = 6f;
    public float attackRange = 2f;
    public float laserRange = 15f;
    public float restDuration = 2f;

    public interface IEnemyState
    {
        void EnterState();
        void UpdateState();
        void ExitState();
    }
    private elitemovement movement;
    private Player player;
    private bool lastAttackWasFast = false;

    // Start is called before the first frame update
    private void Start()
    {
        movement = GetComponent<elitemovement>();
        player = FindObjectOfType<Player>();

        // Initialize state machine with PatrolState
        ChangeState(new PatrolState(this));
    }

    // Update is called once per frame
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

    public void Move()
    {
        if (movement != null)
        {
            movement.Move();
        }
    }


    public bool IsPlayerInRange(float range)
    {
        if (player != null)
        {
            return Vector3.Distance(transform.position, player.transform.position) <= range;
        }
        return false;
    }

    public void StopMovement()
    {
        if (movement != null)
        {
            movement.Stop();
        }
    }

    public void FastMeleeAttack()
    {
        Debug.Log("Performing fast melee attack!");
        if (player != null && IsPlayerInRange(attackRange))
        {
            player.TakeDamage(10); // Fast melee attack deals low damage
        }
        lastAttackWasFast = true;
    }

    public void SlowMeleeAttack()
    {
        Debug.Log("Performing slow melee attack!");
        if (player != null && IsPlayerInRange(attackRange))
        {
            player.TakeDamage(30); // Slow melee attack deals high damage
        }
        lastAttackWasFast = false;
    }

    public void LaserAttack()
    {
        Debug.Log("Performing laser attack!");
        if (player != null && IsPlayerInRange(laserRange))
        {
            player.TakeDamage(20); // Laser attack deals medium damage
        }
    }
}

public interface IEnemyState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public class PatrolState : EliteEnemy.IEnemyState
{
    private EliteEnemy enemy;
    private float patrolTimer;
    private Vector3 patrolDirection;

    public PatrolState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        patrolTimer = 0f;
        ChangePatrolDirection();
    }

    public void UpdateState()
    {
        patrolTimer += Time.deltaTime;

        if (patrolTimer >= 3f)
        {
            ChangePatrolDirection();
            patrolTimer = 0f;
        }

        enemy.Move(); // Use the existing Move() method in elitemovement.

        if (enemy.IsPlayerInRange(10f))
        {
            enemy.ChangeState(new ChaseState(enemy));
        }
    }

    public void ExitState()
    {
        enemy.StopMovement();
    }

    private void ChangePatrolDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        patrolDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }
}

public class ChaseState : EliteEnemy.IEnemyState
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
        enemy.Move();
    }

    public void ExitState()
    {
        enemy.StopMovement();
    }
}

public class AttackState : EliteEnemy.IEnemyState
{
    private EliteEnemy enemy;
    private float attackCooldown;
    private float attackTimer;

    public AttackState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        attackTimer = 0f;
        Debug.Log("Attacking the player!");
    }

    public void UpdateState()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            if (enemy.LastAttackWasFast) // Use the public property
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
            if (enemy.lastAttackWasFast)
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

            attackTimer = 0f;
        }

        if (!enemy.IsPlayerInRange(enemy.attackRange))
        {
            enemy.ChangeState(new ChaseState(enemy));
        }
    }

    public void ExitState()
    {
        Debug.Log("Stopped attacking.");
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


