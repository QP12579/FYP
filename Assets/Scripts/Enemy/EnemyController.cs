using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public float maxHP = 100;
    public float currentHP;

    public Image hpBar; // Reference to the Image component
    public float animationSpeed = 0.1f; // Speed of the animation

    public float detectionRange = 10f; // Range to detect the player
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform bulletSpawnPoint; // Point from where the bullet will be spawned
    public float bulletSpeed = 10f; // Speed of the bullet
    public LayerMask obstacleLayer; // Layer mask to detect obstacles

    public float dashRange = 5f; // Range to dash towards the player
    public float dashSpeed = 20f; // Speed of the dash
    public float dashDamage = 20f; // Damage dealt by the dash

    public float attackCooldown = 3f; // Cooldown time between attacks

    private float targetFillAmount;
    private Transform player;
    private bool isDashing = false;
    private bool isAttacking = false;
    private bool isOnCooldown = false;

    // Start is called before the first frame update
    void Start()
    {
        
        targetFillAmount = 1f;
        UpdateHPBar();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Update the position of the hpBar to follow the enemy
        if (hpBar != null)
        {
            hpBar.transform.position = transform.position + Vector3.up;
            // Smoothly animate the fill amount
            //hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
        }

        if (!isDashing && !isAttacking && !isOnCooldown)
        {
            DetectAndAttackPlayer();
        }
    }

    public void TakeDamage(float damage)
    {
        gameObject.GetComponent<Animator>().SetTrigger("hurt");
        currentHP -= damage;
        targetFillAmount = currentHP / maxHP;
        Debug.Log("Hurt" + targetFillAmount);
        UpdateHPBar();
        UpdateHPBarColor();
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = targetFillAmount;
            Debug.Log("EnemyHP" + hpBar.fillAmount);
            UpdateHPBarColor();
        }
    }

    void UpdateHPBarColor()
    {
        if (hpBar != null)
        {
            if (currentHP > maxHP * 0.5f)
            {
                hpBar.color = Color.green;
            }
            else if (currentHP > maxHP * 0.25f)
            {
                hpBar.color = Color.yellow;
            }
            else
            {
                hpBar.color = Color.red;
            }
        }
    }

    void Die()
    {
        // Handle enemy death (e.g., play animation, destroy object)
        Debug.Log("Enemy died");
        Destroy(hpBar.gameObject); // Destroy the HP bar
        Destroy(gameObject, 0.5f);
    }

    void DetectAndAttackPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= detectionRange)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, detectionRange, obstacleLayer))
            {
                if (hit.transform.CompareTag("Player"))
                {
                    int attackType = Random.Range(0, 2); // Randomly choose between 0 and 1
                    if (attackType == 0)
                    {
                        StartCoroutine(PerformShootPlayer());
                    }
                    else
                    {
                        StartCoroutine(PerformDashToPlayer(directionToPlayer));
                    }
                }
            }
        }
    }

    IEnumerator PerformDashToPlayer(Vector3 direction)
    {
        isOnCooldown = true;

        for (int i = 0; i < 2; i++)
        {
            yield return DashToPlayer(direction);
            yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds between dashes
        }

        yield return new WaitForSeconds(attackCooldown); // Wait for cooldown
        isOnCooldown = false;
    }

    IEnumerator DashToPlayer(Vector3 direction)
    {
        isDashing = true;
        isAttacking = true;

        // Stop moving for 0.5 seconds before dashing
        yield return new WaitForSeconds(0.5f);

        float dashTime = dashRange / dashSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < dashTime)
        {
            transform.position += direction * dashSpeed * Time.deltaTime;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (Vector3.Distance(transform.position, player.position) <= 1f)
        {
            //player.GetComponent<PlayerController>().TakeDamage(dashDamage);
        }

        isDashing = false;
        isAttacking = false;
    }

    IEnumerator PerformShootPlayer()
    {
        isOnCooldown = true;

        for (int i = 0; i < 2; i++)
        {
            yield return ShootPlayer();
            yield return new WaitForSeconds(0.5f); // Wait for 0.5 seconds between shots
        }

        yield return new WaitForSeconds(attackCooldown); // Wait for cooldown
        isOnCooldown = false;
    }

    IEnumerator ShootPlayer()
    {
        isAttacking = true;

        // Stop moving for 0.5 seconds before shooting
        yield return new WaitForSeconds(0.5f);

        Vector3 directionToPlayer = (player.position - bulletSpawnPoint.position).normalized;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = directionToPlayer * bulletSpeed;

        isAttacking = false;
    }
}

