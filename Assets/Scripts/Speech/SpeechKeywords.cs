using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechKeywords : MonoBehaviour
{
    private SpeechRecognitionTest getspeech;

    public string speechwords;
    public string[] keywords;
    public GameObject vfxPrefab;

    // Start is called before the first frame update
    void Start()
    {
        getspeech = GetComponent<SpeechRecognitionTest>();
    }

    // Update is called once per frame
    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.K))
        {
            CheckKeywords();
        }
    }

    void CheckKeywords()
    {
        speechwords = getspeech.text.text;
        foreach (string keyword in keywords) {

            if (speechwords.Contains(keyword))
            {
                //Instantiate(vfxPrefab, Vector3.zero, Quaternion.identity);
                GameObject clonedvfx = Instantiate(vfxPrefab, Vector3.zero, Quaternion.identity);
                Destroy(clonedvfx, 5f);
                break;
            }
        }
    }
}
