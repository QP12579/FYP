using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class shippingpoint : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string boxTag = "Box"; // 箱子的標籤
    [SerializeField] private float requiredTime = 8f; // 需要接觸的時間
    [SerializeField] private Vector3 spawnPosition = new Vector3(-3.5f, 0.2f, -21.5f); // 初始生成位置

    private float timer = 0f; // 計時器
    private bool isBoxTouching = false; // 是否有箱子接觸

    void Start()
    {
        // 設置初始位置
        transform.position = spawnPosition;
    }

    void Update()
    {
        if (isBoxTouching)
        {
            timer += Time.deltaTime;

            if (timer >= requiredTime)
            {
                Debug.Log("Box has been at the shipping point for 8 seconds!");
                // 在這裡執行完成運輸的邏輯
                timer = 0f; // 重置計時器
                isBoxTouching = false; // 重置狀態
            }
        }
        else
        {
            timer = 0f; // 如果箱子不再接觸，重置計時器
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(boxTag))
        {
            Debug.Log("Box started touching the shipping point.");
            isBoxTouching = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(boxTag))
        {
            Debug.Log("Box left the shipping point.");
            isBoxTouching = false;
        }
    }
}
