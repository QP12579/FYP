using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using NaughtyAttributes;

public class StageManager : NetworkBehaviour
{
    [Header("Stage References")]
    [SerializeField] private GameObject[] magicStages;
    [SerializeField] private GameObject[] technoStages;

    [Header("Prefabs")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private GameObject portalPrefab;

    // Player stage trackers - key is the player's netId
    private Dictionary<uint, PlayerStageInfo> playerStages = new Dictionary<uint, PlayerStageInfo>();

    // Track game start time
    private float gameStartTime;

    // UI References
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMPro.TextMeshProUGUI winnerText;

    // Data class to track player progress
    private class PlayerStageInfo
    {
        public bool isMagicCharacter;
        public int currentStageIndex = 0;
        public GameObject currentStage;
    }
    [ServerCallback]
    private void Start()
    {
        gameStartTime = Time.time;

        if (winnerPanel != null)
            winnerPanel.SetActive(false);
    }

    // Called when a player spawns and is ready to start a stage
    [Server]
    public void RegisterPlayer(Player player, bool isMagicCharacter)
    {
        Debug.Log($"Registering player {player.netId} as {(isMagicCharacter ? "Magic" : "Techno")} character");
        // Create player info
        PlayerStageInfo info = new PlayerStageInfo
        {
            isMagicCharacter = isMagicCharacter,
            currentStageIndex = 0
        };

        // Register player
        playerStages[player.netId] = info;

        // Spawn first stage
        SpawnStageForPlayer(player);
    }

   
    [Server]
    private void SpawnStageForPlayer(Player player)
    {
        if (!playerStages.ContainsKey(player.netId))
        {
            Debug.LogError($"No stage info found for player {player.netId}! Did RegisterPlayer run?");
            return;
        }

        PlayerStageInfo info = playerStages[player.netId];

        // Get the stage array
        GameObject[] stageArray = info.isMagicCharacter ? magicStages : technoStages;

        Debug.Log($"SpawnStageForPlayer: Player {player.netId}, Magic: {info.isMagicCharacter}, Stage Index: {info.currentStageIndex}");

        // Check if the stage arrays are properly set up
        if (stageArray == null || stageArray.Length == 0)
        {
            Debug.LogError($"Stage array is null or empty for {(info.isMagicCharacter ? "Magic" : "Techno")} player!");
            return;
        }

        // Check if player has completed all stages
        if (info.currentStageIndex >= stageArray.Length)
        {
            Debug.Log($"Player {player.netId} has completed all stages");
            PlayerCompletedAllStages(player);
            return;
        }

        // check stage prefab valid
        GameObject stagePrefab = stageArray[info.currentStageIndex];
        if (stagePrefab == null)
        {
            Debug.LogError($"Stage prefab at index {info.currentStageIndex} is null for {(info.isMagicCharacter ? "Magic" : "Techno")} player!");
            return;
        }

        Debug.Log($"Instantiating stage {info.currentStageIndex} for player {player.netId}");

        GameObject stageInstance = Instantiate(stagePrefab);

     
        info.currentStage = stageInstance;

        // Find WaveManager 
        WaveManager waveManager = stageInstance.GetComponentInChildren<WaveManager>();
        if (waveManager != null)
        {
            Debug.Log($"Found WaveManager in stage {info.currentStageIndex} for player {player.netId}");

            // Set player reference
            waveManager.SetPlayer(player, info.isMagicCharacter);

            // Subscribe to wave completion event
            waveManager.OnAllWavesCompleted += () => HandleStageCompleted(player);
        }
        else
        {
            Debug.LogError($"No WaveManager found in stage {info.currentStageIndex} for player {player.netId}!");
        }

        // Check if NetworkIdentity is present
        NetworkIdentity netId = stageInstance.GetComponent<NetworkIdentity>();
        if (netId == null)
        {
            Debug.LogError($"No NetworkIdentity on stage prefab {stagePrefab.name}!");
            netId = stageInstance.AddComponent<NetworkIdentity>();
        }

        // Spawn on network
        Debug.Log($"Spawning stage {info.currentStageIndex} on network for player {player.netId}");
        NetworkServer.Spawn(stageInstance);

        // Notify clients
        RpcSetupStage(player.netId, info.currentStageIndex, stageInstance.GetComponent<NetworkIdentity>().netId);
    }

    // Called when a player completes a stage
    [Server]
    private void HandleStageCompleted(Player player)
    {
        Debug.Log($"HandleStageCompleted called for player {player.netId}");

        // Find appropriate spawn points 
        PlayerStageInfo info = playerStages[player.netId];

        // Look for spawn points
        Transform chestSpawnPoint = null;
        Transform portalSpawnPoint = null;

        //  Direct transform.Find at root level
        chestSpawnPoint = info.currentStage.transform.Find("ChestSpawnPoint");
        portalSpawnPoint = info.currentStage.transform.Find("PortalSpawnPoint");

        //  If not found, try case-insensitive search
        if (chestSpawnPoint == null || portalSpawnPoint == null)
        {
            Debug.Log("Trying case-insensitive search for spawn points");

            Transform[] allTransforms = info.currentStage.GetComponentsInChildren<Transform>(true); // true = include inactive

            foreach (Transform t in allTransforms)
            {
                if (chestSpawnPoint == null && t.name.ToLower() == "chestspawnpoint")
                    chestSpawnPoint = t;

                if (portalSpawnPoint == null && t.name.ToLower() == "portalspawnpoint")
                    portalSpawnPoint = t;

                // Break early if we found both
                if (chestSpawnPoint != null && portalSpawnPoint != null)
                    break;
            }
        }

        if (chestSpawnPoint == null || portalSpawnPoint == null)
        {
            Debug.Log("Trying deep search for spawn points");

            Transform[] allChildren = info.currentStage.GetComponentsInChildren<Transform>(true);

            foreach (Transform child in allChildren)
            {
                if (chestSpawnPoint == null && child.name == "ChestSpawnPoint")
                    chestSpawnPoint = child;

                if (portalSpawnPoint == null && child.name == "PortalSpawnPoint")
                    portalSpawnPoint = child;
            }
        }

        //  create spawn points if still not found
        if (chestSpawnPoint == null || portalSpawnPoint == null)
        {
            Debug.LogError($"Spawn points still not found after multiple search attempts! Creating emergency spawn points.");

            // Get player position 
            Vector3 playerPos = player.transform.position;
            Vector3 forward = player.transform.forward;

            // Create  spawn points
            if (chestSpawnPoint == null)
            {
                GameObject tempObj = new GameObject("ChestSpawnPoint");
                tempObj.transform.SetParent(info.currentStage.transform);
                tempObj.transform.position = playerPos + forward * 3f;
                chestSpawnPoint = tempObj.transform;
                Debug.Log($"Created emergency chest spawn point at {chestSpawnPoint.position}");
            }

            if (portalSpawnPoint == null)
            {
                GameObject tempObj = new GameObject("PortalSpawnPoint");
                tempObj.transform.SetParent(info.currentStage.transform);
                tempObj.transform.position = playerPos + forward * 5f;
                portalSpawnPoint = tempObj.transform;
                Debug.Log($"Created emergency portal spawn point at {portalSpawnPoint.position}");
            }
        }

        Debug.Log($"Spawning chest at {chestSpawnPoint.position} and portal at {portalSpawnPoint.position}");

        // Spawn chest
        GameObject chest = Instantiate(chestPrefab, chestSpawnPoint.position, Quaternion.identity);
        NetworkServer.Spawn(chest);

        // Spawn portal
        GameObject portal = Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);

        // Configure portal
        Portal portalTrigger = portal.GetComponent<Portal>();
        if (portalTrigger == null)
        {
            Debug.LogWarning("Portal prefab doesn't have a Portal component - adding one");
            portalTrigger = portal.AddComponent<Portal>();
        }

        // Set the player this portal is for
        portalTrigger.SetTargetPlayer(player);
        portalTrigger.OnPlayerEntered += () => AdvancePlayerStage(player);

        // Spawn portal on network
        NetworkServer.Spawn(portal);

        Debug.Log($"Chest and portal spawned for player {player.netId}");
    }
    // Advance a player to their next stage
    [Server]
    private void AdvancePlayerStage(Player player)
    {
        PlayerStageInfo info = playerStages[player.netId];

        // Unsubscribe from events
        WaveManager waveManager = info.currentStage.GetComponentInChildren<WaveManager>();
        if (waveManager != null)
        {
            waveManager.OnAllWavesCompleted -= () => HandleStageCompleted(player);
        }

        // Destroy the current stage
        NetworkServer.Destroy(info.currentStage);

        // Advance to next stage
        info.currentStageIndex++;

        // Spawn the next stage
        SpawnStageForPlayer(player);
    }

    // Called when a player completes all stages
    [Server]
    private void PlayerCompletedAllStages(Player player)
    {
        // Get player info
        PlayerStageInfo info = playerStages[player.netId];

        // Calculate completion time
        float completionTime = Time.time - gameStartTime;

        // Announce winner to all clients
        RpcAnnounceWinner(info.isMagicCharacter ? "Magic Player" : "Techno Player", completionTime);
    }

    [ClientRpc]
    private void RpcSetupStage(uint playerNetId, int stageIndex, uint stageNetId)
    {
        // Find the player
        Player player = null;
        foreach (Player p in FindObjectsOfType<Player>())
        {
            if (p.netId == playerNetId)
            {
                player = p;
                break;
            }
        }

        if (player == null) return;

        // Find the stage
        GameObject stage = null;
        foreach (NetworkIdentity netId in FindObjectsOfType<NetworkIdentity>())
        {
            if (netId.netId == stageNetId)
            {
                stage = netId.gameObject;
                break;
            }
        }

        if (stage == null) return;

        // If we're the local player, set camera target to this stage
        if (player.isLocalPlayer)
        {
            // You might want to handle camera setup here
            // For example:
            // CameraController.Instance.SetTarget(stage.transform);
        }
    }

    [ClientRpc]
    private void RpcAnnounceWinner(string winnerName, float completionTime)
    {
        // Show winner UI
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);

            if (winnerText != null)
            {
                winnerText.text = $"{winnerName} wins!\nTime: {completionTime:F2} seconds";
            }
        }
    }
}


