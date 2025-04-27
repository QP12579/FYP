using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement1))]
public class RangedEnemy : Enemy
{
    [Header("Attack")]
    [SerializeField] private int damage;
    [SerializeField] private float attackFrequency;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    private float attackDelay;
    private float nextAttackTime;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        attackFrequency = Mathf.Clamp(attackFrequency, 0.01f, 2f); // 將最大攻擊頻率降低到 2
        attackDelay = 30f / attackFrequency; // 增加攻擊延遲，將分母調整為 3
        nextAttackTime = Time.time + attackDelay;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (Time.time >= nextAttackTime)
        {
            TryAttack();
            nextAttackTime = Time.time + attackDelay; // Add a delay between attacks
        }
    }

    private void TryAttack()
    {
        if (IsPlayerInRange())
        {
            Debug.Log("Player is in range. Attempting to attack.");
            Attack();
        }
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

        // 停止移動
        GetComponent<enemymovement>().StartAttack();

        // Instantiate and shoot the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        bullet bulletScript = projectile.GetComponent<bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetTarget(player.transform.position);
            bulletScript.damage = damage;
        }

        // 恢復移動
        GetComponent<enemymovement>().StopAttack();
    }

    public void EnemyGetHit(Vector3 hitDirection, float damage)
    {
        Debug.Log("Enemy got hit");
        GetHit(hitDirection, damage);
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
        else
        {
            
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
