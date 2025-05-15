using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trophy : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        Player player = other.GetComponent<Player>();
        if (player != null && StageManager.Instance != null)
        {
            // Find if this player is magic or techno
            bool isMagicPlayer = false;

            // Look up in StageManager's player registry
            foreach (var playerEntry in FindObjectsOfType<Player>())
            {
                if (playerEntry == player)
                {
                    isMagicPlayer = playerEntry.GetComponent<Player>().isMagicPlayer;
                    break;
                }
            }

            // Show winner
            StageManager.Instance.OnTrophyCollected(isMagicPlayer);

            // Disable trophy
            gameObject.SetActive(false);
        }
    }
}
