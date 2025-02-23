using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SawTooth : MonoBehaviour
{
    public float hurt = 5;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")){
            collision.gameObject.GetComponent<Player>().TakeDamage(hurt);
        }
    }
}
