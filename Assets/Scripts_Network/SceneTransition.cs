using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneTransition : NetworkBehaviour
{
    private NetworkManagerExtended myNetworkManager;


    [Scene]
    public string transistionToSceneName;
    public string scenePosToSpawnOn;




    private void Awake()
    {
        if(myNetworkManager == null)
        {
            myNetworkManager = FindObjectOfType<NetworkManagerExtended>();
            
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.GetComponent<PlayerMovement>())
        {
            if (collision.TryGetComponent<PlayerMovement>(out PlayerMovement playerMoveScript))
            {
                playerMoveScript.enabled = false;
                Debug.Log(" movement false ");
            }
            if (isServer)
            {
                Debug.Log(" moving ");
                StartCoroutine(SendPlayerToNewScene(collision.gameObject));
            }
        }
           

    }
    

    [ServerCallback]

    IEnumerator SendPlayerToNewScene(GameObject player)
    {

        if (player.TryGetComponent<NetworkIdentity>(out NetworkIdentity identity))
        {
            NetworkConnectionToClient conn = identity.connectionToClient;
            if (conn == null) yield break;

            conn.Send(new SceneMessage { sceneName = this.gameObject.scene.path, sceneOperation = SceneOperation.UnloadAdditive, customHandling = true });

            // yield return new WaitForSeconds - fade

            NetworkServer.RemovePlayerForConnection(conn, false);

            NetworkStartPosition[] allStartPos = FindObjectsOfType<NetworkStartPosition>();

            Transform start = myNetworkManager.GetStartPosition();
            foreach (var item in allStartPos)
            {
                if (item.gameObject.scene.name == Path.GetFileNameWithoutExtension(transistionToSceneName) && item.name == scenePosToSpawnOn)
                {
                    start = item.transform;
                }
            }

            player.transform.position = start.position;

            SceneManager.MoveGameObjectToScene(player, SceneManager.GetSceneByPath(transistionToSceneName));

            conn.Send(new SceneMessage { sceneName = transistionToSceneName, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

            NetworkServer.AddPlayerForConnection(conn, player);

            if (NetworkClient.localPlayer != null && NetworkClient.localPlayer.TryGetComponent<PlayerMovement>(out PlayerMovement playerMoveScript))
            {
                playerMoveScript.enabled = true;
            }

        }
    }
}



