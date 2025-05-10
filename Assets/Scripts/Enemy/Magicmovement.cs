using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magicmovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.5f;           // 行走速度
    public float minMoveDistance = 3f;       // 最小隨機移動距離
    public float maxMoveDistance = 7f;       // 最大隨機移動距離

    [Header("Teleport Settings")]
    public float minTeleportInterval = 8f;   // 最短傳送間隔
    public float maxTeleportInterval = 12f;  // 最長傳送間隔
    public float teleportRange = 10f;        // 傳送最大半徑

    [Header("Debug/Test")]
    public KeyCode teleportTestKey = KeyCode.T; // 測試用按鍵

    [Header("Animation")]
    public float appearAnimDuration = 0.5f; // isAppear動畫持續時間（依動畫長度調整）

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isStopped = false;
    private Animator anim;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        SetRandomTargetPosition();
        StartCoroutine(TeleportRoutine());
    }

    private void Update()
    {
        if (isStopped)
        {
            if (anim != null)
                anim.SetFloat("movespeed", 0f);
            return;
        }

        RandomMove();

        // 測試用：按下 T 鍵立即傳送
        if (Input.GetKeyDown(teleportTestKey))
        {
            TeleportRandomly();
        }
    }

    // 隨機行走
    private void RandomMove()
    {
        if (!isMoving)
        {
            SetRandomTargetPosition();
        }

        float speed = moveSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (anim != null)
        {
            float currentSpeed = isMoving ? speed : 0f;
            anim.SetFloat("movespeed", currentSpeed);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            if (anim != null)
                anim.SetFloat("movespeed", 0f);
        }
    }

    // 設定新的隨機目標位置
    private void SetRandomTargetPosition()
    {
        float randomDistance = Random.Range(minMoveDistance, maxMoveDistance);
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        targetPosition = transform.position + randomDirection.normalized * randomDistance;
        isMoving = true;
    }

    // 傳送協程
    private IEnumerator TeleportRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTeleportInterval, maxTeleportInterval);
            yield return new WaitForSeconds(waitTime);
            TeleportRandomly();
        }
    }

    // 隨機傳送到附近
    private void TeleportRandomly()
    {
        Vector3 randomOffset = Random.insideUnitSphere * teleportRange;
        randomOffset.y = 0;
        transform.position += randomOffset;
        isMoving = false; // 傳送後重新選擇目標
        Debug.Log("MagicElite teleported!");

        if (anim != null)
        {
            anim.SetBool("isAppear", true);
            StartCoroutine(ResetIsAppearAfterDelay());
        }
    }

    private IEnumerator ResetIsAppearAfterDelay()
    {
        yield return new WaitForSeconds(appearAnimDuration);
        if (anim != null)
            anim.SetBool("isAppear", false);
    }

    // 停止移動
    public void Stop()
    {
        isStopped = true;
        if (anim != null)
            anim.SetFloat("movespeed", 0f);
    }

    // 恢復移動
    public void Move()
    {
        isStopped = false;
    }
}
