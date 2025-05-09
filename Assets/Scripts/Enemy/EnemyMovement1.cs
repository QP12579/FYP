using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EnemyMovement1 : MonoBehaviour
{
    [Header(" Elements ")]
    private Player player;

    [Header("Settings")]
    [SerializeField] private float moveSpeed;

    // Debuff Part
    private bool isDizziness = false;

    private Vector3 moveDirection; // 用於改變方向

    void Start()
    {
        // 初始化隨機方向
        ChangeDirection();
    }

    void Update()
    {
        if (player != null && !isDizziness)
        {
            FollowPlayer();
        }
    }

    public void StorePlayer(Player player)
    {
        this.player = player;
    }

    private void FollowPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Vector3 targetPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        transform.position = targetPosition;
    }

    // 碰撞檢測：當碰撞到牆壁時改變方向
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            ChangeDirection(); // 碰撞到牆壁時改變方向
        }
    }

    // 改變方向
    private void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }

    // Debuff
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

    public void DizzinessStart(float time)
    {
        isDizziness = true;
        Invoke("DizzinessEnd", time);
    }

    public void DizzinessEnd()
    {
        isDizziness = false;
    }
}
