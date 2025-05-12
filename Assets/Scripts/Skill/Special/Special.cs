using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Special : NetworkBehaviour
{
    [Header("Prefab and Spawn Points")]
    [SerializeField] private GameObject prefabToSpawn;  // This prefab must have a MovablePrefab script attached.
    [SerializeField] private Transform spawnPoint;      // p1: start position on spawn.
    [SerializeField] private Transform targetPoint;     // p2: destination for the spawned prefab.

    // This function is linked to your UI button.
    public void OnSpawnButtonClicked()
    {
        if (!isLocalPlayer)
        {
            Debug.Log("Only the local player can spawn objects.");
            return;
        }
       // CmdSpawnPrefab();
    }

    // [Command] methods run on the server.
    /*[Command]
    private void CmdSpawnPrefab()
    {
        // Instantiate the prefab at the spawn point (p1).
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        // If the prefab has a movement script, set its target location to p2.
        MovablePrefab mover = spawnedObject.GetComponent<MovablePrefab>();
        if (mover != null)
        {
            mover.InitializeMovement(targetPoint.position);
        }

        // Spawn the object on the network so all clients see it.
        NetworkServer.Spawn(spawnedObject);
    }*/

}
