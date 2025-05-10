using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class RangedEnemy : Enemy
{
    [Header("Attack")]
    [SerializeField] private int damage;
    [SerializeField] private float attackFrequency;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;
    [Header("Animation")]
    [SerializeField] private float attackAnimDuration = 0.5f; // 攻擊動畫長度（秒）

    private float attackDelay;
    private float nextAttackTime;

    protected override void Start()
    {
        base.Start();
        movement = GetComponent<EnemyMovement>();
        attackFrequency = Mathf.Clamp(attackFrequency, 0.01f, 2f);
        attackDelay = 30f / attackFrequency;
        nextAttackTime = Time.time + attackDelay;
    }

    protected override void Update()
    {
        base.Update();
        if (Time.time >= nextAttackTime)
        {
            TryAttack();
            nextAttackTime = Time.time + attackDelay;
        }
    }

    private void TryAttack()
    {
        if (IsPlayerInRange())
        {
            Debug.Log("Player is in range. Attempting to attack.");
            Attack();
            if (movement.anim != null)
            {
                movement.anim.SetBool("isAttack", true);
                StartCoroutine(ResetIsAttackBool());
            }
        }
    }

    private IEnumerator ResetIsAttackBool()
    {
        yield return new WaitForSeconds(attackAnimDuration);
        if (movement.anim != null)
            movement.anim.SetBool("isAttack", false);
    }

    private bool IsPlayerInRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        return distanceToPlayer < playerDetectionRadius;
    }

    private void Attack()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("Missing required components for attack.");
            return;
        }

        Debug.Log("Shooting at player with " + damage + " damage");

        // Instantiate and shoot the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetTarget(player.transform);
            projectileScript.SetDamage(damage);
        }

        // 恢復移動
        StartCoroutine(ResumeMovementAfterAttack());
    }

    private IEnumerator ResumeMovementAfterAttack()
    {
        yield return new WaitForSeconds(0.5f); // 攻擊後的延遲時間
    }
}

public class Projectile : MonoBehaviour
{
    public float speed = 10f;
    private int damage;
    private Transform target;

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    void Update()
    {
        if (target != null)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            transform.Translate(direction * speed * Time.deltaTime, Space.World);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Assuming the player has a script with a TakeDamage method
            // other.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject); // Destroy the projectile after it hits the player
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Destroy the projectile if it hits an obstacle
        }
    }
}
