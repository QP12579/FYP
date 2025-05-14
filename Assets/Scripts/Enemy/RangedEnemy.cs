using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class RangedEnemy : Enemy
{
    [Header("Attack")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackFrequency = 1f; // 每秒攻擊次數
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform firePoint;

    [Header("Animation")]
    [SerializeField] private float attackAnimDuration = 0.5f; // 攻擊動畫長度（秒）

    private float nextAttackTime;

    protected override void Start()
    {
        base.Start();
        movement = GetComponent<EnemyMovement>();
        attackFrequency = Mathf.Clamp(attackFrequency, 0.01f, 10f);
        nextAttackTime = Time.time;
    }

    protected override void Update()
    {
        base.Update();
        if (Time.time >= nextAttackTime)
        {
            TryAttack();
            nextAttackTime = Time.time + 1f / attackFrequency;
        }
    }

    private void TryAttack()
    {
        if (IsPlayerInRange())
        {
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
        if (player == null) return false;
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        return distanceToPlayer < playerDetectionRadius;
    }

    private void Attack()
    {
        if (projectilePrefab == null || firePoint == null || player == null)
        {
            Debug.LogWarning("RangedEnemy: Missing projectilePrefab, firePoint, or player.");
            return;
        }

        // 生成並發射投射物
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.SetTarget(player.transform);
            projectileScript.SetDamage(damage);
        }

        if (ATK_SFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySFX(ATK_SFX, this.transform);
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
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                // 這裡假設玩家有 TakeDamage(float) 方法
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
