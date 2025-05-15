using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StageManager : NetworkBehaviour
{
    
    public static StageManager Instance { get; private set; }

    [Header("Magic Path Stages")]
    [SerializeField] private GameObject[] magicStages;

    [Header("Techno Path Stages")]
    [SerializeField] private GameObject[] technoStages;

    
    private Dictionary<uint, PlayerProgress> playerProgress = new Dictionary<uint, PlayerProgress>();

    
    private float gameStartTime;

   
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMPro.TextMeshProUGUI winnerText;


    private class PlayerProgress
    {
        public bool isMagicPlayer;
        public int currentStageIndex = 0;
    }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        
            gameStartTime = Time.time;

            if (isServer)
            {
                // Spawn all stages on network before initializing them
                SpawnAllStages();
            }

            // Initialize stages
            InitializeStages(magicStages);
            InitializeStages(technoStages);

            if (winnerPanel != null)
                winnerPanel.SetActive(false);
        
    }

    private void InitializeStages(GameObject[] stages)
    {
        for (int i = 0; i < stages.Length; i++)
        {
            // Only first stage is active
            stages[i].SetActive(i == 0);
        }
    }

    [Server]
    private void SpawnAllStages()
    {
        Debug.Log("Spawning all stages on network");

        // Spawn all Magic stages
        foreach (GameObject stage in magicStages)
        {
            if (!stage.activeSelf)
            {
                // Temporarily activate to spawn
                bool wasActive = stage.activeSelf;
                stage.SetActive(true);

                // Ensure it has NetworkIdentity
                NetworkIdentity netId = stage.GetComponent<NetworkIdentity>();
                if (netId == null)
                {
                    Debug.LogError($"Stage {stage.name} missing NetworkIdentity!");
                    netId = stage.AddComponent<NetworkIdentity>();
                }

                // Spawn on network
                NetworkServer.Spawn(stage);

                // Restore original state
                if (!wasActive)
                    stage.SetActive(false);
            }
            else
            {
                // Spawn if active
                NetworkServer.Spawn(stage);
            }
        }

        // Same for Techno stages
        foreach (GameObject stage in technoStages)
        {
            if (!stage.activeSelf)
            {
                bool wasActive = stage.activeSelf;
                stage.SetActive(true);

                NetworkIdentity netId = stage.GetComponent<NetworkIdentity>();
                if (netId == null)
                {
                    Debug.LogError($"Stage {stage.name} missing NetworkIdentity!");
                    netId = stage.AddComponent<NetworkIdentity>();
                }

                NetworkServer.Spawn(stage);

                if (!wasActive)
                    stage.SetActive(false);
            }
            else
            {
                NetworkServer.Spawn(stage);
            }
        }
    }

    // Called when a player is ready to start
    [Server]
    public void RegisterPlayer(Player player, bool isMagicPlayer)
    {
        Debug.Log($"Registering player {player.name} as {(isMagicPlayer ? "Magic" : "Techno")} player");

        // Create progress tracker
        PlayerProgress progress = new PlayerProgress
        {
            isMagicPlayer = isMagicPlayer,
            currentStageIndex = 0
        };

        // Store in dictionary
        playerProgress[player.netId] = progress;

        // Get the appropriate stage
        GameObject[] stages = isMagicPlayer ? magicStages : technoStages;
        if (stages.Length > 0)
        {
            GameObject currentStage = stages[0];

            // Get stage controller and set player
            StageController controller = currentStage.GetComponent<StageController>();
            if (controller != null)
            {
                controller.SetPlayer(player, isMagicPlayer);
            }
        }
    }

   
    [Server]
    public void OnStageCompleted(StageController controller, Player player)
    {
        Debug.Log($"Stage completion registered for player {player.name}");

        if (!playerProgress.TryGetValue(player.netId, out PlayerProgress progress))
        {
            Debug.LogError($"No progress found for player {player.name}");
            return;
        }

        // Get stage arrays
        GameObject[] stages = progress.isMagicPlayer ? magicStages : technoStages;

        // Check if there's a next stage
        int nextStageIndex = progress.currentStageIndex + 1;
        if (nextStageIndex < stages.Length)
        {
            Debug.Log($"Enabling next stage {nextStageIndex} for player {player.name}");

            // Get next stage
            GameObject nextStage = stages[nextStageIndex];

            // Set up portal destination from current stage to next stage
            Portal portalComponent = controller.GetComponentInChildren<Portal>(true);
            if (portalComponent != null)
            {
                // Find spawn point in next stage
                StageController nextController = nextStage.GetComponent<StageController>();
                if (nextController != null)
                {
                    Transform spawnPoint = nextController.GetPlayerSpawnPoint();
                    if (spawnPoint != null)
                    {
                        //portalComponent.destinationPoint = spawnPoint;
                    }
                }
            }

            // Enable next stage right away
            nextStage.SetActive(true);

            // Sync to clients
            RpcActivateStage(nextStage.GetComponent<NetworkIdentity>().netId);

            // Initialize next stage with player
            StageController nextStageController = nextStage.GetComponent<StageController>();
            if (nextStageController != null)
            {
                nextStageController.SetPlayer(player, progress.isMagicPlayer);
            }
        }
        else
        {
            Debug.Log($"Player {player.name} has completed all stages!");
        }
    }

    [ClientRpc]
    private void RpcActivateStage(uint netId)
    {
        Debug.Log($"RpcActivateStage called for netId {netId}");

        bool found = false;

        // First try to find the NetworkIdentity directly (it might already be spawned)
        foreach (NetworkIdentity identity in FindObjectsOfType<NetworkIdentity>(true)) // true = include inactive
        {
            if (identity.netId == netId)
            {
                identity.gameObject.SetActive(true);
                Debug.Log($"Activated stage: {identity.name} on client");
                found = true;
                break;
            }
        }

        if (!found)
        {
            Debug.LogWarning($"Could not find stage with netId {netId} - it may not be spawned on client");
        }
    }

    // Called when player enters a portal
    [Server]
    public void OnPortalEntered(Player player)
    {
        if (!playerProgress.TryGetValue(player.netId, out PlayerProgress progress))
            return;

        // Get stage arrays
        GameObject[] stages = progress.isMagicPlayer ? magicStages : technoStages;

        // Deactivate current stage
        GameObject currentStage = stages[progress.currentStageIndex];
        currentStage.SetActive(false);

        // Sync to clients
        RpcDeactivateStage(currentStage.GetComponent<NetworkIdentity>().netId);

        // Advance to next stage
        progress.currentStageIndex++;

        // Check if we've reached the end
        if (progress.currentStageIndex >= stages.Length)
        {
            // Player completed all stages
            PlayerCompletedAllStages(player);
        }
    }

    [ClientRpc]
    private void RpcDeactivateStage(uint netId)
    {
        Debug.Log($"RpcDeactivateStage called for netId {netId}");

        // Find and deactivate stage on clients
        foreach (NetworkIdentity identity in FindObjectsOfType<NetworkIdentity>())
        {
            if (identity.netId == netId)
            {
                identity.gameObject.SetActive(false);
                Debug.Log($"Deactivated stage: {identity.name} on client");
                break;
            }
        }
    }

    // Called when player completes all stages
    [Server]
    private void PlayerCompletedAllStages(Player player)
    {
        if (!playerProgress.TryGetValue(player.netId, out PlayerProgress progress))
            return;

        // Calculate completion time
        float completionTime = Time.time - gameStartTime;

        // Announce winner to all clients
        RpcShowWinner(progress.isMagicPlayer ? "Magic Player" : "Techno Player", completionTime);
    }

    [ClientRpc]
    private void RpcShowWinner(string playerType, float completionTime)
    {
        // Show winner UI
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);

            if (winnerText != null)
            {
                winnerText.text = $"{playerType} wins!\nTime: {completionTime:F2} seconds";
            }
        }
    }
}