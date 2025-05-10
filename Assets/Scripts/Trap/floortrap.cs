using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floortrap : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject safePrefab; // 安全區域的預製件
    [SerializeField] private GameObject dangerPrefab; // 危險區域的預製件
    [SerializeField] private GameObject warningPrefab; // 黃色警告區域的預製件
    [SerializeField] private float damageAmount = 10f; // 危險區域對玩家造成的傷害
    [SerializeField] private float randomDuration = 10f; // 隨機階段持續時間
    [SerializeField] private float warningDuration = 2f; // 警告階段持續時間
    [SerializeField] private Vector3[] spawnPositions; // 預製件的四個固定位置

    private GameObject[] areas; // 用於存儲四個區域的實例
    private int safeAreaIndex = -1; // 當前安全區域的索引
    private float timer = 0f;
    private bool isWarningState = false; // 是否處於警告階段

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
            RandomizeAreas();
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

    private void RandomizeAreas()
    {
        // 隨機選擇一個安全區域
        safeAreaIndex = Random.Range(0, areas.Length);

        for (int i = 0; i < areas.Length; i++)
        {
            // 切換區域的預製件
            if (areas[i] != null)
            {
                Destroy(areas[i]);
            }

            areas[i] = Instantiate(i == safeAreaIndex ? safePrefab : dangerPrefab, transform);
            areas[i].transform.localPosition = spawnPositions[i]; // 使用固定位置
        }

        Debug.Log("Areas randomized. Safe area index: " + safeAreaIndex);
    }

    private void SetWarningState()
    {
        for (int i = 0; i < areas.Length; i++)
        {
            // 切換所有區域為警告狀態
            if (areas[i] != null)
            {
                Destroy(areas[i]);
            }

            areas[i] = Instantiate(warningPrefab, transform);
            areas[i].transform.localPosition = spawnPositions[i]; // 使用固定位置
        }

        Debug.Log("Warning state activated.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckPlayerPosition(other.transform);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CheckPlayerPosition(other.transform);
        }
    }

    private void CheckPlayerPosition(Transform playerTransform)
    {
        for (int i = 0; i < areas.Length; i++)
        {
            // 檢查玩家是否在當前區域內
            if (IsPlayerInArea(playerTransform, areas[i].transform))
            {
                if (i != safeAreaIndex)
                {
                    Debug.Log("Player is standing on a dangerous area! Taking damage.");
                    // 在這裡對玩家造成傷害
                    Player player = playerTransform.GetComponent<Player>();
                    if (player != null)
                    {
                        player.TakeDamage(damageAmount);
                    }
                }
                else
                {
                    Debug.Log("Player is standing on a safe area.");
                }
                break;
            }
        }
    }

    private bool IsPlayerInArea(Transform playerTransform, Transform areaTransform)
    {
        // 檢查玩家是否在區域內（可以根據具體需求調整判斷邏輯）
        return Vector3.Distance(playerTransform.position, areaTransform.position) < 1f;
    }
}
