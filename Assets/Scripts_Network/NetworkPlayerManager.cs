using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class NetworkPlayerManager : NetworkBehaviour
{
    // Use SyncVars to ensure consistent IDs
    [SyncVar]
    private int playerId = -1;

    [SyncVar(hook = nameof(OnSelectedCharacterChanged))]
    private int selectedCharacterIndex = -1;

    [SyncVar]
    private bool isReady = false;

    // Add this to track the player's spawned character
    [SyncVar]
    private uint playerCharacterNetId;

    private GameObject playerCharacter;

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
    }

    // Call this when a character is spawned for this player
    public void AssignCharacter(GameObject character)
    {
        playerCharacter = character;
        playerCharacterNetId = character.GetComponent<NetworkIdentity>().netId;
    }

    // Get reference to player's spawned character
    public GameObject GetPlayerCharacter()
    {
        if (playerCharacter != null) return playerCharacter;

        if (playerCharacterNetId != 0)
        {
            // Find the object by netId using NetworkClient
            playerCharacter = NetworkClient.spawned.ContainsKey(playerCharacterNetId)
                ? NetworkClient.spawned[playerCharacterNetId].gameObject
                : null;
            return playerCharacter;
        }

        return null;
    }

    [Command]
    private void CmdRestoreCharacterSelection(int index)
    {
        Debug.Log($"Restoring character selection to: {index} for player {playerId}");
        selectedCharacterIndex = index;
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
        if (index != -1 && (index < 0 || index >= 2)) // Assuming 2 characters: Magic and Tech
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

    public int GetPlayerId()
    {
        return playerId;
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