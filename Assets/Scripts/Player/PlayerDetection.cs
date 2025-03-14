using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    [Header(" Coliiders ")]
    [SerializeField] private Collider playerCollider;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent(out Coins coins))
        {
            if (!collider.bounds.Intersects(playerCollider.bounds))
                return;

            Debug.Log("Collected : " + coins.name);
            Destroy(coins.gameObject);
        }
    }

}
