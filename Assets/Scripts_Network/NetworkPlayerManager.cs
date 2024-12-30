using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class NetworkPlayerManager : NetworkBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private GameObject[] characterPrefabs;

    [Header("Arena Spawn Points")]
    [SerializeField] private Vector3 player1SpawnPoint = new Vector3(-5f, 1f, 0f);
    [SerializeField] private Vector3 player2SpawnPoint = new Vector3(5f, 1f, 0f);

    private GameObject spawnedCharacter;

    [SyncVar(hook = nameof(OnCharacterSelectionChanged))]
    private int selectedCharacterIndex = -1;

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer Called");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectionUI.SetActive(true);
            SetupButtons();
        }

        CmdSetPosition();
    }

    void SetupButtons()
    {
        var buttons = GameManager.Instance.characterSelectButtons;
        if (buttons != null)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                if (buttons[i] != null)
                {
                    buttons[i].onClick.AddListener(() => SelectCharacter(index));
                }
            }
        }
    }

    void SelectCharacter(int index)
    {
        if (!isLocalPlayer) return;

        // Hide UI immediately after selection
        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectionUI.SetActive(false);
        }

        CmdSelectCharacter(index);
    }

    [Command]
    void CmdSelectCharacter(int index)
    {
        if (index < 0 || index >= characterPrefabs.Length) return;
        selectedCharacterIndex = index;
    }

    void OnCharacterSelectionChanged(int oldIndex, int newIndex)
    {
        if (spawnedCharacter != null)
        {
            Destroy(spawnedCharacter);
        }

        if (newIndex >= 0 && newIndex < characterPrefabs.Length)
        {
            // Spawn the character
            GameObject characterPrefab = characterPrefabs[newIndex];
            spawnedCharacter = Instantiate(characterPrefab, transform.position, transform.rotation);
            spawnedCharacter.transform.SetParent(transform);

            // Set up movement
            var movement = spawnedCharacter.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                movement.enabled = isLocalPlayer;
            }

            // Set up camera ONLY for local player
            if (isLocalPlayer)
            {
                Debug.Log("Setting up camera for local player");
                var vcam = FindObjectOfType<CinemachineVirtualCamera>();
                if (vcam != null)
                {
                    vcam.Follow = spawnedCharacter.transform;
                    // Make sure this camera has highest priority
                    vcam.Priority = 100;
                    Debug.Log($"Camera following: {spawnedCharacter.name}");
                }
                else
                {
                    Debug.LogError("No CinemachineVirtualCamera found in scene!");
                }
            }
            else
            {
                Debug.Log("Not local player, skipping camera setup");
            }
        }
    }
    void Start()
    {
        if (isLocalPlayer)
        {
            var vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Follow = transform;
            }
        }
    }



    [Command]
    void CmdSetPosition()
    {
        bool isPlayer1 = (connectionToClient.connectionId == 0);
        Vector3 spawnPos = isPlayer1 ? player1SpawnPoint : player2SpawnPoint;
        transform.position = spawnPos;
    }
}