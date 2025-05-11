using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapsDamage : MonoBehaviour
{
    public float damage = 5f; // 每次傷害的數值
    public float damageInterval = 2f; // 每次傷害的間隔時間

    private Dictionary<Transform, Coroutine> activeDamageCoroutines = new Dictionary<Transform, Coroutine>();

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform playerTransform = collision.gameObject.transform;

            if (!activeDamageCoroutines.ContainsKey(playerTransform))
            {
                // 開始對玩家造成傷害的協程
                Coroutine damageCoroutine = StartCoroutine(DealDamageOverTime(playerTransform));
                activeDamageCoroutines.Add(playerTransform, damageCoroutine);
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Transform playerTransform = collision.gameObject.transform;

            if (activeDamageCoroutines.ContainsKey(playerTransform))
            {
                // 停止對玩家造成傷害的協程
                StopCoroutine(activeDamageCoroutines[playerTransform]);
                activeDamageCoroutines.Remove(playerTransform);
            }
        }
    }

    private IEnumerator DealDamageOverTime(Transform playerTransform)
    {
        while (true)
        {
            Player player = playerTransform.GetComponentInParent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log($"Player took {damage} damage from trap. Remaining HP: {player.HP}");
            }

            // 等待指定的間隔時間
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
