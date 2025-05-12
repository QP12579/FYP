using DG.Tweening;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentUI : NetworkBehaviour
{
    [Header("UI References")]
    public Slider HPSlider;
    public Slider MPSlider;
    public Slider SpecialAttackSlider;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI HpText;
    public GameObject SPFullEffect;
    public GameObject SPHintKeywords;
    private bool isSPFullEffectAnimating = false;

    // Reference to the main canvas that holds all UI elements
    private GameObject playerUICanvas;

    private void Awake()
    {
        // Initialize UI only for local player
        if (isLocalPlayer)
        {
            InitializeUI();
        }
    }

    private void InitializeUI()
    {
        Debug.Log($"Initializing UI for player {gameObject.name} (isLocalPlayer: {isLocalPlayer})");

        // First, we need to create a unique UI canvas for this player
        // or ensure we're accessing the correct one
        string canvasName = "PlayerUI_" + netId.ToString();

        // Find or create a canvas specific to this player
        playerUICanvas = GameObject.Find(canvasName);
        if (playerUICanvas == null)
        {
            // UI canvas doesn't exist yet, let's create or find the base one and clone it
            GameObject baseCanvas = GameObject.Find("UISavedPrefab");
            if (baseCanvas != null)
            {
                // Instantiate a new canvas for this player
                playerUICanvas = Instantiate(baseCanvas);
                playerUICanvas.name = canvasName;

                // Make sure the UI is visible (proper layer, etc.)
                SetUIForLocalPlayer(playerUICanvas);

                Debug.Log($"Created new UI canvas for player {gameObject.name}: {canvasName}");
            }
            else
            {
                Debug.LogError("Could not find base UI canvas 'UISavedPrefab'");
                return;
            }
        }

        // Now find the references within this player's UI canvas
        Transform playerHPMPSP = playerUICanvas.transform.Find("PlayerHPMPSP");
        if (playerHPMPSP != null)
        {
            Slider[] sliders = playerHPMPSP.GetComponentsInChildren<Slider>();
            foreach (Slider s in sliders)
            {
                if (s.name == "HP_Slider" && HPSlider == null) { HPSlider = s; continue; }
                if (s.name == "MP_Slider" && MPSlider == null) { MPSlider = s; continue; }
                if (s.name == "SpecialSkill_Slider" && SpecialAttackSlider == null) { SpecialAttackSlider = s; continue; }
            }

            // Find other UI elements within this canvas
            if (SPFullEffect == null)
                SPFullEffect = playerUICanvas.transform.Find("SPFullEffect")?.gameObject;

            if (SPHintKeywords == null)
                SPHintKeywords = playerUICanvas.transform.Find("SpeechText")?.gameObject;

            if (levelText == null)
            {
                string levelTextName = gameObject.name[0] == 'M' ? "MLevelText" : "TLevelText";
                levelText = playerUICanvas.transform.Find(levelTextName)?.GetComponent<TextMeshProUGUI>();
            }
          



            if (HPSlider == null || MPSlider == null || SpecialAttackSlider == null
                || SPFullEffect == null || SPHintKeywords == null || levelText == null)
            {
                // Not all UI elements found, try again after a short delay
                LeanTween.delayedCall(0.1f, InitializeUI);
                Debug.LogWarning($"Not all UI elements found for {gameObject.name}, retrying...");
            }
            else
            {
                SetAlpha(SPFullEffect, 0f);
                SPHintKeywords.SetActive(false);
                Debug.Log($"UI initialization complete for player {gameObject.name}");
            }
        }
        else
        {
            Debug.LogError($"Could not find PlayerHPMPSP in canvas for player {gameObject.name}");
        }
    }

    private void SetUIForLocalPlayer(GameObject uiCanvas)
    {
        // Setup the UI canvas for this local player
        Canvas canvas = uiCanvas.GetComponent<Canvas>();
        if (canvas != null)
        {
            // Ensure it's in the right render mode
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Set proper layer
            uiCanvas.layer = LayerMask.NameToLayer("UI");

            // Set sorting order if needed
            canvas.sortingOrder = 10;
        }
    }

    // This method should only be called by the local player or for the local player
    public void UpdatePlayerUI(float currentHP, float maxHP, float currentMP, float maxMP, float SP, int level)
    {
        // Direct reference finding approach (simpler but works)
        if (HPSlider == null)
            HPSlider = GameObject.Find("HP_Slider")?.GetComponent<Slider>();

        if (MPSlider == null)
            MPSlider = GameObject.Find("MP_Slider")?.GetComponent<Slider>();

        if (SpecialAttackSlider == null)
            SpecialAttackSlider = GameObject.Find("SpecialSkill_Slider")?.GetComponent<Slider>();

        if (levelText == null)
            levelText = GameObject.Find("LevelText")?.GetComponent<TMPro.TextMeshProUGUI>();

        // Update values if references found
        if (HPSlider != null)
        {
            HPSlider.value = currentHP;
            HPSlider.maxValue = maxHP;
            TMPro.TextMeshProUGUI hpText = HPSlider.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (hpText != null)
                hpText.text = currentHP.ToString("####") + "/" + maxHP.ToString();
            else
                Debug.LogWarning("HP Text not found in slider");
        }

        if (MPSlider != null)
        {
            MPSlider.value = currentMP;
            MPSlider.maxValue = maxMP;
            TMPro.TextMeshProUGUI mpText = MPSlider.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (mpText != null)
                mpText.text = currentMP.ToString("####") + "/" + maxMP.ToString();
            else
                Debug.LogWarning("MP Text not found in slider");
        }

        if (SpecialAttackSlider != null)
        {
            SpecialAttackSlider.value = SP;

            if (SpecialAttackSlider.value == 1)
            {
                if (!isSPFullEffectAnimating)
                {
                    AnimateAlpha();
                    SPHintKeywords.SetActive(true);
                }
            }
            else
            {
                if (isSPFullEffectAnimating)
                {
                    DOTween.Kill(SPFullEffect);
                    isSPFullEffectAnimating = false;
                }
                if (SPFullEffect != null)
                {
                    SetAlpha(SPFullEffect, 0f);
                    SPHintKeywords.SetActive(false);
                }
            }
        }
        if (levelText != null)
            levelText.text = level.ToString();

    }

    private void AnimateAlpha()
    {
        isSPFullEffectAnimating = true;

        SPFullEffect.GetComponent<CanvasGroup>().DOFade(1f, 1f).OnComplete(() =>
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                SPFullEffect.GetComponent<CanvasGroup>().DOFade(0f, 1f).OnComplete(() =>
                {
                    isSPFullEffectAnimating = false;
                });
            });
        });
    }

    private void SetAlpha(GameObject obj, float alpha)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = alpha;
    }
}