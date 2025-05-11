using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floortrap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject safePrefab; // 安全區域的預製件（綠色）
    [SerializeField] private GameObject dangerPrefab; // 危險區域的預製件（紅色）
    [SerializeField] private GameObject warningPrefab; // 警告區域的預製件（黃色）
    [SerializeField] private float damageAmount = 10f; // 危險區域對玩家造成的傷害
    [SerializeField] private float randomDuration = 10f; // 隨機階段持續時間
    [SerializeField] private float warningDuration = 2f; // 警告階段持續時間
    [SerializeField] private Vector3[] spawnPositions; // 預製件的四個固定位置

    private GameObject[] areas; // 用於存儲四個區域的實例
    private int nextSafeAreaIndex = -1; // 下一個安全區域的索引
    private float timer = 0f;
    private bool isWarningState = false; // 是否處於警告階段
    private Dictionary<Transform, Coroutine> activeDamageCoroutines = new Dictionary<Transform, Coroutine>(); // 用於追蹤玩家的傷害協程

    void Start()
    {
        // 初始化區域
        areas = new GameObject[spawnPositions.Length];
        for (int i = 0; i < areas.Length; i++)
        {
            // 默認生成警告區域
            areas[i] = Instantiate(warningPrefab, transform);
            areas[i].transform.localPosition = spawnPositions[i]; // 使用固定位置
        }

        // 初始化為警告階段
        SetWarningState();
        isWarningState = true;
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (isWarningState && timer >= warningDuration)
        {
            // 警告階段結束，進入隨機階段
            ApplyNextPhase();
            isWarningState = false;
            timer = 0f;
        }
        else if (!isWarningState && timer >= randomDuration)
        {
            // 隨機階段結束，進入警告階段
            SetWarningState();
            isWarningState = true;
            timer = 0f;
        }
    }

    private void ApplyNextPhase()
    {
        // 將警告階段的狀態應用為下一個階段
        for (int i = 0; i < areas.Length; i++)
        {
            if (areas[i] != null)
            {
                Destroy(areas[i]);
            }

            // 將下一階段的安全區域設置為綠色，其餘設置為紅色
            areas[i] = Instantiate(i == nextSafeAreaIndex ? safePrefab : dangerPrefab, transform);
            areas[i].transform.localPosition = spawnPositions[i]; // 使用固定位置
        }

        Debug.Log("Next phase applied. Safe area index: " + nextSafeAreaIndex);
    }

    private void SetWarningState()
    {
        // 隨機選擇下一個安全區域
        nextSafeAreaIndex = Random.Range(0, areas.Length);

        for (int i = 0; i < areas.Length; i++)
        {
            if (areas[i] != null)
            {
                Destroy(areas[i]);
            }

            // 在警告階段，安全區域顯示綠色，其他區域顯示黃色
            areas[i] = Instantiate(i == nextSafeAreaIndex ? safePrefab : warningPrefab, transform);
            areas[i].transform.localPosition = spawnPositions[i]; // 使用固定位置
        }

        Debug.Log("Warning state activated. Next safe area index: " + nextSafeAreaIndex);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trap area.");
            if (!activeDamageCoroutines.ContainsKey(other.transform))
            {
                //Debug.Log("Player entered the trap area.");
                Coroutine damageCoroutine = StartCoroutine(HandlePlayerInDangerArea(other.transform));
                activeDamageCoroutines.Add(other.transform, damageCoroutine);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeDamageCoroutines.ContainsKey(other.transform))
            {
                StopCoroutine(activeDamageCoroutines[other.transform]);
                activeDamageCoroutines.Remove(other.transform);
            }
        }
    }

    private IEnumerator HandlePlayerInDangerArea(Transform playerTransform)
    {
        // 等待 2 秒
        yield return new WaitForSeconds(2f);

        while (IsPlayerInDangerArea(playerTransform))
        {
            Debug.Log("Player is standing on a dangerous area! Taking damage.");
            Player player = playerTransform.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
            }

            // 每 0.5 秒造成一次傷害
            yield return new WaitForSeconds(0.5f);
        }
    }

    private bool IsPlayerInDangerArea(Transform playerTransform)
    {
        for (int i = 0; i < areas.Length; i++)
        {
            if (i != nextSafeAreaIndex && IsPlayerInArea(playerTransform, areas[i].transform))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPlayerInArea(Transform playerTransform, Transform areaTransform)
    {
        // 檢查玩家是否在區域內（可以根據具體需求調整判斷邏輯）
        return Vector3.Distance(playerTransform.position, areaTransform.position) < 1f;
    }
}
