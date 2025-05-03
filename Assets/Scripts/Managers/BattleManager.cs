using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : NetworkBehaviour
{
    public static BattleManager Instance;


    private Dictionary<int, Dictionary<int, float>> playerLevelCompletionTimes = new Dictionary<int, Dictionary<int, float>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RecordLevelCompletion(int playerId, int levelId, float completionTime)
    {
        // Initialize player data if needed
        if (!playerLevelCompletionTimes.ContainsKey(playerId))
        {
            playerLevelCompletionTimes[playerId] = new Dictionary<int, float>();
        }

        // Store completion time
        playerLevelCompletionTimes[playerId][levelId] = completionTime;

        // Check if this was the first player to complete
        bool isFirst = true;
        foreach (var player in playerLevelCompletionTimes.Keys)
        {
            if (player != playerId &&
                playerLevelCompletionTimes[player].ContainsKey(levelId) &&
                playerLevelCompletionTimes[player][levelId] < completionTime)
            {
                isFirst = false;
                break;
            }
        }

        // Notify all clients about winner
        if (isFirst)
        {
            RpcAnnounceFirstCompletion(playerId, levelId);
        }
    }

    [ClientRpc]
    private void RpcAnnounceFirstCompletion(int playerId, int levelId)
    {
        Debug.Log($"Player {playerId} was the first to complete level {levelId}!");
        // Display UI notification, etc.
    }
}
