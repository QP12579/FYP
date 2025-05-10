using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BattleManager : MonoBehaviour
{

    public static BattleManager instance;

    private void Awake()
    {
        if
            (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        SetGameState(GameState.STARTING);
    }

    public void StartGame()          => SetGameState(GameState.GAME);
    public void StartShop()          => SetGameState(GameState.SHOP);

    
    void Update()
    {
        
    }

    public void SetGameState(GameState gameState)
    {
        IEnumerable<IGameStateListener> gameStateListeners = 
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IGameStateListener>();

        foreach(IGameStateListener gameStateListener in gameStateListeners)
            gameStateListener.GameStateChangedCallback(gameState);
    }

   


    public interface IGameStateListener
    {
        void GameStateChangedCallback(GameState gameState);
    }

    
}
