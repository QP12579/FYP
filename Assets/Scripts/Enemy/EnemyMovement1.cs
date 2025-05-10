using Mirror;
using UnityEngine;

public class EnemyMovement1 : EnemyMovement
{

    protected override void DifferentMovement()
    {
        base.DifferentMovement();
        if (player != null) 
        {
            FollowPlayer();
            anim.SetFloat("moveSpeed", 1);
        }
        else
        {
            Debug.Log("NoFound Player, Cannot Follow.");
        }
    }

    [Server] 
    private void FollowPlayer()
    {
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        Vector3 targetPosition = transform.position + direction * syncedMoveSpeed * Time.deltaTime;
        transform.position = targetPosition;
    }
}
