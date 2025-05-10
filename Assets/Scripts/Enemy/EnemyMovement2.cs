using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement2 : EnemyMovement
{
    public float changeDirectionInterval = 2f; // 改變方向的間隔時間
    private float timeSinceLastChange = 0f; // 距離上次改變方向的時間

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        // 如果速度為 0，則停止移動
        if (syncedMoveSpeed == 0)
        {
            anim.SetFloat("moveSpeed", 0); // 停止動畫
            return;
        }

        // 如果速度不為 0，執行正常移動邏輯
        DifferentMovement();
    }

    protected override void DifferentMovement()
    {
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= changeDirectionInterval)
        {
            ChangeDirection(); // 改變移動方向
            timeSinceLastChange = 0f;
        }

        // 根據方向和速度移動
        transform.Translate(moveDirection * syncedMoveSpeed * Time.deltaTime);
        anim.SetFloat("moveSpeed", 1); // 播放移動動畫
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
}
