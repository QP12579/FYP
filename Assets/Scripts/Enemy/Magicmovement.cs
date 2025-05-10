using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magicmovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.5f;
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
    private Animator anim;

    private void Start()
    {
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

    private void Update()
    {
        if (isStopped)
        {
            if (anim != null)
                anim.SetFloat("movespeed", 0f);
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

    private void SetRandomTargetPosition()
    {
        float randomDistance = Random.Range(minMoveDistance, maxMoveDistance);
        Vector3 randomDirection = Random.insideUnitSphere;
        randomDirection.y = 0;
        Vector3 candidate = transform.position + randomDirection.normalized * randomDistance;
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
        transform.position = candidate;
        isMoving = false;
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

    public void Stop()
    {
        isStopped = true;
        if (anim != null)
            anim.SetFloat("movespeed", 0f);
    }

    public void Move()
    {
        isStopped = false;
    }
}
