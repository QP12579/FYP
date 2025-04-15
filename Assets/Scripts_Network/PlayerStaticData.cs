using System.Collections.Generic;
using UnityEngine;

// Static class to store player selections between scenes
public static class PlayerSelectionData
{
    // Dictionary to store player ID -> character selection
    private static Dictionary<int, int> playerSelections = new Dictionary<int, int>();

    // Store a player's character selection
    public static void StoreSelection(int playerId, int characterIndex)
    {
        playerSelections[playerId] = characterIndex;
    }

    // Get a player's character selection
    public static int GetSelection(int playerId)
    {
        if (playerSelections.TryGetValue(playerId, out int index))
            return index;

        // Default to -1 if no selection found
        return -1;
    }

    // Clear all stored selections
    public static void ClearSelections()
    {
        playerSelections.Clear();
    }

    // Get all player IDs who have made selections
    public static List<int> GetAllPlayerIds()
    {
        List<int> playerIds = new List<int>();
        foreach (var playerId in playerSelections.Keys)
        {
            playerIds.Add(playerId);
        }
        return playerIds;
    }
}