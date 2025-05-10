using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class box : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Vector3 spawnPosition = new Vector3(-20, 1, -4); // 初始生成位置

    void Start()
    {
        // 設置箱子的初始位置
        transform.position = spawnPosition;
    }
}
