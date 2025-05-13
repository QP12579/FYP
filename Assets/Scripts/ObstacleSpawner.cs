using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : NetworkBehaviour
{
    public GameObject[] obstaclePrefabs; 
    public Transform[] spawnPoints;
    public GameObject[] trapPrefabs;
    public Transform[] trapSpawnPoints;
    public int numberOfObstacles = 5;
    public int numberOfTraps = 3;

    public GameObject specificTrapPrefab;
    public int maxSpecificTrapCount = 3;
    
    void Start()
    {
        SpawnObstacles();
        SpawnTraps();
    }
    [Server]
    void SpawnObstacles()
    {
        List<Transform> availableSpawnPoints = new List<Transform>(spawnPoints);

        for (int i = 0; i < numberOfObstacles; i++)
        {
            if (availableSpawnPoints.Count == 0) break;

            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform spawnPoint = availableSpawnPoints[randomIndex];

            GameObject obstaclePrefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];

            Instantiate(obstaclePrefab, spawnPoint.position, spawnPoint.rotation);

            availableSpawnPoints.RemoveAt(randomIndex);

           

        }
    }
    [Server]
    void SpawnTraps()
    {
        List<Transform> availableTrapSpawnPoints = new List<Transform>(trapSpawnPoints);
        int specificTrapCount = 0;

        for (int i = 0; i < numberOfTraps; i++)
        {
            if (availableTrapSpawnPoints.Count == 0) break;

            GameObject trapPrefab;
            if (specificTrapCount < maxSpecificTrapCount && Random.value < 0.5f) 
            {
                trapPrefab = specificTrapPrefab;
                specificTrapCount++;
            }
            else
            {
                trapPrefab = trapPrefabs[Random.Range(0, trapPrefabs.Length)];
            }

            int randomIndex = Random.Range(0, availableTrapSpawnPoints.Count);
            Transform trapSpawnPoint = availableTrapSpawnPoints[randomIndex];

            Instantiate(trapPrefab, trapSpawnPoint.position, trapSpawnPoint.rotation);
            availableTrapSpawnPoints.RemoveAt(randomIndex);
        }
    }


}
