using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using Random = UnityEngine.Random;
using Mirror;

public class DropManager : NetworkBehaviour
{
    
    [Header(" Elements ")]
    [SerializeField] private Coins coinPrefab;

    private void Awake()
    {
        Enemy.OnPassAway += EnemyPassAwayCallBack;
       
    }

    private void OnDestroy() 
    {
        Enemy.OnPassAway -= EnemyPassAwayCallBack;
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Server]
    private void EnemyPassAwayCallBack(Vector3 enemyPosition)
    {
       Coins coinInstance =  Instantiate(coinPrefab , enemyPosition, Quaternion.identity);
        NetworkServer.Spawn(coinInstance.gameObject);
        coinInstance.gameObject.transform.localPosition = enemyPosition;

        coinInstance.name = "Coin " + Random.Range(0, 5000);
       

        
    }
}
