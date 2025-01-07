using UnityEngine;
using Mirror;
using Cinemachine;

public class PlayerCameraManager : NetworkBehaviour
{
    [SerializeField] private GameObject virtualCameraPrefab; // prefab with CinemachineVirtualCamera

    private CinemachineVirtualCamera playerCamera;
    private GameObject spawnedCamera;

    public override void OnStartLocalPlayer()
    {
        // Only create camera for local player
        if (isLocalPlayer)
        {
            SetupCamera();
        }
    }

    void SetupCamera()
    {
        // Create new camera
        spawnedCamera = Instantiate(virtualCameraPrefab);
        playerCamera = spawnedCamera.GetComponent<CinemachineVirtualCamera>();

        // Set this player as the camera's follow target
        playerCamera.Follow = transform;

        // You might want to adjust priority to ensure this camera takes precedence
        playerCamera.Priority = 10;
    }

    void OnDestroy()
    {
        if (spawnedCamera != null)
        {
            Destroy(spawnedCamera);
        }
    }
}