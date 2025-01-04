using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReference : NetworkBehaviour
{
    [SyncVar]
    public int connectionId;

    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        if (identity != null && identity.connectionToClient != null)
        {
            connectionId = identity.connectionToClient.connectionId;
            Debug.Log($"Server set connectionId to: {connectionId}");
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"Client received connectionId: {connectionId}");
    }
}
