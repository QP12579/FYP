using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class box : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float pushStrength = 5f; // 推動的力量大小

    private void OnCollisionEnter(Collision collision)
    {
        // 檢查是否與玩家碰撞
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player is pushing the box.");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        // 當玩家持續碰撞箱子時，施加推動力
        if (collision.gameObject.CompareTag("Player"))
        {
            Rigidbody boxRigidbody = GetComponent<Rigidbody>();
            Vector3 pushDirection = collision.contacts[0].point - transform.position; // 計算推動方向
            pushDirection = -pushDirection.normalized; // 反向推動
            boxRigidbody.AddForce(pushDirection * pushStrength, ForceMode.Force); // 施加推動力
        }
    }
}
