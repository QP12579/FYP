using UnityEngine;
using Mirror;

public class NetworkPlayerMovement : NetworkBehaviour
{
    private PlayerMovement moveScript;

    void Awake()
    {
        // Get the movement script reference
        moveScript = GetComponent<PlayerMovement>();
    }

    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        // Enable movement for local player
        if (moveScript != null)
        {
            moveScript.enabled = true;
            Debug.Log("Movement enabled for local player");
        }
    }

    void Start()
    {
        // Disable movement by default
        if (moveScript != null)
        {
            moveScript.enabled = false;
        }

        // If this is the local player, enable movement
        if (isLocalPlayer && moveScript != null)
        {
            moveScript.enabled = true;
            Debug.Log("Movement enabled in Start for local player");
        }
    }
}