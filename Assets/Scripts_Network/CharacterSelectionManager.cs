using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class CharacterSelectionManager : NetworkBehaviour
{
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private Button[] characterButtons;
    [SerializeField] private GameObject selectionUI;
    [SerializeField] private TextMeshProUGUI statusText;

    // Track selected characters across all players
    public static readonly HashSet<int> selectedCharacters = new HashSet<int>();

    [SyncVar(hook = nameof(OnCharacterChanged))]
    private int currentCharacterIndex = -1;  // -1 means no selection

    [SyncVar]
    private bool isReady = false;

    private GameObject spawnedCharacter;

    void Start()
    {
        if (isLocalPlayer)
        {
            selectionUI.SetActive(true);
            SetupButtons();
            UpdateButtonStates();
        }
        else
        {
            selectionUI.SetActive(false);
        }
    }

    void SetupButtons()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;  // Capture the index for the lambda
            characterButtons[i].onClick.AddListener(() => OnCharacterButtonClicked(index));

            // Add character name or image to button here if needed
        }
    }

    void UpdateButtonStates()
    {
        for (int i = 0; i < characterButtons.Length; i++)
        {
            characterButtons[i].interactable = !selectedCharacters.Contains(i);
        }
    }

    void OnCharacterButtonClicked(int index)
    {
        if (!selectedCharacters.Contains(index))
        {
            CmdSelectCharacter(index);
        }
    }

    [Command]
    void CmdSelectCharacter(int index)
    {
        // If player already had a character selected, remove it from selected set
        if (currentCharacterIndex != -1)
        {
            selectedCharacters.Remove(currentCharacterIndex);
        }

        // Add new selection if valid
        if (!selectedCharacters.Contains(index))
        {
            currentCharacterIndex = index;
            selectedCharacters.Add(index);
            RpcUpdateSelection();
        }
    }

    [ClientRpc]
    void RpcUpdateSelection()
    {
        // Update UI for all clients
        if (isLocalPlayer)
        {
            UpdateButtonStates();
        }
    }

    void OnCharacterChanged(int oldIndex, int newIndex)
    {
        if (spawnedCharacter != null)
        {
            Destroy(spawnedCharacter);
        }

        if (newIndex != -1)
        {
            SpawnPreviewCharacter(newIndex);
        }
    }

    void SpawnPreviewCharacter(int index)
    {
        // Spawn character for preview
        Vector3 previewPosition = transform.position;
        spawnedCharacter = Instantiate(characterPrefabs[index], previewPosition, Quaternion.identity);

        // Disable movement during preview
        var movement = spawnedCharacter.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.enabled = false;
        }
    }

    [Command]
    public void CmdSetReady()
    {
        isReady = true;
        CheckAllPlayersReady();
    }

    void CheckAllPlayersReady()
    {
        if (!isServer) return;

        // Check if all connected players are ready and have different characters
        var players = NetworkServer.connections;
        bool allReady = true;
        HashSet<int> uniqueCharacters = new HashSet<int>();

        foreach (var player in players.Values)
        {
            var selector = player.identity.GetComponent<CharacterSelectionManager>();
            if (selector != null)
            {
                if (!selector.isReady || selector.currentCharacterIndex == -1)
                {
                    allReady = false;
                    break;
                }
                uniqueCharacters.Add(selector.currentCharacterIndex);
            }
        }

        // If all players are ready and have unique characters, start the game
        if (allReady && uniqueCharacters.Count == players.Count)
        {
            RpcStartGame();
        }
    }

    [ClientRpc]
    void RpcStartGame()
    {
        if (isLocalPlayer)
        {
            // Enable movement for selected character
            if (spawnedCharacter != null)
            {
                var movement = spawnedCharacter.GetComponent<PlayerMovement>();
                if (movement != null)
                {
                    movement.enabled = true;
                }
            }

            // Hide selection UI
            selectionUI.SetActive(false);
        }
    }
}