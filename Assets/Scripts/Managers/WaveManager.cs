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

    [Header(" Wave ")]
    [SerializeField] private Wave[] waves;
    private List<float> localCounters = new List<float>();  

    // Start is called before the first frame update
    void Start()
    {
        localCounters.Add(1);
    }

    // Update is called once per frame
    void Update()
    {
        if (timer < waveDuration)
            ManageCurrentWave();


        Debug.Log("Timer : " + timer);
    }

    private void ManageCurrentWave()
    {
        Wave currentWave = waves[0];

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
                Instantiate(segment.prefab, new Vector3(-10 , 1 , -8 ), Quaternion.identity );
                localCounters[i]++;
            }    
        }

        timer += Time.deltaTime;
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

