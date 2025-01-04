using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using Cinemachine;
using System.Collections.Generic;
using System.Collections;

public class NetworkPlayerManager : NetworkBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private GameObject[] characterPrefabs;

    
    [Header("Character Spawn Points")]
    [SerializeField] private Vector3 magicSpawnPoint = Vector3.zero;  // 0, 0, 0
    [SerializeField] private Vector3 techSpawnPoint = new Vector3(0, 0, 85);  // 0, 0, 85


    // Use SyncVars to ensure consistent IDs
    [SyncVar]
    private int playerId = -1;

    [SyncVar(hook = nameof(OnSelectedCharacterChanged))]
    private int selectedCharacterIndex = -1;

    [SyncVar]
    private bool isReady = false;

    private GameObject spawnedCharacter;
    private static Dictionary<int, int> playerSelections = new Dictionary<int, int>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        playerId = connectionToClient.connectionId;
        Debug.Log($"Server assigned playerId: {playerId}");

        if (playerSelections.ContainsKey(playerId))
        {
            selectedCharacterIndex = playerSelections[playerId];
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"OnStartClient: IsLocalPlayer={isLocalPlayer}, SelectedIndex={selectedCharacterIndex}, PlayerId={playerId}");

        if (isLocalPlayer && playerSelections.ContainsKey(playerId))
        {
            CmdRestoreCharacterSelection(playerSelections[playerId]);
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameManager.Instance.battleSceneName)
        {
            if (isServer)
            {
                SpawnCharacterOnServer();
            }
            else if (isLocalPlayer)
            {
                CmdRequestSpawn();
            }
        }
    }

    [Command]
    private void CmdRequestSpawn()
    {
        SpawnCharacterOnServer();
    }

    [Command]
    private void CmdRestoreCharacterSelection(int index)
    {
        Debug.Log($"Restoring character selection to: {index} for player {playerId}");
        selectedCharacterIndex = index;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameManager.Instance.battleSceneName)
        {
            SpawnCharacterOnServer();
        }
    }

    private Vector3 GetSpawnPosition()
    {
        return selectedCharacterIndex == 0 ? magicSpawnPoint : techSpawnPoint;
    }

    private void SpawnCharacterOnServer()
    {
        if (!isServer) return;

        Debug.Log($"SpawnCharacterOnServer: SelectedIndex={selectedCharacterIndex}, PlayerId={playerId}");

        if (selectedCharacterIndex < 0 || selectedCharacterIndex >= characterPrefabs.Length)
        {
            Debug.LogError($"Invalid character index: {selectedCharacterIndex}");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        transform.position = spawnPos;

        GameObject characterPrefab = characterPrefabs[selectedCharacterIndex];
        GameObject character = Instantiate(characterPrefab, spawnPos, Quaternion.identity);

        NetworkServer.Spawn(character, connectionToClient);
        RpcSetupCharacter(character);
    }

    [ClientRpc]
    private void RpcSetupCharacter(GameObject character)
    {
        if (character == null) return;

        Debug.Log($"RpcSetupCharacter - IsLocalPlayer: {isLocalPlayer}, ConnectionId: {connectionToClient?.connectionId}, Character: {character.name}");

        if (spawnedCharacter != null)
        {
            Destroy(spawnedCharacter);
        }

        spawnedCharacter = character;
        spawnedCharacter.transform.SetParent(transform);

        var movement = spawnedCharacter.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = isLocalPlayer;
        }
        // Setup camera priority based on ownership
        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null)
        {
            vcam.Follow = spawnedCharacter.transform;
            // Set high priority if we own this character, low if we don't
            vcam.Priority = isOwned ? 10 : 0;
            Debug.Log($"Camera priority set to {vcam.Priority} for {(isOwned ? "owned" : "non-owned")} character");
        }
    }

    private IEnumerator SetupCameraForLocalPlayer()
    {
        // Wait a frame to ensure all components are properly initialized
        yield return new WaitForEndOfFrame();

        Debug.Log($"Setting up camera for local player. ConnectionId: {connectionToClient?.connectionId}");

        var vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam != null && spawnedCharacter != null)
        {
            // Find all PlayerReferences and debug log their connection IDs
            var allPlayerRefs = FindObjectsOfType<PlayerReference>();
            foreach (var playerRef in allPlayerRefs)
            {
                Debug.Log($"Found player with ConnectionId: {playerRef.connectionId}");
            }

            vcam.Follow = spawnedCharacter.transform;
            vcam.Priority = 10;
            Debug.Log($"Camera following character for ConnectionId: {connectionToClient?.connectionId}");
        }
        else
        {
            Debug.LogError($"Camera setup failed! Camera: {vcam != null}, Character: {spawnedCharacter != null}");
        }
    }

        public void SelectCharacter(int index)
    {
        if (!isLocalPlayer) return;

        if (playerId != -1)
        {
            playerSelections[playerId] = index;
            Debug.Log($"Player {playerId} selected character {index}");
            CmdSelectCharacter(index);
        }
    }

    [Command]
    void CmdSelectCharacter(int index)
    {
        // Validate the selection
        if (index != -1 && (index < 0 || index >= characterPrefabs.Length))
        {
            Debug.LogError($"Invalid character index: {index}");
            return;
        }

        // Check if anyone else has selected this character
        var players = FindObjectsOfType<NetworkPlayerManager>();
        foreach (var player in players)
        {
            if (player != this && player.GetSelectedCharacter() == index && index != -1)
            {
                Debug.Log($"Character {index} already selected by another player");
                return;
            }
        }

        selectedCharacterIndex = index;
        isReady = (index != -1);

        if (index == -1)
        {
            // Clear the selection from playerSelections
            if (playerSelections.ContainsKey(playerId))
            {
                playerSelections.Remove(playerId);
            }
        }
        else
        {
            playerSelections[playerId] = index;
        }

        Debug.Log($"Server updated character selection: Player {playerId} selected {index}");
        GameManager.Instance.UpdateSelectionState();
    }

    void OnSelectedCharacterChanged(int oldValue, int newValue)
    {
        Debug.Log($"Character selection changed from {oldValue} to {newValue} for player {playerId}");

        if (newValue == -1)
        {
            if (playerSelections.ContainsKey(playerId))
            {
                playerSelections.Remove(playerId);
            }
        }
        else if (playerId != -1)
        {
            playerSelections[playerId] = newValue;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUIState();
        }
    }

    public int GetSelectedCharacter()
    {
        return selectedCharacterIndex;
    }

    public void CancelSelection()
    {
        if (!isLocalPlayer) return;

        Debug.Log($"Player {playerId} canceling selection");
        CmdSelectCharacter(-1); // Use -1 to indicate no selection
    }

    public bool IsReady()
    {
        return isReady;
    }
}