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
        "<color=yellow>Fire Tornado</color>: Generate a massive fire tornado in the arena to damage the opponent.",
        "<color=yellow>Double Damage</color>: Cause the opponent to take double damage from enemies and traps.",
        "<color=yellow>Guess Who I Am</color>: Allow a helper to obstruct the opponent's line of sight.",
        "<color=yellow>Set the Bomb</color>: Place bombs on obstacles in the opponent's arena; touching the obstacles will trigger explosive damage.",
        "<color=yellow>Give Up Treatment</color>: Prevent the opponent from recovering HP by any means for 2 minutes.",
        "<color=yellow>Being a Poor Person</color>: Prevent the opponent from acquiring any coins or items for 1 minute.",
        "<color=yellow>Donald Trump</color>: Force the opponent to pay double the price for certain items at their next encounter with a shop.",
        "<color=yellow>Enemies From The Sky</color>: Random enemies will drop from the sky above the opponent, potentially causing trouble.",
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
