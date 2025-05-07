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

    // Spawn a stage for a specific player
    [Server]
    private void SpawnStageForPlayer(Player player)
    {
        if (!playerStages.ContainsKey(player.netId))
        {
            Debug.LogError($"No stage info found for player {player.netId}! Did RegisterPlayer run?");
            return;
        }

        PlayerStageInfo info = playerStages[player.netId];

        // Get the appropriate stage array
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

        // Make sure the stage prefab is valid
        GameObject stagePrefab = stageArray[info.currentStageIndex];
        if (stagePrefab == null)
        {
            Debug.LogError($"Stage prefab at index {info.currentStageIndex} is null for {(info.isMagicCharacter ? "Magic" : "Techno")} player!");
            return;
        }

        Debug.Log($"Instantiating stage {info.currentStageIndex} for player {player.netId}");

        // Instantiate the stage
        GameObject stageInstance = Instantiate(stagePrefab);

        // Set as the current stage
        info.currentStage = stageInstance;

        // Find the WaveManager in the stage
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
        // Find appropriate spawn points in the stage
        PlayerStageInfo info = playerStages[player.netId];

        // Look for spawn points in the stage
        Transform chestSpawnPoint = info.currentStage.transform.Find("ChestSpawnPoint");
        Transform portalSpawnPoint = info.currentStage.transform.Find("PortalSpawnPoint");

        if (chestSpawnPoint == null || portalSpawnPoint == null)
        {
            Debug.LogError("Spawn points not found in stage!");
            return;
        }

        // Spawn chest
        GameObject chest = Instantiate(chestPrefab, chestSpawnPoint.position, Quaternion.identity);
        NetworkServer.Spawn(chest);

        // Spawn portal
        GameObject portal = Instantiate(portalPrefab, portalSpawnPoint.position, Quaternion.identity);

        // Configure portal
        Portal portalTrigger = portal.GetComponent<Portal>();
        if (portalTrigger == null)
        {
            portalTrigger = portal.AddComponent<Portal>();
        }

        // Set the player this portal is for
        portalTrigger.SetTargetPlayer(player);
        portalTrigger.OnPlayerEntered += () => AdvancePlayerStage(player);

        // Spawn portal on network
        NetworkServer.Spawn(portal);
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


