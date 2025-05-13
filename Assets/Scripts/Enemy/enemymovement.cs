using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public Animator anim;
    protected Rigidbody rb;

    [Header("Elements")]
    protected Player player;

    protected Vector3 moveDirection;

    [Header("Hit Effect")]
    [SerializeField] protected float hitBackForce = 5f;
    [SerializeField] protected float hitBackDuration = 0.5f; // 擊退持續時間

    [SyncVar]
    protected float syncedMoveSpeed;

    // Debuff Part
    protected bool isDizziness = false;
    protected bool isSlow = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        syncedMoveSpeed = moveSpeed;

        if (anim == null)
            anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();
        ChangeDirection();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isDizziness)
            DifferentMovement();
    }

    protected virtual void DifferentMovement()
    {
        if (anim != null)
            anim.SetFloat("moveSpeed", 1);
    }

    [Server]
    public void StorePlayer(Player player)
    {
        this.player = player;
    }

    [Server]
    protected void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;

        // 確保方向不為零向量
        if (moveDirection.magnitude < 0.1f)
        {
            ChangeDirection();
        }
    }

    [ServerCallback]
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            ChangeDirection();
        }
    }

    /// <summary>
    /// Apply a hit-back effect to the enemy.
    /// </summary>
    /// <param name="hitDirection">The direction of the hit as a 2D vector.</param>
    public void ApplyHitBack(Vector3 hitDirection)
    {
        // Temporarily store the current speed
        float originalSpeed = moveSpeed;

        // Stop movement
        moveSpeed = 0f;

        anim.SetBool("isAttack", false);

        // Ensure Rigidbody exists
        if (rb != null)
        {
            // Calculate the hit-back force in 3D
            Vector3 force = new Vector3(hitDirection.x, 0, hitDirection.y) * hitBackForce;

            // Apply the force to the Rigidbody
            rb.AddForce(force, ForceMode.Impulse);
        }

        // Restore the original speed after the hit-back effect
        StartCoroutine(RestoreSpeedAfterDelay(originalSpeed, hitBackDuration));
    }

    /// <summary>
    /// Coroutine to restore the enemy's speed after a delay.
    /// </summary>
    /// <param name="originalSpeed">The original speed to restore.</param>
    /// <param name="delay">The delay before restoring the speed.</param>
    private IEnumerator RestoreSpeedAfterDelay(float originalSpeed, float delay)
    {
        yield return new WaitForSeconds(delay);
        moveSpeed = originalSpeed;
    }

    // Network
    [Server]
    public void LowerSpeedStart(float time, float lowSpeedPercentage)
    {
        float baseMoveSpeed = moveSpeed;
        moveSpeed *= lowSpeedPercentage;

        // Use a coroutine to restore speed
        StartCoroutine(RestoreSpeedAfterDelay(baseMoveSpeed, time));
    }

    public void GoToBaseSpeed(float baseSpeed)
    {
        moveSpeed = baseSpeed;
    }

    // Debuff
    [Server]
    public void DizzinessStart(float time)
    {
        float baseMoveSpeed = moveSpeed;
        moveSpeed = 0;
        isDizziness = true;

        // Trigger hurt animation
        if (anim != null)
        {
            anim.SetTrigger("hurt");
            anim.SetFloat("moveSpeed", 0);
        }

        // Show visual effect for clients
        RpcShowDizzinessEffect(true);

        // Reset after the specified time
        StartCoroutine(EndDizzinessAfterDelay(time, baseMoveSpeed));
    }

    [Server]
    protected IEnumerator EndDizzinessAfterDelay(float time, float baseSpeed)
    {
        yield return new WaitForSeconds(time);
        moveSpeed = baseSpeed;
        isDizziness = false;

        if (anim != null)
            anim.SetFloat("moveSpeed", 1);

        // Turn off visual effect
        RpcShowDizzinessEffect(false);
    }

    // Client-side visual effects
    [ClientRpc]
    protected void RpcShowDizzinessEffect(bool active)
    {
        // Add visual effects for dizziness here
        Debug.Log("Showing dizziness effect: " + active);
    }

    [ClientRpc]
    protected void RpcShowSlowEffect(bool active)
    {
        // Add visual effects for slow here
        Debug.Log("Showing slow effect: " + active);
    }
}
