using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float damageAmount = 10f; // 激光對玩家造成的傷害
    [SerializeField] private float damageInterval = 2f; // 每次傷害的間隔時間

    private Dictionary<Transform, Coroutine> activeDamageCoroutines = new Dictionary<Transform, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!activeDamageCoroutines.ContainsKey(other.transform))
            {
                // 開始對玩家造成傷害的協程
                Coroutine damageCoroutine = StartCoroutine(DealDamageOverTime(other.transform));
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
                // 停止對玩家造成傷害的協程
                StopCoroutine(activeDamageCoroutines[other.transform]);
                activeDamageCoroutines.Remove(other.transform);
            }
        }
    }

    private IEnumerator DealDamageOverTime(Transform playerTransform)
    {
        while (true)
        {
            Player player = playerTransform.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
                Debug.Log("Player took " + damageAmount + " damage from the laser.");
            }

            // 等待指定的間隔時間
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
