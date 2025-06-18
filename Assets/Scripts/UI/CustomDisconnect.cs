using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CustomDisconnect : MonoBehaviour
{
    // Reference to the Disconnect UI button
    public Button disconnectButton;

    private void Awake()
    {
        // If not assigned in the Inspector, try to fetch the button component on this GameObject.
        if (disconnectButton == null)
            disconnectButton = GetComponent<Button>();

        if (disconnectButton != null)
            disconnectButton.onClick.AddListener(HandleDisconnect);
        else
            Debug.LogWarning("Disconnect Button component not found!");
    }

    private void HandleDisconnect()
    {
        Time.timeScale = 1f; // Ensure time scale is reset before disconnecting
        
        // Check if we're running as a host (server and client)
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            Debug.Log("Stopping host.");
            NetworkManager.singleton.StopHost();
        }
        // Otherwise, we are simply a client, so stop the client.
        else if (NetworkClient.isConnected)
        {
            Debug.Log("Stopping client.");
            NetworkManager.singleton.StopClient();
        }
        else
        {
            Debug.Log("Not connected to any network session.");
        }
    }
}
