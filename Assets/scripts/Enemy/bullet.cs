using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f; // Speed of the bullet
    public float damage = 10f; // Damage dealt by the bullet
    public float lifetime = 5f; // Lifetime of the bullet before it gets destroyed

    // Start is called before the first frame update
    void Start()
    {
        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Assuming the player has a script with a TakeDamage method
           // other.GetComponent<PlayerController>().TakeDamage(damage);
            Destroy(gameObject); // Destroy the bullet after it hits the player
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Destroy the bullet if it hits an obstacle
        }
    }
}
