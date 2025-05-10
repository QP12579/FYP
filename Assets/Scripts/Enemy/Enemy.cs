using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Mirror;

public abstract class Enemy : NetworkBehaviour, IAttackable, IDebuffable
{
    [Header(" Components ")]
    protected EnemyMovement1 movement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header(" Elements ")]
    protected Player player;
    public uint NetId => netId;
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

    [SyncVar(hook = nameof(OnHPChanged))]
    public float currentHP;

    [Header("EnemyController")]
    public float maxHP = 100;
   
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
    [SyncVar]
    private uint targetPlayerNetId;

    [Server]
    public void SetTargetPlayer(Player targetPlayer)
    {
        if (targetPlayer != null)
        {
            targetPlayerNetId = targetPlayer.netId;
            // Also set the local reference for server
            player = targetPlayer;

            // If we already have movement component, update it
            if (movement != null)
            {
                movement.StorePlayer(targetPlayer);
            }
        }
    }

    protected virtual void Start()
    {
        targetFillAmount = 1f;
        UpdateHPBar();

        movement = GetComponent<EnemyMovement1>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;

        player = FindFirstObjectByType<Player>();

        if (isServer)
        {
            FindingPlayer();

        }
        UpdateHPBar();
    }
    void OnHPChanged(float oldValue, float newValue)
    {
        targetFillAmount = newValue / maxHP;
        UpdateHPBar();
    }

    [ClientRpc]
    public virtual void RpcSetParent(uint parentNetId)
    {
        if (!isServer)
        {
            if (NetworkClient.spawned.TryGetValue(parentNetId, out NetworkIdentity parentIdentity))
            {
                transform.SetParent(parentIdentity.GetComponentInChildren<WaveManager>().transform, true); // true = worldPositionStays
            }
        }
    }

    protected virtual void FindingPlayer()
    {
        if (targetPlayerNetId == 0)
        {
            // If no specific target was set, we can't proceed
            Debug.LogWarning("No target player set for this enemy!");
            return;
        }

        // Find the player with matching netId
        foreach (Player p in FindObjectsOfType<Player>())
        {
            if (p.netId == targetPlayerNetId)
            {
                player = p;
                Debug.Log($"Found target player with ID {targetPlayerNetId}");

                // Start spawn sequence after finding player
                StartSpawnSequence();
                return;
            }
        }

        // If we didn't find the player, try again shortly
        LeanTween.delayedCall(0.1f, FindingPlayer);
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

       // if (!spriteRenderer.enabled)
           // return;
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
    
    public void TakeDamage(Vector3 attackerPosi, float damage)
    {
        gameObject.GetComponent<Animator>().SetTrigger("hurt");
        float realdamage = Mathf.Min(damage, currentHP);
        currentHP -= realdamage;
        targetFillAmount = currentHP / maxHP;
        GetHit(attackerPosi, realdamage);
        UpdateHPBar();
        UpdateHPBarColor();
        if (currentHP <= 0)
        {
            PassAway();
        }
    }

    [ClientRpc]
    private void RpcPlayHurtAnimation()
    {
        if (gameObject.GetComponent<Animator>() != null)
            gameObject.GetComponent<Animator>().SetTrigger("hurt");
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = targetFillAmount;
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

    [Server]
    public void PassAway()
    {
        // Play death effect on all clients
        RpcPlayDeathEffect();

        // Trigger the static action
        OnPassAway?.Invoke(transform.position);

        // Destroy on server, which will automatically destroy on clients
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcPlayDeathEffect()
    {
        // Play death effect on all clients
        if (passAwayParticles != null)
            passAwayParticles.Play();
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

    [Server]
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
            TakeDamage(gameObject.transform.position, debuffP);
            time -= 1f;
        }
    }
}
