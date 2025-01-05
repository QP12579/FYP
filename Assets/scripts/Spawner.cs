using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Reference to the enemy prefab
    public GameObject trapPrefab; // Reference to the trap prefab
    public int numberOfEnemies = 5; // Number of enemies to spawn
    public int numberOfTraps = 3; // Number of traps to place
    public Vector2 spawnAreaMin; // Minimum coordinates of the spawn area
    public Vector2 spawnAreaMax; // Maximum coordinates of the spawn area
    public float spawnAreaScale = 0.8f; // Scale factor for the spawn area

    // Start is called before the first frame update
    void Start()
    {
        SpawnEnemies();
        PlaceTraps();
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 spawnPosition = GetRandomPosition();
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    void PlaceTraps()
    {
        for (int i = 0; i < numberOfTraps; i++)
        {
            Vector3 trapPosition = GetRandomPosition();
            Instantiate(trapPrefab, trapPosition, Quaternion.identity);
        }
    }

    Vector3 GetRandomPosition()
    {
        float xRange = (spawnAreaMax.x - spawnAreaMin.x) * spawnAreaScale;
        float zRange = (spawnAreaMax.y - spawnAreaMin.y) * spawnAreaScale;
        float x = Random.Range(spawnAreaMin.x + xRange * 0.5f, spawnAreaMax.x - xRange * 0.5f);
        float y = 0; // Assuming the game is 2.5D, so y is fixed
        float z = Random.Range(spawnAreaMin.y + zRange * 0.5f, spawnAreaMax.y - zRange * 0.5f);
        return new Vector3(x, y, z);
    }
}

