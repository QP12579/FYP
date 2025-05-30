using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using Mirror;
using System.Security.Principal;
using System;
using static Cinemachine.DocumentationSortingAttribute;
using Unity.Mathematics;

public class PlayerSpeechSkill : NetworkBehaviour
{
    [Header("UI & Speech Settings")]
    public TMP_Text outputText;
    public List<string> keywords;

    private delegate void KeywordAction();

    // Create a dictionary to map keywords to actions.
    private Dictionary<string, KeywordAction> keywordActions;

    [Header("VFX Settings")]
    public List<GameObject> vfxPrefabs;

    public Vector3 vfxSpawnOffset = new Vector3(0, 0, 0);

    private HashSet<string> generatedKeywords = new HashSet<string>();

    private GameObject otherPlayer;
    private Player notLocalPlayer;
    private Player LocalPlayer;
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
        FindLocalPlayer();
        FindOtherPlayer();
        keywordActions = new Dictionary<string, KeywordAction>(StringComparer.OrdinalIgnoreCase)
    {
        { "Double", () => {
                if (notLocalPlayer != null)
                    FindOtherPlayer();

                 Vector3 spawnPosition = otherPlayer.transform.position + vfxSpawnOffset;

                     MakeItDouble();
                     CmdSpawnSkillVFX(1, spawnPosition, otherPlayer.transform.rotation);
                     ResetSP();
                     UIDoubleDamageStatus(true);
            }
        },

        { "burn", () => {
                if (otherPlayer != null)
                {
                    Vector3 spawnPosition = otherPlayer.transform.position + vfxSpawnOffset;
                    // Assume that index 0 is reserved for an explosion VFX or adjust accordingly.
                    CmdSpawnSkillVFX(0, spawnPosition, otherPlayer.transform.rotation);
                     UIFireStatus(true);
                    ResetSP();
                    Debug.Log("[Action] Explosion VFX spawned.");
                }
                else
                {
                    Debug.LogWarning("Other player reference is null. Cannot spawn explosion VFX.");
                }
              }
            },
            {
                "Enemy", () => {
            if (otherPlayer != null)
                FindOtherPlayer();
             Vector3 spawnPosition = otherPlayer.transform.position + vfxSpawnOffset;

                 CmdSpawnSkillVFX(2, spawnPosition, otherPlayer.transform.rotation);

                 ResetSP();

         }

            }
        };

    }
    public void FindOtherPlayer()
    {
        if (otherPlayer != null) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Player identity = player.GetComponent<Player>();
            if (identity != null && !(identity.isLocalPlayer))
            {
                otherPlayer = player;
                notLocalPlayer = identity;
                Debug.Log($"Other player found: {otherPlayer.name}");


                break;
            }
        }

        if (otherPlayer == null)
        {
            Debug.LogWarning("No other player found.");
        }
    }

    public void FindLocalPlayer()
    {

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            Player identity = player.GetComponent<Player>();
            if (identity != null && (identity.isLocalPlayer))

                LocalPlayer = identity;
            Debug.Log($"local player found: {otherPlayer.name}");
            break;
        }


    }
    public void CheckForKeyword()
    {
        FindOtherPlayer();
        if (PersistentUI.Instance.SpecialAttackSlider.value != 1)
        {
            Debug.Log("special bar is not full.");
            return;
        }

        string speechword = RemovePunctuation(outputText.text.Trim());
        Debug.Log($"Current output: {speechword}");

        // Loop through our keyword mapping.
        foreach (var pair in keywordActions)
        {
            if (speechword.Equals(pair.Key, StringComparison.OrdinalIgnoreCase))
            {
                Debug.Log($"Keyword matched: {pair.Key}! Executing its action.");
                // Mark the keyword as used.
                generatedKeywords.Add(pair.Key);
                // Execute the corresponding action.
                pair.Value.Invoke();
                break;
            }
        }
    }


    [Command(requiresAuthority = false)]
    private void MakeItDouble()
    {

        notLocalPlayer.DamageMultiplier = 1.5f;
        StartCoroutine(TemporaryDoubleDamage());
    }
    private IEnumerator TemporaryDoubleDamage()
    {

        float originalMultiplier = notLocalPlayer.DamageMultiplier;

        notLocalPlayer.DamageMultiplier = 1.5f;

        yield return new WaitForSeconds(7f);

        notLocalPlayer.DamageMultiplier = 1f;

    }

    private void ResetSP()
    {
        FindLocalPlayer();
        LocalPlayer.UseSP();

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

     
    }

   


    [TargetRpc]
    private void RPCUIDoubleDamageStatus(bool isActive)
    {
        // Find the UI manager on this client.
        OnRecieveUI onRecieveUI = FindObjectOfType<OnRecieveUI>();
        if (onRecieveUI != null)
        {
            if (isActive)
                onRecieveUI.ShowStatusMessage("Feel Double", 7f);

        }
    }

    [Command]
    private void UIDoubleDamageStatus(bool isActive)
    {
        // Find the UI manager on this client.
        OnRecieveUI onRecieveUI = FindObjectOfType<OnRecieveUI>();
        if (onRecieveUI != null)
        {
            if (isActive)
                onRecieveUI.ShowStatusMessage("Double Damage Applied!", 7f);

        }
    }

    [Command]
    private void UIFireStatus(bool isActive)
    {
        // Find the UI manager on this client.
        OnRecieveUI onRecieveUI = FindObjectOfType<OnRecieveUI>();
        if (onRecieveUI != null)
        {
            if (isActive)
                onRecieveUI.ShowStatusMessage("Opponent is burning LOL", 3f);

        }
    }
}