using Mirror;
using Mono.CecilX.Cil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Trophy : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
       
        Player player = other.GetComponent<Player>();

        if (player != null)
        {
           
            CmdTriggerWin(player.netId);
        }
        else
        {
            Debug.Log("Player component not found on " + other.name);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdTriggerWin(uint playerNetId)
    {
        RpcPlayerWin(playerNetId);
    }

    [ClientRpc]
    private void RpcPlayerWin(uint playerNetId)
    {
       
        if (NetworkClient.spawned.TryGetValue(playerNetId, out NetworkIdentity identity))
        {
            Player player = identity.GetComponent<Player>();

            if (player != null)
            {
                if (player.isMagic)
                {
                    player.persistentUI.magicPanel.SetActive(true);
                }
                else
                {
                    player.persistentUI.techPanel.SetActive(true);
                }
            }
        }
    }
}
 
