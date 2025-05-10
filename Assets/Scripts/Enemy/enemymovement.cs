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

    [Header(" Elements ")]
    protected Player player;

    protected Vector3 moveDirection;

    [Header("Hit Effect")]
    [SerializeField] protected float hitBackForce = 5f;

    [SyncVar]
    protected float syncedMoveSpeed;
    // Debuff Part
    protected bool isDizziness = false;
    protected bool isSlow = false;

    // Start is called before the first frame update
    [ServerCallback]
    protected virtual void Start()
    {
        // 初始化隨機方向
        syncedMoveSpeed = moveSpeed;
        if(anim == null)
            anim = GetComponent<Animator>();

        rb = GetComponent<Rigidbody>();
        ChangeDirection();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if(!isDizziness)
            DifferentMovement();
    }    
    
    protected virtual void DifferentMovement()
    {
        anim.SetFloat("moveSpeed", 1);
    }

    [Server]
    public void StorePlayer(Player player)
    {
        this.player = player;
    }

    [Server]
    // 改變方向
    protected void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }

    [ServerCallback]
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            ChangeDirection(); // 碰撞到牆壁時改變方向
        }
    }

    public void ApplyHitBack(Vector2 hitDirection)
    {
        float baseSpeed = moveSpeed;
        moveSpeed = 0f;
        if (rb != null)
        {
            rb.AddForce(hitDirection * hitBackForce, ForceMode.Impulse);
        }
        LeanTween.delayedCall(hitBackForce, () =>GoToBaseSpeed(baseSpeed));
    }

    // Network
    [Server]
    public void LowerSpeedStart(float time, float lowSpeedPersentage)
    {
        float baseMoveS = moveSpeed;
        moveSpeed *= lowSpeedPersentage;
        LeanTween.delayedCall(time, () => GoToBaseSpeed(baseMoveS));
    }

    public void GoToBaseSpeed(float baseSpeed)
    {
        moveSpeed = baseSpeed;
    }

    // Debuff
    [Server]
    public void DizzinessStart(float time)
    {
        float baseMoveS = moveSpeed;
        moveSpeed = 0;
        isDizziness = true;
        // animation for dizziness maybe
        anim.SetTrigger("hurt");
        anim.SetFloat("moveSpeed", 0);

        // Visual effect for clients
        RpcShowDizzinessEffect(true);

        // Reset after time
        StartCoroutine(EndDizzinessAfterDelay(time, baseMoveS));
    }

    [Server]
    protected IEnumerator EndDizzinessAfterDelay(float time, float baseSpeed)
    {
        yield return new WaitForSeconds(time);
        moveSpeed = baseSpeed;
        isDizziness = false;

        anim.SetFloat("moveSpeed", 1);

        // Turn off visual effect
        RpcShowDizzinessEffect(false);
    }

    // Client-side visual effects
    [ClientRpc]
    protected void RpcShowDizzinessEffect(bool active)
    {
        // Add visual effects for dizziness here
        // For example, you could play a particle effect or animation
        Debug.Log("Showing dizziness effect: " + active);
    }

    [ClientRpc]
    protected void RpcShowSlowEffect(bool active)
    {
        // Add visual effects for slow here
        // For example, you could tint the sprite or play a particle effect
        Debug.Log("Showing slow effect: " + active);
    }
}
