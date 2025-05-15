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

    [Header("Box Puzzle")]
    [SerializeField] private GameObject boxObject;
    [SerializeField] private Transform platformArea;
    [SerializeField] private float checkRadius = 1f;

    [SyncVar]
    [SerializeField] private bool isBoxInPosition = false;
    private bool areWavesCompleted = false;

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
    void Update()
    {
        if (isServer)
        {
            CheckBoxPosition();

            // Check both conditions for completion
            if (areWavesCompleted && isBoxInPosition && !isChestActive)
            {
                ActivateStageCompletion();
            }
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
    [Server]
    public void SetPlayer(Player player, bool isMagicPlayer)
    {
        targetPlayer = player;

        // Find the WaveManager in this stage
        if (waveManager != null)
        {
            Debug.Log($"Setting player {player.name} (NetID: {player.netId}) on WaveManager with stageId {waveManager.stageId}");
            waveManager.SetPlayer(player, isMagicPlayer);
        }
        else
        {
            Debug.LogError("No WaveManager found in this stage!");
        }
    }

    [Server]
    private void CheckBoxPosition()
    {
        if (boxObject == null || platformArea == null) return;

        float distance = Vector3.Distance(
            new Vector3(boxObject.transform.position.x, platformArea.position.y, boxObject.transform.position.z),
            platformArea.position);

        isBoxInPosition = distance <= checkRadius;
    }

    [Server]
    private void OnStageCompleted()
    {
        Debug.Log($"Waves completed on {gameObject.name}");
        areWavesCompleted = true;

        if (isBoxInPosition)
        {
            ActivateStageCompletion();
        }
    }
    [Server]
    private void ActivateStageCompletion()
    {
        Debug.Log($"Stage fully completed - waves done and box in position");

        isChestActive = true;
        isPortalActive = true;

        if (portal != null)
        {
            Portal portalComponent = portal.GetComponent<Portal>();
            if (portalComponent != null && targetPlayer != null)
            {
                //portalComponent.SetTargetPlayer(targetPlayer);
            }
        }

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

    private void OnDrawGizmosSelected()
    {
        if (platformArea != null)
        {
            // Draw platform detection area
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(platformArea.position, checkRadius);

            // Draw dot in center
            Gizmos.color = new Color(0, 1, 0, 0.3f); 
            Gizmos.DrawSphere(platformArea.position, 0.1f);

            // If we have a box object, draw a line 
            if (boxObject != null)
            {
                if (Application.isPlaying)
                {
                    Gizmos.color = isBoxInPosition ? Color.cyan : Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.yellow;
                }

                Vector3 boxPosition = boxObject.transform.position;
                Vector3 platformPosition = platformArea.position;

                // Set the box point at the same Y level as the platform for distance comparison
                Vector3 boxLeveledPosition = new Vector3(boxPosition.x, platformPosition.y, boxPosition.z);

                Gizmos.DrawLine(platformPosition, boxLeveledPosition);
                // Draw a small sphere at the box position 
                Gizmos.DrawWireSphere(boxLeveledPosition, 0.2f);
            }
        }
    }

}