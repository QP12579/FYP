using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagingSkill : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        Player player = other.gameObject.GetComponent<Player>();

        if (player != null && !isDamaging)
        {
            StartCoroutine(DamageOverTime(player));
        }
    }

    private bool isDamaging = false;

    private IEnumerator DamageOverTime(Player player)
    {
        isDamaging = true;
        while (player != null)
        {
            player.TakeDamage(12);
            yield return new WaitForSeconds(1f); 
        }
        isDamaging = false;
    }
}
