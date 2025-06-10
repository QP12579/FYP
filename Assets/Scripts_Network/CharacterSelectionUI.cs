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
        public TextMeshProUGUI selectedByText;  // New: Text to show which player selected
        public GameObject maskObject;
        
    }

    public TMP_Text IP_Text;

    public CharacterSlot[] characterSlots = new CharacterSlot[2];
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI playerIdentityText; // Shows if you are P1 or P2
    public GameObject CountPanel; 

    private void Start()
    {
        countdownText.gameObject.SetActive(false);
        CountPanel.gameObject.SetActive(false);

        foreach (var slot in characterSlots)
        {
            slot.maskObject.SetActive(false);
            slot.selectedIndicator.SetActive(false);
            slot.selectedByText.gameObject.SetActive(false);

            int index = System.Array.IndexOf(characterSlots, slot);
            slot.selectButton.onClick.AddListener(() => OnCharacterButtonClicked(index));
        }

        // Show player identity
        if (NetworkServer.active)
        {
            playerIdentityText.text = "You are Player 1";
            UpdateIPText();
        }
        else if (NetworkClient.active)
        {
            playerIdentityText.text = "You are Player 2";
            IP_Text.gameObject.SetActive(false);
        }
    }

    public void UpdateIPText()
    {
        string localIP = "Not available";

        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
        }
        catch
        {
            localIP = "Error retrieving IP";
        }

        if (IP_Text != null)
        {
            IP_Text.text = $"{localIP}";
        }

        
        IP_Text.gameObject.SetActive(true);
    }

    public void UpdateSlotState(int slotIndex, CharacterSelectState state, bool isLocalPlayer, bool isHost)
    {
        var slot = characterSlots[slotIndex];

        switch (state)
        {
            case CharacterSelectState.Available:
                slot.maskObject.SetActive(false);
                slot.selectedIndicator.SetActive(false);
                slot.selectedByText.gameObject.SetActive(false);
                slot.buttonText.text = "Select";
                slot.selectButton.interactable = true;
                break;

            case CharacterSelectState.SelectedByLocal:
                slot.maskObject.SetActive(true);
                slot.selectedIndicator.SetActive(true);
                slot.selectedByText.gameObject.SetActive(true);
                slot.selectedByText.text = isHost ? "P1 Selected(You)" : "P2 Selected(You)";
                slot.buttonText.text = "Cancel";
                slot.selectButton.interactable = true;
                break;

            case CharacterSelectState.SelectedByOther:
                slot.maskObject.SetActive(true);
                slot.selectedIndicator.SetActive(true);
                slot.selectedByText.gameObject.SetActive(true);
                slot.selectedByText.text = isHost ? "P2 Selected" : "P1 Selected";
                slot.buttonText.text = "Unavailable";
                slot.selectButton.interactable = false;
                break;
        }
    }

    private void OnCharacterButtonClicked(int index)
    {
        var playerManager = NetworkClient.connection?.identity?.GetComponent<NetworkPlayerManager>();
        if (playerManager != null)
        {
            if (playerManager.GetSelectedCharacter() == index)
            {
                playerManager.CancelSelection();
            }
            else
            {
                playerManager.SelectCharacter(index);
            }
        }
    }

    public void ShowCountdown(int seconds)
    {
        countdownText.gameObject.SetActive(true);
        CountPanel.gameObject.SetActive(true);
        countdownText.text = seconds.ToString();
    }
}
