using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magicmovement : EnemyMovement
{
    [Header("Movement Settings")]
    public float minMoveDistance = 3f;
    public float maxMoveDistance = 7f;

    [Header("Teleport Settings")]
    public float minTeleportInterval = 8f;
    public float maxTeleportInterval = 12f;
    public float teleportRange = 10f;

    [Header("Debug/Test")]
    public KeyCode teleportTestKey = KeyCode.T;

    [Header("Animation")]
    public float appearAnimDuration = 0.5f;

    [Header("Floor Reference")]
    public GameObject floor; // 拖曳你的 floor prefab 進來

    private Vector3 areaMin;
    private Vector3 areaMax;

    private Vector3 targetPosition;
    private bool isMoving = false;
    private bool isStopped = false;
    // 移除 private Animator anim;，直接使用繼承的 anim

    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
        SetRandomTargetPosition();
        StartCoroutine(TeleportRoutine());

        // 自動取得 floor 範圍
        if (floor != null)
        {
            BoxCollider box = floor.GetComponent<BoxCollider>();
            if (box != null)
            {
                Vector3 center = box.transform.position + box.center;
                Vector3 size = Vector3.Scale(box.size, box.transform.lossyScale) * 0.5f;
                areaMin = center - size;
                areaMax = center + size;
            }
            else
            {
                MeshRenderer mesh = floor.GetComponent<MeshRenderer>();
                if (mesh != null)
                {
                    areaMin = mesh.bounds.min;
                    areaMax = mesh.bounds.max;
                }
                else
                {
                    Debug.LogWarning("Floor prefab 沒有 BoxCollider 或 MeshRenderer，請檢查！");
                    areaMin = new Vector3(-20, 0, -20);
                    areaMax = new Vector3(20, 0, 20);
                }
            }
        }
        else
        {
            areaMin = new Vector3(-20, 0, -20);
            areaMax = new Vector3(20, 0, 20);
        }
    }

    protected override void Update()
    {
        if (isStopped)
        {
            if (anim != null)
                anim.SetFloat("moveSpeed", 0f);
            return;
        }

        RandomMove();

        if (Input.GetKeyDown(teleportTestKey))
        {
            TeleportRandomly();
        }
    }

    private void RandomMove()
    {
        // 如果目標點已經超出地圖範圍，立即重選
        if (targetPosition.x < areaMin.x || targetPosition.x > areaMax.x ||
            targetPosition.z < areaMin.z || targetPosition.z > areaMax.z)
        {
            SetRandomTargetPosition();
        }

        if (!isMoving)
        {
            SetRandomTargetPosition();
        }

        float speed = moveSpeed;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (anim != null)
        {
            float currentSpeed = isMoving ? speed : 0f;
            anim.SetFloat("moveSpeed", currentSpeed);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isMoving = false;
            if (anim != null)
                anim.SetFloat("moveSpeed", 0f);
        }
    }

    private void SetRandomTargetPosition()
    {
        // 隨機距離
        float randomDistance = Random.Range(minMoveDistance, maxMoveDistance);
        // 隨機方向（單位圓上的隨機點，y=0，確保在XZ平面）
        float angle = Random.Range(0f, 360f);
        Vector3 randomDirection = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
        Vector3 candidate = transform.position + randomDirection * randomDistance;
        candidate.x = Mathf.Clamp(candidate.x, areaMin.x, areaMax.x);
        candidate.z = Mathf.Clamp(candidate.z, areaMin.z, areaMax.z);
        targetPosition = candidate;
        isMoving = true;
    }

    private IEnumerator TeleportRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(minTeleportInterval, maxTeleportInterval);
            yield return new WaitForSeconds(waitTime);
            TeleportRandomly();
        }
    }

    private void TeleportRandomly()
    {
        Vector3 randomOffset = Random.insideUnitSphere * teleportRange;
        randomOffset.y = 0;
        Vector3 candidate = transform.position + randomOffset;
        candidate.x = Mathf.Clamp(candidate.x, areaMin.x, areaMax.x);
        candidate.z = Mathf.Clamp(candidate.z, areaMin.z, areaMax.z);
        candidate.y = transform.position.y; // 明確保持 Y 軸不變
        transform.position = candidate;
        isMoving = false;
        Debug.Log("MagicElite teleported!");

        if (anim != null)
        {
            anim.SetBool("isAppear", false);
            StartCoroutine(ResetIsAppearAfterDelay());
        }
    }

    private IEnumerator ResetIsAppearAfterDelay()
    {
        yield return new WaitForSeconds(appearAnimDuration);
        if (anim != null)
            anim.SetBool("isAppear", true);
    }

    // 移除 new 關鍵字
    public void Stop()
    {
        isStopped = true;
        if (anim != null)
            anim.SetFloat("moveSpeed", 0f);
    }

    public void Move()
    {
        isStopped = false;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
        if (collision.gameObject.CompareTag("Wall"))
        {
            // 將位置限制在地板範圍內
            Vector3 clamped = transform.position;
            clamped.x = Mathf.Clamp(clamped.x, areaMin.x, areaMax.x);
            clamped.z = Mathf.Clamp(clamped.z, areaMin.z, areaMax.z);
            transform.position = clamped;

            // 重新選擇一個新的隨機目標點
            SetRandomTargetPosition();

            Debug.Log("Collided with wall, position clamped and direction changed.");
        }
    }
}
