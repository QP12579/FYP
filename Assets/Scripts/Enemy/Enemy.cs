using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Enemy : MonoBehaviour
{
    [Header(" Components ")]
    protected EnemyMovement1 movement;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    [Header(" Elements ")]
    protected Player player;

    [Header("Spawn Sequence Related ")]
    [SerializeField] protected SpriteRenderer renderer;
    [SerializeField] protected SpriteRenderer spawnIndicator;
    protected bool hasSpawned;

    [Header(" Effects ")]
    [SerializeField] protected ParticleSystem passAwayParticles;

    [Header("Attack")]
    [SerializeField] protected float playerDetectionRadius;

    [Header("  DEBUG  ")]
    [SerializeField] protected bool gizmos;

    [Header("Hit Effect")]
    [SerializeField] protected float hitEffectDuration = 0.2f;
    [SerializeField] protected float hitBackForce = 5f;

    protected virtual void Start()
    {
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
    void Update()
    {

    }

    private void StartSpawnSequence()
    {
        SetRendererVisibility(false);
        // Hide renderer
        renderer.enabled = false;

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
        renderer.enabled = visibility;
        spawnIndicator.enabled = !visibility;
    }

    private void PassAway()
    {
        // Unparent the particles and play them
        passAwayParticles.transform.SetParent(null);
        passAwayParticles.Play();

        Destroy(gameObject);
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
        // Apply damage
        // (You can add your own damage handling logic here)

        // Apply hit effect
        StartCoroutine(ShowHitEffect());

        // Apply knockback
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
}
