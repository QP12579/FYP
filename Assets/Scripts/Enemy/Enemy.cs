using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour, IAttackable, IDebuffable
{
    [Header(" Components ")]
    protected EnemyMovement1 movement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header(" Elements ")]
    protected Player player;

    [Header("Spawn Sequence Related ")]
    [SerializeField] protected SpriteRenderer spriterenderer;
    [SerializeField] protected SpriteRenderer spawnIndicator;
    protected bool hasSpawned;

    [Header(" Effects ")]
    [SerializeField] protected ParticleSystem passAwayParticles;

    [Header("Attack")]
    [SerializeField] protected float playerDetectionRadius;

    [Header("Actions")]
    public static Action<Vector3> OnPassAway;

    [Header("  DEBUG  ")]
    [SerializeField] protected bool gizmos;

    [Header("Hit Effect")]
    [SerializeField] protected float hitEffectDuration = 0.2f;
    [SerializeField] protected float hitBackForce = 5f;

    [Header("EnemyController")]
    public float maxHP = 100;
    public float currentHP;
    public Image hpBar; // Reference to the Image component
    public float animationSpeed = 0.1f; // Speed of the animation
    public float detectionRange = 10f; // Range to detect the player
    public LayerMask obstacleLayer; // Layer mask to detect obstacles
    public float dashRange = 5f; // Range to dash towards the player
    public float dashSpeed = 20f; // Speed of the dash
    public float dashDamage = 20f; // Damage dealt by the dash
    public float attackCooldown = 3f; // Cooldown time between attacks
    private float targetFillAmount;
    private bool isDashing = false;
    private bool isAttacking = false;
    private bool isOnCooldown = false;
    private Transform playerTransform;

    protected virtual void Start()
    {
        targetFillAmount = 1f;
        UpdateHPBar();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        movement = GetComponent<EnemyMovement1>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        player = FindFirstObjectByType<Player>();

        if (player == null)
        {
            Debug.LogWarning(" No player found , Auto Destroying..");
            Destroy(gameObject);
        }

        StartSpawnSequence();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        // Update the position of the hpBar to follow the enemy
        if (hpBar != null)
        {
            hpBar.transform.position = transform.position + Vector3.up;
        }

        if (!isDashing && !isAttacking && !isOnCooldown)
        {
            //DetectAndAttackPlayer();
        }

        if (!spriteRenderer.enabled)
            return;
    }

    private void StartSpawnSequence()
    {
        SetRendererVisibility(false);
        // Hide renderer
        //spriterenderer.enabled = false;

        // Show spawn indicator
        spawnIndicator.enabled = true;

        // Scale up and down the spawn indicator
        Vector3 targetScale = spawnIndicator.transform.localScale * 1.2f;
        LeanTween.scale(spawnIndicator.gameObject, targetScale, .3f)
            .setLoopPingPong(4)
            .setOnComplete(SpawnSequenceCompleted);
    }

    private void SpawnSequenceCompleted()
    {
        SetRendererVisibility(true);
        hasSpawned = true;

        movement.StorePlayer(player);
    }

    private void SetRendererVisibility(bool visibility = true)
    {
        //spriterenderer.enabled = visibility;
        spawnIndicator.enabled = !visibility;
    }

    public void TakeDamage(float damage)
    {
        gameObject.GetComponent<Animator>().SetTrigger("hurt");
        float realdamage = Mathf.Min(damage, currentHP);
        currentHP -= realdamage;
        targetFillAmount = currentHP / maxHP;
        Debug.Log("Hurt" + targetFillAmount);
        UpdateHPBar();
        UpdateHPBarColor();
        if (currentHP <= 0)
        {
            PassAway();
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

    public void PassAway()
    {
        OnPassAway?.Invoke(transform.position);
        Destroy(gameObject, 0.5f);
    }

    private void OnDrawGizmos()
    {
        if (gizmos)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
    }

    public void GetHit(Vector2 hitDirection, float damage)
    {
        StartCoroutine(ShowHitEffect());
        ApplyHitBack(hitDirection);
    }

    private IEnumerator ShowHitEffect()
    {
        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(hitEffectDuration);
        spriteRenderer.color = originalColor;
    }

    private void ApplyHitBack(Vector2 hitDirection)
    {
        if (rb != null)
        {
            rb.AddForce(hitDirection * hitBackForce, ForceMode2D.Impulse);
        }
    }

    public void DeBuff(DeBuffType deBuffType, float time, float debuffStats)
    {
        switch (deBuffType)
        {
            case DeBuffType.Blooding:
                StartCoroutine(BloodingTimer(deBuffType, time, debuffStats));
                break;
            case DeBuffType.Dizziness:
                movement.DizzinessStart(time);
                break;
            case DeBuffType.Slow:
                movement.LowerSpeedStart(time, debuffStats);
                break;

        }
    }

    IEnumerator BloodingTimer(DeBuffType deBuffType, float time, float debuffP) 
    {
        while (time > 0)
        {
            yield return new WaitForSeconds(1f);
            TakeDamage(debuffP);
            time -= 1f;
        }
    }
}
