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

            // Give control to the player
            NetworkServer.Spawn(character, conn);

            Debug.Log($"Spawned character {characterIndex} for player {playerId}");
        }
    }

    private Transform GetSpawnPoint(int playerId)
    {
        if (playerSpawnPoints == null || playerSpawnPoints.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return transform;
        }

        int spawnIndex = playerId % playerSpawnPoints.Length;
        return playerSpawnPoints[spawnIndex];
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