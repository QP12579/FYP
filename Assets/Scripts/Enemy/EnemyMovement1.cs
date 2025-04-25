using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class EnemyMovement1 : MonoBehaviour
{
    [Header(" Elements ")]
    private Player player;

    [Header("Settings")]
    [SerializeField] private float moveSpeed;

    //Debuff Part
    private bool isDizziness = false; 

    void Update()
    {
        if (player != null && !isDizziness)
            FollowPlayer();
    }

    public void StorePlayer(Player player)
    {
        this.player = player;
    }

    private void FollowPlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        Vector3 targetPosition = transform.position + direction * moveSpeed * Time.deltaTime;
        transform.position = targetPosition;
    }

    //Debuff
    public void LowerSpeedStart(float time, float lowSpeedPersentage)
    {
        float baseMoveS = moveSpeed;
        moveSpeed *= lowSpeedPersentage;
        LeanTween.delayedCall(time, ()=>GoToBaseSpeed(baseMoveS));
    }

    public void GoToBaseSpeed(float baseSpeed) 
    {
        moveSpeed = baseSpeed;
    }

    public void DizzinessStart(float time)
    {
        isDizziness = true;
        Invoke("DizzinessEnd", time);
    }

    public void DizzinessEnd()
    {
        isDizziness = false;
    }
}
