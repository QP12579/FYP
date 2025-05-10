using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class StageController : NetworkBehaviour
{
    [Header("Stage Objects")]
    [SerializeField] private GameObject chest; 
    [SerializeField] private GameObject portal; 
    [SerializeField] private Transform playerSpawnPoint; 

    [Header("References")]
    [SerializeField] private WaveManager waveManager;

    // SyncVars to synchronize activation state
    [SyncVar(hook = nameof(OnChestActiveChanged))]
    private bool isChestActive = false;

    [SyncVar(hook = nameof(OnPortalActiveChanged))]
    private bool isPortalActive = false;

    // For portal configuration
    private Player targetPlayer;

    void Start()
    {
        // Make sure chest and portal are initially disabled
        if (chest != null) chest.SetActive(false);
        if (portal != null) portal.SetActive(false);

        // Connect to wave manager event
        if (waveManager != null)
        {
            waveManager.OnAllWavesCompleted += OnStageCompleted;
        }
    }

    // Called when SyncVar changes
    void OnChestActiveChanged(bool oldValue, bool newValue)
    {
        if (chest != null)
        {
            chest.SetActive(newValue);
            Debug.Log($"Chest active state changed to {newValue} on {(isServer ? "server" : "client")}");
        }
    }

    void OnPortalActiveChanged(bool oldValue, bool newValue)
    {
        if (portal != null)
        {
            portal.SetActive(newValue);
            Debug.Log($"Portal active state changed to {newValue} on {(isServer ? "server" : "client")}");
        }
    }

    // Set player reference
    public void SetPlayer(Player player, bool isMagicPlayer)
    {
        targetPlayer = player;

        // Find the WaveManager in this stage
        if (waveManager != null)
        {
            Debug.Log($"Setting player {player.name} on WaveManager with stageId {waveManager.stageId}");
            waveManager.SetPlayer(player, isMagicPlayer);
        }
        else
        {
            Debug.LogError("No WaveManager found in this stage!");
        }
    }

    // Called when all waves are completed
    [Server] // This ensures it only runs on the server
    private void OnStageCompleted()
    {
        Debug.Log($"Stage completed on {(isServer ? "server" : "client")} for {gameObject.name}");

        // Activate chest and portal - this will sync to clients
        isChestActive = true;
        isPortalActive = true;

        // Configure portal
        if (portal != null)
        {
            Portal portalComponent = portal.GetComponent<Portal>();
            if (portalComponent != null && targetPlayer != null)
            {
                portalComponent.SetTargetPlayer(targetPlayer);
            }
        }

        // Notify the stage manager - this will also enable the next stage
        StageManager.Instance.OnStageCompleted(this, targetPlayer);
    }

    // Get spawn point for next stage
    public Transform GetPlayerSpawnPoint()
    {
        return playerSpawnPoint;
    }

    // Clean up when destroyed
    private void OnDestroy()
    {
        if (waveManager != null)
        {
            waveManager.OnAllWavesCompleted -= OnStageCompleted;
        }
    }
}