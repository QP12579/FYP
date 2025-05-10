using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class laser : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float damageAmount = 10f; // 激光對玩家造成的傷害

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DealDamageToPlayer(other);
        }
    }

    private void DealDamageToPlayer(Collider playerCollider)
    {
        Player player = playerCollider.GetComponent<Player>();
        if (player != null)
        {
            player.TakeDamage(damageAmount);
            Debug.Log("Player took " + damageAmount + " damage from the laser.");
        }
    }
}
