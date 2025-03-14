using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using NaughtyAttributes;
 
public class WaveManager : MonoBehaviour
{
    [Header(" Settings ")]
    [SerializeField] private Wave[] wave;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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

