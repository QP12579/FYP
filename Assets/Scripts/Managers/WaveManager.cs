using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using NaughtyAttributes;
 
public class WaveManager : MonoBehaviour
{
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
    [SerializeField] private Player player;

    // Start is called before the first frame update
    void Start()
    {
        

        StartWave(currentWaveIndex);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTimerOn)
            return;

        if (timer < waveDuration)
            ManageCurrentWave();
        else
            StartWaveTransition();


        Debug.Log("Timer : " + timer);
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

            float   tStart      = segment.tStartEnd.x / 100 * waveDuration;
            float   tEnd        = segment.tStartEnd.y / 100 * waveDuration;

            if (timer < tStart || timer > tEnd)
                continue;

            float timeSinceSegmentStart = timer - tStart;

            float spawnDelay = 1f / segment.spawnmFrequency;    // Delay =   1/ frequency

            if (timeSinceSegmentStart / spawnDelay > localCounters[i] )
            {
                Instantiate(segment.prefab, GetsSpawnPosition(), Quaternion.identity, transform );
                localCounters[i]++;
            }    
        }

        timer += Time.deltaTime;
    }

    private void StartWaveTransition()
    {
        isTimerOn = false;

       // DefeatAllEnemies();

        currentWaveIndex++;
        if (currentWaveIndex >= waves.Length)
        {
            Debug.Log(" Waves completed ");
            
            return; 
        }
        StartWave(currentWaveIndex);

    }

    private void DefeatAllEnemies()
    {
        transform.Clear();
    }

    private Vector3 GetsSpawnPosition()
    {
        // Get random direction on XZ plane (horizontal only)
        Vector2 randomCircle = Random.insideUnitCircle.normalized;
        Vector3 direction = new Vector3(randomCircle.x, 0, randomCircle.y);

        // Create offset from player in that direction (horizontal only)
        float distance = Random.Range(6f, 10f);
        Vector3 offset = direction * distance;

        // Calculate target position (on same XZ plane as player initially)
        Vector3 targetPosition = player.transform.position + offset;

        // Set default height
        targetPosition.y = defaultSpawnHeight;

        // Raycast to find the actual ground height
        RaycastHit hit;
        if (Physics.Raycast(targetPosition + Vector3.up * 50f, Vector3.down, out hit, 100f, LayerMask.GetMask("Ground")))
        {
            // Place at the configured height above the ground
            targetPosition.y = hit.point.y + defaultSpawnHeight;
        }
        else
        {
            // If no ground found, use player's Y position plus default height
            targetPosition.y = player.transform.position.y + defaultSpawnHeight;
        }

        // Clamp position within game boundaries (only X and Z)
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

