using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Enemy : MonoBehaviour
{

   
    [Header(" Components ")]
    protected EnemyMovement1 movement;

    [Header(" Elements ")]
    protected Player player;

    [Header("Spawn Sequence Realted ")]
    [SerializeField] protected SpriteRenderer renderer;
    [SerializeField] protected SpriteRenderer spawnIndicator;
    protected bool hasSpawned;

    [Header(" Effects ")]
    [SerializeField] protected ParticleSystem passAwayParticles;

    [Header("Attack")]
    [SerializeField] protected float playerDetectionRadius;

    [Header("  DEBUG  ")]
    [SerializeField] protected bool gizmos;
    protected virtual void Start()
    {
        movement = GetComponent<EnemyMovement1>();

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

        // SHow spawn indicator
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
        //Unparent the particles and play them
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
}
