using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class SimpleNetworkPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject[] characterPrefabs;
    [SerializeField] private GameObject selectionUI;
    [SerializeField] private Button[] characterButtons;

    private GameObject currentCharacter;

    public override void OnStartLocalPlayer()
    {
        Debug.Log("OnStartLocalPlayer called");
        // Show UI only for local player
        if (selectionUI != null)
        {
            selectionUI.SetActive(true);
            SetupButtons();
        }

        // Set initial position based on if we're host or client
        if (isServer)
        {
            transform.position = new Vector3(-5, 1, 0);
        }
        else
        {
            transform.position = new Vector3(5, 1, 0);
        }
    }

    void SetupButtons()
    {
        Debug.Log("Setting up buttons");
        if (characterButtons != null)
        {
            for (int i = 0; i < characterButtons.Length; i++)
            {
                int characterIndex = i;
                if (characterButtons[i] != null)
                {
                    characterButtons[i].onClick.AddListener(() => SpawnCharacter(characterIndex));
                }
            }
        }
    }

    // Directly spawn character without network sync for testing
    void SpawnCharacter(int index)
    {
        Debug.Log($"SpawnCharacter called with index {index}");

        if (!isLocalPlayer) return;

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }

        if (index >= 0 && index < characterPrefabs.Length)
        {
            currentCharacter = Instantiate(characterPrefabs[index], transform.position, transform.rotation);
            currentCharacter.transform.SetParent(transform);

            var movement = currentCharacter.GetComponent<playermovement>();
            if (movement != null)
            {
                movement.enabled = true;
                Debug.Log("Movement component enabled");
            }
        }
    }
}