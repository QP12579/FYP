using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameplayManager : NetworkBehaviour
{
    [SerializeField]
    private GameObject[] characterPrefabs; // Character prefabs [0] = Magic, [1] = Tech

    public Transform[] playerSpawnPoints;
    // Dictionary to keep track of spawned player characters
    private Dictionary<int, GameObject> spawnedCharacters = new Dictionary<int, GameObject>();

    // Called when scene is loaded on the server
    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log("GameplayManager - OnStartServer called");

        // Give clients a moment to load the scene before spawning characters
        StartCoroutine(SpawnPlayersWithDelay());
    }

    private IEnumerator SpawnPlayersWithDelay()
    {
        // Small delay to ensure clients are ready
        yield return new WaitForSeconds(0.5f);

        // Spawn all player characters
        SpawnAllPlayerCharacters();
    }

    [Server]
    private void SpawnAllPlayerCharacters()
    {
        Debug.Log("Starting to spawn player characters");

        // Get all connected players
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn == null)
            {
                Debug.LogWarning("Found null connection");
                continue;
            }

            // Get player ID
            int playerId = conn.connectionId;

            // Get character selection from our static data
            int characterIndex = PlayerSelectionData.GetSelection(playerId);

            if (characterIndex < 0 || characterIndex >= characterPrefabs.Length)
            {
                Debug.LogError($"Invalid character index {characterIndex} for player {playerId}");
                continue;
            }

            // Find spawn position
            Transform spawnPoint = GetSpawnPoint(playerId);

            // Create the player's character
            GameObject character = Instantiate(
                characterPrefabs[characterIndex],
                spawnPoint.position,
                spawnPoint.rotation
            );

            // Store reference to spawned character
            spawnedCharacters[playerId] = character;

            Destroy(conn.identity.gameObject, 0.1f);
            NetworkServer.ReplacePlayerForConnection(conn, character, true);

            Debug.Log($"Spawned character {characterIndex} for player {playerId}");

            // Register the player with the stage manager
            Player playerComponent = character.GetComponent<Player>();
            if (playerComponent != null)
            {
                
                StageManager stageManager = FindObjectOfType<StageManager>();
                if (stageManager != null)
                {
                    // Determine if this is a magic character 
                    bool isMagicCharacter = characterIndex == 0; // Adjust this based on your character indices

                    // Register the player
                    stageManager.RegisterPlayer(playerComponent, isMagicCharacter);

                    Debug.Log($"Registered player {playerId} with stage manager. Magic character: {isMagicCharacter}");
                }
                else
                {
                    Debug.LogError("StageProgressionManager not found in scene!");
                }
            }
            else
            {
                Debug.LogError("Player component not found on character object!");
            }
        }
    }
    
    private Transform GetSpawnPoint(int playerId)
    {
        if (NetworkManager.startPositions.Count == 0)
        {
            Debug.LogError("No start positions assigned!");
            return transform;
        }

        // Retrieve character selection for the given player
        int characterIndex = PlayerSelectionData.GetSelection(playerId);

        // Define separate spawn points for each character type
        List<Transform> character1SpawnPoints = new List<Transform>();
        List<Transform> character2SpawnPoints = new List<Transform>();

        // Categorize start positions based on predefined setup
        foreach (Transform startPosition in NetworkManager.startPositions)
        {
            if (startPosition.CompareTag("Character1Spawn"))
            {
                character1SpawnPoints.Add(startPosition);
            }
            else if (startPosition.CompareTag("Character2Spawn"))
            {
                character2SpawnPoints.Add(startPosition);
            }
        }

        // Choose spawn point based on selected character
        if (characterIndex == 0 && character1SpawnPoints.Count > 0)
        {
            return character1SpawnPoints[Random.Range(0, character1SpawnPoints.Count)];
        }
        else if (characterIndex == 1 && character2SpawnPoints.Count > 0)
        {
            return character2SpawnPoints[Random.Range(0, character2SpawnPoints.Count)];
        }

        Debug.LogError("No valid spawn points for chosen character, using default spawn.");
        return transform;
    }


    // Helper method to find other players (for ability targeting)
    public GameObject GetOtherPlayerCharacter(int myPlayerId)
    {
        foreach (var entry in spawnedCharacters)
        {
            if (entry.Key != myPlayerId)
            {
                return entry.Value;
            }
        }
        return null;
    }

    // Helper method to find a player by ID (for ability targeting)
    public GameObject GetPlayerCharacterById(int playerId)
    {
        if (spawnedCharacters.TryGetValue(playerId, out GameObject character))
        {
            return character;
        }
        return null;
    }


}