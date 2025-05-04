using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapsDamage : MonoBehaviour
{
    public float damage = 5;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
}
