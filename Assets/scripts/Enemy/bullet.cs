using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float speed = 10f; // Speed of the bullet
    public float damage = 10f; // Damage dealt by the bullet
    public float lifetime = 5f; // Lifetime of the bullet before it gets destroyed

    private float timeSinceSpawn;
    private Vector3 targetDirection;

    // Start is called before the first frame update
    void Start()
    {
        timeSinceSpawn = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the bullet forward
        transform.Translate(targetDirection * speed * Time.deltaTime, Space.World);

        // Update the time since the bullet was spawned
        timeSinceSpawn += Time.deltaTime;

        // Destroy the bullet after its lifetime expires
        if (timeSinceSpawn >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetTarget(Vector3 targetPosition)
    {
        targetDirection = (targetPosition - transform.position).normalized;
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
