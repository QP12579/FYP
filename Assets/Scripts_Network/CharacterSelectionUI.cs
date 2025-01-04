using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;


public class CharacterSelectionUI : MonoBehaviour
{
    [System.Serializable]


    public class CharacterSlot
    {
        public Image characterImage;
        public Button selectButton;
        public TextMeshProUGUI buttonText;
        public GameObject selectedIndicator;
        public GameObject maskObject;
    }

    public CharacterSlot[] characterSlots = new CharacterSlot[2];
    public TextMeshProUGUI countdownText;

    private void Start()
    {
        countdownText.gameObject.SetActive(false);

        // Initialize masks and selected indicators
        foreach (var slot in characterSlots)
        {
            slot.maskObject.SetActive(false);
            slot.selectedIndicator.SetActive(false);

            // Add button listeners
            int index = System.Array.IndexOf(characterSlots, slot);
            slot.selectButton.onClick.AddListener(() => OnCharacterButtonClicked(index));
        }
    }

    private void OnCharacterButtonClicked(int index)
    {
        var playerManager = NetworkClient.connection?.identity?.GetComponent<NetworkPlayerManager>();
        if (playerManager != null)
        {
            if (playerManager.GetSelectedCharacter() == index)
            {
                // If clicking the same character, cancel selection
                playerManager.CancelSelection();
            }
            else
            {
                // Select new character
                playerManager.SelectCharacter(index);
            }
        }
    }

    public void UpdateSlotState(int slotIndex, CharacterSelectState state, bool isLocalPlayer)
    {
        var slot = characterSlots[slotIndex];

        switch (state)
        {
            case CharacterSelectState.Available:
                slot.maskObject.SetActive(false);
                slot.selectedIndicator.SetActive(false);
                slot.buttonText.text = "Select";
                slot.selectButton.interactable = true;
                break;

            case CharacterSelectState.SelectedByLocal:
                slot.maskObject.SetActive(true);
                slot.selectedIndicator.SetActive(true);
                slot.buttonText.text = "Cancel";
                slot.selectButton.interactable = true;
                break;

            case CharacterSelectState.SelectedByOther:
                slot.maskObject.SetActive(true);
                slot.selectedIndicator.SetActive(true);
                slot.buttonText.text = "Unavailable";
                slot.selectButton.interactable = false;
                break;
        }
    }

    public void ShowCountdown(int seconds)
    {
        countdownText.gameObject.SetActive(true);
        countdownText.text = seconds.ToString();
    }
}
