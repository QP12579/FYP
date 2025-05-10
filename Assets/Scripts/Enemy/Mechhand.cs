using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mechhand : MonoBehaviour
{
    [Header("Hand Settings")]
    public float shootSpeed = 20f;         // 射出速度
    public float maxDistance = 10f;        // 最遠射程
    public float returnSpeed = 10f;        // 回收初始速度
    public float returnMaxSpeed = 40f;     // 回收最大速度
    public float returnAcceleration = 60f; // 回收加速度（單位：每秒速度增量）

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isShooting = false;
    private bool isReturning = false;
    private float currentReturnSpeed = 0f;
    private System.Action onHandReturned;  // 可選：回收後的回呼

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (isShooting)
        {
            // 前進
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, shootSpeed * Time.deltaTime);

            // 到達最大距離或目標點
            if (Vector3.Distance(startPosition, transform.position) >= maxDistance ||
                Vector3.Distance(transform.position, targetPosition) < 0.01f)
            {
                isShooting = false;
                isReturning = true;
                currentReturnSpeed = returnSpeed; // 回收時重設初始速度
            }
        }
        else if (isReturning)
        {
            // 回收加速
            currentReturnSpeed += returnAcceleration * Time.deltaTime;
            currentReturnSpeed = Mathf.Min(currentReturnSpeed, returnMaxSpeed);

            transform.position = Vector3.MoveTowards(transform.position, startPosition, currentReturnSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, startPosition) < 0.01f)
            {
                isReturning = false;
                // 回到原點後可觸發事件
                onHandReturned?.Invoke();
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// 射出手臂到指定方向
    /// </summary>
    /// <param name="targetWorldPosition">目標世界座標</param>
    /// <param name="onReturned">回收後的回呼（可選）</param>
    public void ShootAt(Vector3 targetWorldPosition, System.Action onReturned = null)
    {
        if (isShooting || isReturning) return;

        startPosition = transform.position;
        Vector3 direction = (targetWorldPosition - startPosition).normalized;
        targetPosition = startPosition + direction * maxDistance;
        isShooting = true;
        isReturning = false;
        onHandReturned = onReturned;
    }

    /// <summary>
    /// 強制立即回收手臂
    /// </summary>
    public void ForceReturn()
    {
        isShooting = false;
        isReturning = true;
        currentReturnSpeed = returnSpeed;
    }
}
