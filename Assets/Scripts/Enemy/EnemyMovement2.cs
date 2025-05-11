using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement2 : EnemyMovement
{
    [Header("Movement Settings")]
    public float changeDirectionInterval = 2f; // 改變方向的間隔時間
    private float timeSinceLastChange = 0f; // 距離上次改變方向的時間

    [Header("Collision Settings")]
    [SerializeField] private string wallTag = "Wall"; // 牆壁的標籤

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // 如果速度為 0，則停止移動
        if (IsMovementStopped())
        {
            StopAnimation();
            return;
        }

        // 執行正常移動邏輯
        HandleMovement();
    }

    /// <summary>
    /// 判斷是否停止移動
    /// </summary>
    private bool IsMovementStopped()
    {
        return syncedMoveSpeed == 0;
    }

    /// <summary>
    /// 停止移動動畫
    /// </summary>
    private void StopAnimation()
    {
        if (anim != null)
        {
            anim.SetFloat("moveSpeed", 0); // 停止動畫
        }
    }

    /// <summary>
    /// 處理移動邏輯
    /// </summary>
    private void HandleMovement()
    {
        timeSinceLastChange += Time.deltaTime;

        // 定期改變方向
        if (timeSinceLastChange >= changeDirectionInterval)
        {
            ChangeDirection();
            timeSinceLastChange = 0f;
        }

        // 根據方向和速度移動
        MoveInDirection();
    }

    /// <summary>
    /// 根據當前方向移動
    /// </summary>
    private void MoveInDirection()
    {
        transform.Translate(moveDirection * syncedMoveSpeed * Time.deltaTime);

        if (anim != null)
        {
            anim.SetFloat("moveSpeed", 1); // 播放移動動畫
        }
    }

    /// <summary>
    /// 停止移動（攻擊時調用）
    /// </summary>
    public void StartAttack()
    {
        syncedMoveSpeed = 0; // 將速度設置為 0
        Debug.Log("EnemyMovement2: Movement stopped for attack.");
    }

    /// <summary>
    /// 恢復移動（攻擊結束後調用）
    /// </summary>
    public void StopAttack()
    {
        syncedMoveSpeed = moveSpeed; // 恢復原始速度
        Debug.Log("EnemyMovement2: Movement resumed after attack.");
    }

    /// <summary>
    /// 碰撞檢測：當敵人碰到牆壁時改變方向
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(wallTag))
        {
            Debug.Log("EnemyMovement2: Collided with a wall. Changing direction.");
            ChangeDirection(); // 碰撞牆壁時改變方向
        }
    }
}
