using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System.IO;


public class NetworkManagerExtended : NetworkManager

{

    public string firstSceneToLoad;

    private string[] scenesToLoad;
    private bool subscenesLoaded;

    private readonly List<Scene> subScenes = new List<Scene>();

    private bool isInTransition;
    private bool firstSceneLoaded;

    private void Start()
    {
        int startIndex = 3;
        int endIndex = 4;
        int sceneCount = endIndex - startIndex + 1;
        scenesToLoad = new string[sceneCount];

  
        for (int i = 0; i < sceneCount; i++)
        {
            scenesToLoad[i] = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(startIndex + i));

        }

    }


    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        if (sceneName == firstSceneToLoad)
        {
            StartCoroutine(ServerLoadSubScenes());
        }
    }

    public override void OnClientSceneChanged()
    {
        if (isInTransition == false)
        {
            base.OnClientSceneChanged();
        }
    }

    IEnumerator ServerLoadSubScenes()
    {
        foreach (var additiveScene in scenesToLoad)
        {
            yield return SceneManager.LoadSceneAsync(additiveScene, new LoadSceneParameters
            {
                loadSceneMode = LoadSceneMode.Additive,
                localPhysicsMode = LocalPhysicsMode.Physics3D
            });
        
            }
        subscenesLoaded = true;
        

        }


    public override void OnClientChangeScene(string sceneName, SceneOperation sceneOperation, bool customHandling)
    {
        if(sceneOperation == SceneOperation.UnloadAdditive)
            StartCoroutine(UnLoadAdditive(sceneName));

        if (sceneOperation == SceneOperation.LoadAdditive)
            StartCoroutine(LoadAdditive(sceneName));
    }

    IEnumerator LoadAdditive(string sceneName)
    {
        isInTransition = true;
       //fade
        if (mode == NetworkManagerMode.ClientOnly)
        {
            loadingSceneAsync = SceneManager.LoadSceneAsync(sceneName , LoadSceneMode.Additive);
            while (loadingSceneAsync != null && !loadingSceneAsync.isDone)
            {
                yield return null;
            }
        }

        NetworkClient.isLoadingScene = false;
        isInTransition = false;

        OnClientSceneChanged();

        if (firstSceneLoaded == false)
        {
            firstSceneLoaded = true;

            yield return new WaitForSeconds(0.5f);

        }

          // fade
    }

    IEnumerator UnLoadAdditive(string sceneName)
    {
        isInTransition = true;
        //fade

        if (mode == NetworkManagerMode.ClientOnly)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
            yield return Resources.UnloadUnusedAssets();
        }

        NetworkClient.isLoadingScene = false;
        isInTransition = false;

        OnClientSceneChanged();

     }

    public override void OnServerReady(NetworkConnectionToClient conn)
    {
        base.OnServerReady(conn);

        if (conn.identity == null)
            StartCoroutine(AddPlayerDelayed(conn));
    }

    IEnumerator AddPlayerDelayed(NetworkConnectionToClient conn)
     {
        while (subscenesLoaded ==false)
            yield return null;

        NetworkIdentity[] allObjectsWithNetworkIdentity = FindObjectsOfType<NetworkIdentity>();

        foreach(var item in allObjectsWithNetworkIdentity)
        {
            item.enabled = true;
        }
        firstSceneLoaded = false;

        conn.Send(new SceneMessage { sceneName = firstSceneToLoad, sceneOperation = SceneOperation.LoadAdditive, customHandling = true });

        Transform startPos = GetStartPosition();

        GameObject player = Instantiate(playerPrefab , startPos);
        player.transform.SetParent(null);

        yield return new WaitForEndOfFrame();

        SceneManager.MoveGameObjectToScene(player,SceneManager.GetSceneByName(firstSceneToLoad));

        NetworkServer.AddPlayerForConnection(conn, player);
        
     }
    
  }


