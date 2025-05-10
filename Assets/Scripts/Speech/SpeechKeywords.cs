using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class SpeechKeywords : MonoBehaviour
{
    public TMP_Text outputText;
    public List<string> keywords; 
    public List<GameObject> vfxPrefabs; 
    private HashSet<string> generatedKeywords = new HashSet<string>();
    private Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
    }

    public void CheckForKeyword()
    {
        string speechword = RemovePunctuation(outputText.text.Trim());
        Debug.Log($"Current output: {speechword}");

        for (int i = 0; i < keywords.Count; i++)
        {
            if (speechword.Equals(keywords[i], System.StringComparison.OrdinalIgnoreCase) && !generatedKeywords.Contains(keywords[i]))
            {
                Debug.Log($"Keyword matched: {keywords[i]}! Generating VFX.");
                GenerateVFX(i);
                generatedKeywords.Add(keywords[i]);
                break;
            }
        }
        Debug.Log("Checked for keyword");
    }

    private string RemovePunctuation(string input)
    {
        return Regex.Replace(input, @"[^\w\s]", "").TrimEnd();
    }

    private void GenerateVFX(int index)
    {
        GameObject clonedVFX = Instantiate(vfxPrefabs[index], Vector3.zero, Quaternion.identity);
        Destroy(clonedVFX, 5f);
        Debug.Log("VFX cloned");
        if(player == null)
        player = FindObjectOfType<Player>();
        if (player!=null)
        player.UseSP();
    }
}