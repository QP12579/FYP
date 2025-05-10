using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EnemyMovement1 : NetworkBehaviour
{
    [Header(" Elements ")]
    private Player player;
    private Player targetPlayer;
    [Header("Settings")]
    [SerializeField] private float moveSpeed = 5f;

    // Debuff Part
    private bool isDizziness = false;

    private Vector3 moveDirection; // 用於改變方向
    [SyncVar]
    private float syncedMoveSpeed;

    [ServerCallback]
    void Start()
    {
        // 初始化隨機方向
        syncedMoveSpeed = moveSpeed;
        ChangeDirection();
    }

    void Update()
    {
        if (player != null && !isDizziness)
        {
            FollowPlayer();
        }
    }

    [Server]
    public void StorePlayer(Player player)
    {
        this.player = player;
    }


    [Server] 
    private void FollowPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        Vector3 targetPosition = transform.position + direction * syncedMoveSpeed * Time.deltaTime;
        transform.position = targetPosition;
    }

    // 碰撞檢測：當碰撞到牆壁時改變方向
    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            ChangeDirection(); // 碰撞到牆壁時改變方向
        }
    }

    // 改變方向
    [Server]
    private void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }

    // Debuff
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
   
    [Server]
    public void DizzinessStart(float time)
    {
        isDizziness = true;

        // Visual effect for clients
        RpcShowDizzinessEffect(true);

        // Reset after time
        StartCoroutine(EndDizzinessAfterDelay(time));
    }

    [Server]
    private IEnumerator EndDizzinessAfterDelay(float time)
    {
        yield return new WaitForSeconds(time);
        isDizziness = false;

        // Turn off visual effect
        RpcShowDizzinessEffect(false);
    }

    // Client-side visual effects
    [ClientRpc]
    private void RpcShowDizzinessEffect(bool active)
    {
        // Add visual effects for dizziness here
        // For example, you could play a particle effect or animation
        Debug.Log("Showing dizziness effect: " + active);
    }

    [ClientRpc]
    private void RpcShowSlowEffect(bool active)
    {
        // Add visual effects for slow here
        // For example, you could tint the sprite or play a particle effect
        Debug.Log("Showing slow effect: " + active);
    }
}
