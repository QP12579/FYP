using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class hurtcube : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 10f; // 每次傷害的數值
    [SerializeField] private float damageInterval = 0.5f; // 每次傷害的間隔時間
    [SerializeField] private float initialDelay = 2f; // 初始延遲時間

    private Dictionary<Collider, Coroutine> activeDamageCoroutines = new Dictionary<Collider, Coroutine>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!activeDamageCoroutines.ContainsKey(other))
            {
                Coroutine damageCoroutine = StartCoroutine(HandlePlayerDamage(other));
                activeDamageCoroutines.Add(other, damageCoroutine);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeDamageCoroutines.ContainsKey(other))
            {
                StopCoroutine(activeDamageCoroutines[other]);
                activeDamageCoroutines.Remove(other);
            }
        }
    }

    private IEnumerator HandlePlayerDamage(Collider playerTransform)
    {
        // 初始延遲
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            Player player = playerTransform.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damageAmount);
                Debug.Log($"Player took {damageAmount} damage from hurtcube. Remaining HP: {player.HP}");
            }

            // 等待下一次傷害
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
