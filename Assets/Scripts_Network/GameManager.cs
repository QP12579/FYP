using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CharacterSelectionUI;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;
    public CharacterSelectionUI selectionUI;
    public float countdownDuration = 4f;
    public string battleSceneName;

    [SyncVar(hook = nameof(OnCountdownChanged))]
    private float countdown = -1;
    [SyncVar]
    private bool isCountingDown = false;
    private void Awake()
    {
        Instance = this;
    }

    [Server]
    public void UpdateSelectionState()
    {
        var players = FindObjectsOfType<NetworkPlayerManager>();
        int readyCount = 0;

        foreach (var player in players)
        {
            if (player.IsReady()) readyCount++;
        }

        // Start countdown only if all players are ready
        if (readyCount >= 2 && !isCountingDown)
        {
            StartMatchCountdown();
        }
        // Stop countdown if not all players are ready
        else if (readyCount < 2 && isCountingDown)
        {
            StopMatchCountdown();
        }
    }

    [Server]
    private void StartMatchCountdown()
    {
        isCountingDown = true;
        countdown = countdownDuration;
        StartCoroutine(CountdownCoroutine());
    }

    [Server]
    private void StopMatchCountdown()
    {
        isCountingDown = false;
        countdown = -1;
        StopCoroutine(CountdownCoroutine());
        RpcUpdateCountdownDisplay(-1);
    }

    private IEnumerator CountdownCoroutine()
    {
        while (countdown > 0 && isCountingDown)
        {
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        if (countdown <= 0 && isCountingDown)
        {
            NetworkManager.singleton.ServerChangeScene(battleSceneName);
        }
    }

    [ClientRpc]
    private void RpcUpdateCountdownDisplay(float timeLeft)
    {
        if (timeLeft < 0)
        {
            selectionUI.countdownText.gameObject.SetActive(false);
            selectionUI.CountPanel.gameObject.SetActive(false);
        }
        else
        {
            selectionUI.countdownText.gameObject.SetActive(true);
            selectionUI.CountPanel.SetActive(true);
            selectionUI.countdownText.text = Mathf.CeilToInt(timeLeft).ToString();
        }
    }

    private void OnCountdownChanged(float oldValue, float newValue)
    {
        RpcUpdateCountdownDisplay(newValue);
    }



    public void UpdateUIState()
    {
        if (selectionUI == null) return;

        var players = FindObjectsOfType<NetworkPlayerManager>();

        // Reset all slots to available first
        for (int i = 0; i < selectionUI.characterSlots.Length; i++)
        {
            bool isLocalPlayerSlot = false;
            foreach (var player in players)
            {
                if (player.isLocalPlayer)
                {
                    isLocalPlayerSlot = true;
                    break;
                }
            }
            selectionUI.UpdateSlotState(i, CharacterSelectState.Available, isLocalPlayerSlot, NetworkServer.active);
        }

        // Update based on player selections
        foreach (var player in players)
        {
            int selectedChar = player.GetSelectedCharacter();
            if (selectedChar != -1)
            {
                bool isLocalPlayer = player.isLocalPlayer;
                selectionUI.UpdateSlotState(
                    selectedChar,
                    isLocalPlayer ? CharacterSelectState.SelectedByLocal : CharacterSelectState.SelectedByOther,
                    isLocalPlayer,
                    NetworkServer.active
                );
            }
        }
    }
}
public enum CharacterSelectState
{
    Available,
    SelectedByLocal,
    SelectedByOther
}

