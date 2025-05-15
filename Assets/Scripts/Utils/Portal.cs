using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Portal : NetworkBehaviour
{
    [SyncVar]
    private uint targetPlayerNetId;

    public Transform destinationPoint;

    [Server]
    public void SetTargetPlayer(Player player)
    {
        if (player != null)
        {
            targetPlayerNetId = player.netId;
            Debug.Log($"Portal target set to player with netId {targetPlayerNetId}");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player.netId == targetPlayerNetId)
        {
            Debug.Log($"Target player entered portal");

            // Teleport player if destination is set
            if (destinationPoint != null)
            {
                // Teleport on server
                player.transform.position = destinationPoint.position;
                player.transform.rotation = destinationPoint.rotation;

                // Sync to clients
                RpcTeleportPlayer(player.netId, destinationPoint.position, destinationPoint.rotation);
            }

            // Notify stage manager that portal was used
            StageManager.Instance.OnPortalEntered(player);

            // Deactivate portal - this stage should already be inactive
            gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void RpcTeleportPlayer(uint playerNetId, Vector3 position, Quaternion rotation)
    {
        // Find the player with this netId
        foreach (Player p in FindObjectsOfType<Player>())
        {
            if (p.netId == playerNetId)
            {
                // Teleport the player on all clients
                p.transform.position = position;
                p.transform.rotation = rotation;
                break;
            }
        }
    }
}