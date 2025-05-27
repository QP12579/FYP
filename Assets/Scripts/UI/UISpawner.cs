using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using TMPro;
using UnityEngine.UI;

public class UISpawner : MonoBehaviour
{
    public GameObject prefab;
    public int numberOfPrefabs = 15;
    public Transform spawnParent;

    public Scrollbar SpecialAttackscrollbar;
    public float scrollSpeed = 0.1f;

    // KeywordTexts' content
    [SerializeField] string[] keywordTexts = {     
        "<color=yellow>Fire</color>: Generate a massive fire tornado in the arena to damage the opponent.",
        "<color=yellow>Double</color>: Cause the opponent to take double damage from enemies and traps.",
        "<color=yellow>Enemy</color>: Summon a strong elite enemy on your oponents head.",
        
    };

    void Start()
    {
        SpawnPrefabs();
    }

    void Update()
    {
        // get mouse scroll value
        float scrollDelta = Input.mouseScrollDelta.y;

        // update value if have scroll value
        if (scrollDelta != 0)
        {
            // update scrollbar value between 0 and 1
            SpecialAttackscrollbar.value += scrollDelta * scrollSpeed;
            SpecialAttackscrollbar.value = Mathf.Clamp01(SpecialAttackscrollbar.value);
        }
    }

    void SpawnPrefabs()
    {
        for (int i = 0; i < numberOfPrefabs; i++)
        {
            //spawn prefab
            GameObject instance = Instantiate(prefab, spawnParent);

            // update spwan name
            instance.name = "KW (" + (i + 1) + ")";

            // change TMP text
            TextMeshProUGUI numText = instance.transform.Find("Num").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI keywordText = instance.transform.Find("KeywordTexts").GetComponent<TextMeshProUGUI>();

            if (numText != null)
            {
                numText.text = "#" + (i + 1); // #1, #2, ...
            }

            if (keywordText != null && i < keywordTexts.Length)
            {
                keywordText.text = keywordTexts[i]; // Generate keywords in order
            }
        }

        // refresh Layout
        LayoutRebuilder.ForceRebuildLayoutImmediate(spawnParent.GetComponent<RectTransform>());

    }
    
}
