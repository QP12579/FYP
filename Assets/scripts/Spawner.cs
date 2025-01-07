using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public List<GameObject> enemyPrefabs; // List of enemy prefabs
    public List<GameObject> trapPrefabs; // List of trap prefabs
    public int numberOfEnemies = 5; // Number of enemies to spawn
    public int numberOfTraps = 3; // Number of traps to place
    public float spawnAreaScale = 0.8f; // Scale factor for the spawn area

    private Vector2 spawnAreaMin; // Minimum coordinates of the spawn area
    private Vector2 spawnAreaMax; // Maximum coordinates of the spawn area

    // Start is called before the first frame update
    void Start()
    {
        DetectFloorSize();
        SpawnEnemies();
        PlaceTraps();
    }

    void DetectFloorSize()
    {
        // Assuming the floor has a collider
        Collider floorCollider = GameObject.FindGameObjectWithTag("Floor").GetComponent<Collider>();
        if (floorCollider != null)
        {
            Bounds bounds = floorCollider.bounds;
            Vector3 floorMin = bounds.min;
            Vector3 floorMax = bounds.max;

            // Calculate the spawn area based on the floor size and scale factor
            float xRange = (floorMax.x - floorMin.x) * spawnAreaScale;
            float zRange = (floorMax.z - floorMin.z) * spawnAreaScale;

            spawnAreaMin = new Vector2(floorMin.x + xRange * 0.5f, floorMin.z + zRange * 0.5f);
            spawnAreaMax = new Vector2(floorMax.x - xRange * 0.5f, floorMax.z - zRange * 0.5f);
        }
        else
        {
            Debug.LogError("Floor with tag 'Floor' not found or missing Collider component.");
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = GetRandomPosition(0.66f); // Set the y axis to 0.66 for enemies
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void PlaceTraps()
    {
        for (int i = 0; i < numberOfTraps; i++)
        {
            Vector3 trapPosition = GetRandomPosition(0f); // Set the y axis to 0 for traps
            GameObject trapPrefab = trapPrefabs[Random.Range(0, trapPrefabs.Count)];
            Instantiate(trapPrefab, trapPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomPosition(float y)
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float z = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        return new Vector3(x, y, z);
    }
}



