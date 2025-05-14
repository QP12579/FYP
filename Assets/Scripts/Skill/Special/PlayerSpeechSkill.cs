using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Mirror;

public class PlayerSpeechSkill : NetworkBehaviour
{
    [Header("UI & Speech Settings")]
    public TMP_Text outputText;       
    public List<string> keywords;     

    [Header("VFX Settings")]
    public List<GameObject> vfxPrefabs;

    public Vector3 vfxSpawnOffset = new Vector3(0, 0, 0);

    private HashSet<string> generatedKeywords = new HashSet<string>();

    private GameObject otherPlayer;

    private void Awake()
    {

        if (outputText == null)
        {
            GameObject outputObj = GameObject.Find("Transcript");
            if (outputObj)
                outputText = outputObj.GetComponent<TMP_Text>();
            else
                Debug.LogWarning("OutputText object not found in the scene.");
        }
    }

    private void Start()
    {
        Debug.Log(" aiosjudhnsaijndf "); 
       
        FindOtherPlayer();
    }

   public void FindOtherPlayer()
    {
        
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Player identity = player.GetComponent<Player>();
            if (identity != null && !(identity.isLocalPlayer))
            {
                otherPlayer = player;
                Debug.Log($"Other player found: {otherPlayer.name}");
                break;
            }
        }

        if (otherPlayer == null)
        {
            Debug.LogWarning("No other player found.");
        }
    }
    public void CheckForKeyword()
    {
        FindOtherPlayer();
        if (PersistentUI.Instance.SpecialAttackSlider.value != 1)
        {
            Debug.Log("Special skill cannot be used because the special bar is not full.");
            return;
        }

        // Remove punctuation and trim spaces
        string speechword = RemovePunctuation(outputText.text.Trim());
        Debug.Log($"Current output: {speechword}");

        // Loop through the keywords list.
        for (int i = 0; i < keywords.Count; i++)
        { 
            if (speechword.Equals(keywords[i], System.StringComparison.OrdinalIgnoreCase) )
            {
                Debug.Log($"Keyword matched: {keywords[i]}! Generating VFX on the other player.");

                // Mark this keyword as used.
                generatedKeywords.Add(keywords[i]);

                // If we have a reference to the other player, spawn the effect at their position.
                if (otherPlayer != null)
                {
                    Vector3 spawnPosition = otherPlayer.transform.position + vfxSpawnOffset;
                    CmdSpawnSkillVFX(i, spawnPosition, otherPlayer.transform.rotation);
                    ResetSP();
                    Debug.Log(otherPlayer.transform.rotation);
                    Debug.Log(otherPlayer.name);
                }
                else
                {
                    Debug.LogWarning("Other player reference is null. Cannot spawn VFX.");
                }

                break;
            }
        }
    }

    private void ResetSP()
    {
        PersistentUI.Instance.SpecialAttackSlider.value = 0;
    }

    private string RemovePunctuation(string input)
    {
        return Regex.Replace(input, @"[^\w\s]", "").TrimEnd();
    }

    [Command(requiresAuthority = false)]
    private void CmdSpawnSkillVFX(int index, Vector3 targetPosition, Quaternion targetRotation)
    {
        
        // Instantiate the VFX prefab at the target player's position.
        GameObject effect = Instantiate(vfxPrefabs[index], targetPosition, targetRotation);

        // Make sure the effect is registered with Mirror.
        NetworkServer.Spawn(effect);

        // Optionally, clean it up after 5 seconds.
        Destroy(effect, 5f);

        // Optionally, broadcast an RPC for logging/notification.
        RpcOnSkillUsed(index, targetPosition);
    }

    [ClientRpc]
    private void RpcOnSkillUsed(int index, Vector3 targetPosition)
    {
        Debug.Log($"[RPC] Skill VFX #{index} spawned at {targetPosition}");
    }
}
