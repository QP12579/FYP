using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizzinessBanana : MonoBehaviour
{
    [SerializeField] private float dizzinessTime = 5f;
    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.gameObject.GetComponentInChildren<PlayerMovement>();

        if (player != null)
        {
            player.Dizziness(dizzinessTime);
            Destroy(gameObject);
        }
    }
}
