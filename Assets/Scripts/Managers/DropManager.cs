using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropManager : MonoBehaviour
{

    [Header(" Elements ")]
    [SerializeField] private Coins coinPrefab;

    private void Awake()
    {
        Enemy.OnPassAway += EnemyPassAwayCallBack;
       
    }

    private void OnDestroy() 
    {
        Enemy.OnPassAway += EnemyPassAwayCallBack;
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void EnemyPassAwayCallBack(Vector3 enemyPosition)
    {
        Instantiate(coinPrefab , enemyPosition, Quaternion.identity , transform);

    }
}
