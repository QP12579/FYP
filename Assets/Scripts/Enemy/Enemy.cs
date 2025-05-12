using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using Mirror;

public abstract class Enemy : NetworkBehaviour, IAttackable, IDebuffable
{
    [Header(" Components ")]
    protected EnemyMovement movement;
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

        movement = GetComponent<EnemyMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

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
                transform.SetParent(parentIdentity.GetComponentInChildren<WaveManager>().transform, false); // true = worldPositionStays
            }
        }
    }

    protected virtual void FindingPlayer()
    {
        // If we already have a target player assigned via SetTargetPlayer, use that
        if (player != null)
        {
            // Start spawn sequence with existing player
            StartSpawnSequence();
            return;
        }

        // Otherwise, try to find by network ID
        if (targetPlayerNetId != 0)
        {
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
        else
        {
            // Fallback to finding any player if no specific player is targeted
            // This should be less common if we're properly setting targets
            player = FindObjectOfType<Player>();
            if (player != null)
            {
                Debug.Log("Found a player as fallback");
                SetTargetPlayer(player);
                StartSpawnSequence();
                return;
            }
            else
            {
                Debug.LogWarning("No target player set for this enemy, and no fallback found!");
                LeanTween.delayedCall(0.1f, FindingPlayer);
            }
        }
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
    }

    private void StartSpawnSequence()
    {
        SetRendererVisibility(false);
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

        if (movement != null && player != null)
        {
            movement.StorePlayer(player);
        }
    }

    private void SetRendererVisibility(bool visibility = true)
    {
        //spriterenderer.enabled = visibility;
        if (spawnIndicator != null)spawnIndicator.enabled = !visibility;
        
    }

    // Method for server to apply damage

    [Command(requiresAuthority = false)]
    public void TakeDamage(Vector3 attackerPosi, float damage)
    {
        if (movement != null && movement.anim != null)
            movement.anim.SetTrigger("hurt");

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
        RPCTakeDamage();

    }

    [ClientRpc]
    public void RPCTakeDamage()
    {
        if (!isServer)
            targetFillAmount = currentHP / maxHP;
    }
    

    // Method for clients to request damage be applied
    [Command(requiresAuthority = false)]
    public void CmdTakeDamage(Vector3 attackerPosi, float damage, NetworkConnectionToClient sender = null)
    {
        // Optional: Add validation here if needed
        // For example, check if the sender is allowed to damage this enemy

        // Apply the damage on the server
        TakeDamage(attackerPosi, damage);
    }

    [ClientRpc]
    private void RpcPlayHurtAnimation()
    {
        if (movement != null && movement.anim != null)
            movement.anim.SetTrigger("hurt");
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

    [ClientRpc]
    public void RPCPassAway(GameObject enemy)
    {
       DestroyImmediate(enemy);
        Debug.Log(" Died ");
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
        RPCPassAway(gameObject);
    
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
        if (movement != null)
            movement.ApplyHitBack(hitDirection);
    }

    private IEnumerator ShowHitEffect()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(hitEffectDuration);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return null;
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
                if (movement != null)
                    movement.DizzinessStart(time);
                break;
            case DeBuffType.Slow:
                if (movement != null)
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