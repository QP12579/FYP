using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameController : NetworkBehaviour
{
    // Singleton instance
    public static GameController Instance { get; private set; }

    // Track player completion times
    private Dictionary<bool, float> completionTimes = new Dictionary<bool, float>();

    // Game start time
    private float gameStartTime;

    // UI References
    [SerializeField] private GameObject winnerPanel;
    [SerializeField] private TMPro.TextMeshProUGUI winnerText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Record the game start time
        gameStartTime = Time.time;

        // Hide the winner panel
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(false);
        }
    }

    [Server]
    public void PlayerPathCompleted(bool isMagicPath)
    {
        float completionTime = Time.time - gameStartTime;

        // Record the completion time
        completionTimes[isMagicPath] = completionTime;

        // If this is the first player to finish, they win
        if (completionTimes.Count == 1)
        {
            // Announce the winner to all clients
            RpcAnnounceWinner(isMagicPath, completionTime);
        }
    }

    [ClientRpc]
    private void RpcAnnounceWinner(bool isMagicPathWon, float completionTime)
    {
        // Show the winner panel
        if (winnerPanel != null)
        {
            winnerPanel.SetActive(true);

            // Update the winner text
            if (winnerText != null)
            {
                string winner = isMagicPathWon ? "Magic Player" : "Techno Player";
                winnerText.text = $"{winner} wins!\nTime: {completionTime:F2} seconds";
            }
        }
    }
}