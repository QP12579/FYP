using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using NaughtyAttributes;

public class WaveManager : NetworkBehaviour
{
    // Event for when all waves are completed
    public delegate void WaveCompletionEvent();
    public event WaveCompletionEvent OnAllWavesCompleted;

    [Header(" Settings ")]
    [SerializeField] private float waveDuration;
    private float timer;
    private bool isTimerOn;
    private int currentWaveIndex;
    [SerializeField] private float defaultSpawnHeight = 0.59f;

    [Header(" Spawn Settings ")]
    [SerializeField] private Transform stageCenter; // Reference to the center of the stage
    [SerializeField] private float minSpawnRadius = 5f; // Minimum distance from center
    [SerializeField] private float maxSpawnRadius = 15f; // Maximum distance from center

    [Header("Identity")]
    [SyncVar]
    public string stageId;

    [Header(" Path Type ")]
    [SerializeField] private bool isMagicPath; // True for magic path, false for techno path

    [Header(" Wave ")]
    [SerializeField] private Wave[] waves;
    private List<float> localCounters = new List<float>();

    [Header(" Elements ")]
    private Player targetPlayer; // The specific player this wave manager targets

    // Track active enemies
    private List<GameObject> activeEnemies = new List<GameObject>();

    // Set the player reference
    public void SetPlayer(Player newPlayer, bool isPlayerMagic)
    {
        // Only set this player as the target if it matches our path type
        if (isPlayerMagic == isMagicPath)
        {
            targetPlayer = newPlayer;
            Debug.Log($"Set target player for {(isMagicPath ? "Magic" : "Techno")} path: {newPlayer.name}");

            // Start the first wave once we have our target player
            if (isServer)
            {
                StartWave(0);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Make sure we have our target player and waves are running
        if (targetPlayer == null || !isTimerOn)
            return;

        if (timer < waveDuration)
            ManageCurrentWave();
        else
            StartWaveTransition();
    }

    private void StartWave(int waveIndex)
    {
        Debug.Log($"Starting Wave {waveIndex} on {(isMagicPath ? "Magic" : "Techno")} path");

        localCounters.Clear();

        foreach (WaveSegment segment in waves[waveIndex].segments)
        {
            localCounters.Add(1);
        }

        timer = 0;
        isTimerOn = true;
    }

    private void ManageCurrentWave()
    {
        // Only server should spawn enemies
        if (!NetworkServer.active)
            return;

        Wave currentWave = waves[currentWaveIndex];

        for (int i = 0; i < currentWave.segments.Count; i++)
        {
            WaveSegment segment = currentWave.segments[i];

            float tStart = segment.tStartEnd.x / 100 * waveDuration;
            float tEnd = segment.tStartEnd.y / 100 * waveDuration;

            if (timer < tStart || timer > tEnd)
                continue;

            float timeSinceSegmentStart = timer - tStart;

            float spawnDelay = 1f / segment.spawnmFrequency;    // Delay = 1/frequency

            if (timeSinceSegmentStart / spawnDelay > localCounters[i])
            {
                // Get spawn position relative to our target player
                Vector3 spawnPosition = GetSpawnPosition();

                // Instantiate the enemy
                GameObject enemy = Instantiate(segment.prefab, spawnPosition, Quaternion.identity, transform);

                // IMPORTANT: Set the target player BEFORE spawning on the network
                Enemy enemyComponent = enemy.GetComponent<Enemy>();
                if (enemyComponent != null && targetPlayer != null)
                {
                    enemyComponent.SetTargetPlayer(targetPlayer);
                    Debug.Log($"Assigned enemy to target player: {targetPlayer.name} (NetID: {targetPlayer.netId})");
                }
                else
                {
                    Debug.LogError("Failed to set target player for enemy - component or player missing");
                }

                // Spawn on the network to make visible to all clients
                NetworkServer.Spawn(enemy);

                var parentSetter = enemy.GetComponent<Enemy>();
                if (parentSetter != null)
                {
                    uint parentNetId = transform.parent.GetComponentInParent<NetworkIdentity>().netId; // or the netId of your desired parent
                    parentSetter.RpcSetParent(parentNetId);
                }

                activeEnemies.Add(enemy);
                localCounters[i]++;

                Debug.Log($"Spawned enemy for {(isMagicPath ? "Magic" : "Techno")} path, wave {currentWaveIndex}, segment {i}");
                Debug.LogError("StageCentre is : " + stageCenter);
            }
        }

        timer += Time.deltaTime;

        // Remove destroyed enemies from the list
        CleanupDestroyedEnemies();

        // If wave time is over and all enemies are defeated, finish earlier
        if (timer >= waveDuration * 0.9f && activeEnemies.Count == 0)
        {
            StartWaveTransition();
        }
    }

    private void CleanupDestroyedEnemies()
    {
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    private void StartWaveTransition()
    {
        isTimerOn = false;

        // Wait for all remaining enemies to be destroyed
        if (activeEnemies.Count > 0)
        {
            StartCoroutine(WaitForAllEnemiesToBeDefeated());
            return;
        }

        AdvanceWave();
    }

    private IEnumerator WaitForAllEnemiesToBeDefeated()
    {
        // Check every 0.5 seconds if all enemies are defeated
        while (activeEnemies.Count > 0)
        {
            // Clean up destroyed enemies
            CleanupDestroyedEnemies();
            yield return new WaitForSeconds(0.5f);
        }

        AdvanceWave();
    }

    private void AdvanceWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log($"All waves completed for {(isMagicPath ? "Magic" : "Techno")} path");

            // Trigger the completion event
            OnAllWavesCompleted?.Invoke();

            return;
        }

        StartWave(currentWaveIndex);
    }

    private Vector3 GetSpawnPosition()
    {
        if (stageCenter == null)
        {
            Debug.LogError("Stage center is null in GetSpawnPosition!");

            return Vector3.zero;
        }

        // Get random direction on XZ plane (horizontal only)
        Vector2 randomCircle = Random.insideUnitCircle.normalized;

        // Randomize the distance from center between min and max radius
        float distance = Random.Range(minSpawnRadius, maxSpawnRadius);

        // Create vector from random circle
        Vector3 direction = new Vector3(randomCircle.x, 0, randomCircle.y);

        // Calculate position relative to stage center
        Vector3 targetPosition = stageCenter.position + direction * distance;

        // Set default height
        targetPosition.y = defaultSpawnHeight;

        // Raycast to find the actual ground height
        RaycastHit hit;
        if (Physics.Raycast(targetPosition + Vector3.up * 50f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground")))
        {
            // Place at the configured height above the ground
            targetPosition.y = hit.point.y + defaultSpawnHeight;
        }

        // Clamp position within game boundaries (only X and Z)
        targetPosition.x = Mathf.Clamp(targetPosition.x, stageCenter.position.x - maxSpawnRadius, stageCenter.position.x + maxSpawnRadius);
        targetPosition.z = Mathf.Clamp(targetPosition.z, stageCenter.position.z - maxSpawnRadius, stageCenter.position.z + maxSpawnRadius);

        return targetPosition;
    }
}

[System.Serializable]
public struct Wave
{
    public string name;
    public List<WaveSegment> segments;
}

[System.Serializable]
public struct WaveSegment
{
    [MinMaxSlider(0, 100)] public Vector2 tStartEnd;
    public float spawnmFrequency;
    public GameObject prefab;
}