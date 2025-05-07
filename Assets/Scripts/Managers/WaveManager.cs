using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class WaveManager : MonoBehaviour
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

    [Header(" Wave ")]
    [SerializeField] private Wave[] waves;
    private List<float> localCounters = new List<float>();

    [Header(" Elements ")]
    private Player player; // Will be set at runtime

    // Track active enemies
    private List<GameObject> activeEnemies = new List<GameObject>();

    // Set the player reference
    public void SetPlayer(Player newPlayer)
    {
        player = newPlayer;

        // Start the first wave once player is set
        StartWave(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null || !isTimerOn)
            return;

        if (timer < waveDuration)
            ManageCurrentWave();
        else
            StartWaveTransition();
    }

    private void StartWave(int waveIndex)
    {
        Debug.Log(" Starting Wave " + waveIndex);

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
                GameObject enemy = Instantiate(segment.prefab, GetsSpawnPosition(), Quaternion.identity, transform);
                activeEnemies.Add(enemy);
                localCounters[i]++;
            }
        }

        timer += Time.deltaTime;

        // Remove destroyed enemies from the list
        activeEnemies.RemoveAll(enemy => enemy == null);

        // If wave time is over and all enemies are defeated, finish earlier
        if (timer >= waveDuration * 0.9f && activeEnemies.Count == 0)
        {
            StartWaveTransition();
        }
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
            activeEnemies.RemoveAll(enemy => enemy == null);
            yield return new WaitForSeconds(0.5f);
        }

        AdvanceWave();
    }

    private void AdvanceWave()
    {
        currentWaveIndex++;
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log(" Waves completed ");

            // Trigger the completion event
            OnAllWavesCompleted?.Invoke();

            return;
        }

        StartWave(currentWaveIndex);
    }

    private Vector3 GetsSpawnPosition()
    {
       
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 direction = new Vector3(randomCircle.x, 0, randomCircle.y);

        float distance = Random.Range(6f, 10f);
        Vector3 offset = direction * distance;

        Vector3 targetPosition = player.transform.position + offset;
        targetPosition.y = defaultSpawnHeight;

        RaycastHit hit;
        if (Physics.Raycast(targetPosition + Vector3.up * 50f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground")))
        {
            targetPosition.y = hit.point.y + defaultSpawnHeight;
        }
        else
        {
            targetPosition.y = player.transform.position.y + defaultSpawnHeight;
        }

        targetPosition.x = Mathf.Clamp(targetPosition.x, -18f, 18f);
        targetPosition.z = Mathf.Clamp(targetPosition.z, -18f, 18f);

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