using Mirror;
using UnityEngine;

public class EnemyMovement1 : EnemyMovement
{
    [Header(" Elements ")]
    private Player player;

    protected override void Update()
    {
        base.Update();
    }

    protected override void DifferentMovement()
    {
        base.DifferentMovement();
        if (player != null) 
        {
            FollowPlayer();
        }
    }

    [Server]
    public void StorePlayer(Player player)
    {
        this.player = player;
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
