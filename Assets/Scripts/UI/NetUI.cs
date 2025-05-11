using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetUI : NetworkBehaviour
{
    public GameObject uiPanel; // Reference to UI panel

    void Start()
    {
        if (!isLocalPlayer)
        {
            uiPanel.SetActive(false); // Hide UI for non-local players
        }
        else
        {
            uiPanel.SetActive(true); // Show UI only for local player
        }
    }
}
