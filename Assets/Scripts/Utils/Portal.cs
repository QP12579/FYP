using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    private uint targetPlayerNetId;

    public delegate void PortalEvent();
    public event PortalEvent OnPlayerEntered;

    public void SetTargetPlayer(Player player)
    {
        targetPlayerNetId = player.netId;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
      
        Player player = other.GetComponent<Player>();

        if (player != null && player.netId == targetPlayerNetId)
        {
           
            OnPlayerEntered?.Invoke();

           
            NetworkServer.Destroy(gameObject);
        }
    }
}