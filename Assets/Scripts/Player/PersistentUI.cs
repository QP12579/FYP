using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PersistentUI : MonoBehaviour
{
    public static PersistentUI Instance { get; private set; }

    [Header("UI References")]
    public Slider HPSlider;
    public Slider MPSlider;
    public Slider SpecialAttackSlider;
    public TextMeshProUGUI levelText;
    public GameObject SPFullEffect;
    public GameObject SPHintKeywords;
    private bool isSPFullEffectAnimating = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (HPSlider == null || MPSlider == null || SpecialAttackSlider == null)
        {
            GameObject PlayerHPMPSP = GameObject.Find("UISavedPrefab/PlayerHPMPSP");
            Slider[] sliders = PlayerHPMPSP.GetComponentsInChildren<Slider>();
            foreach (Slider s in sliders)
            {
                if (s.name == "HP_Slider" && HPSlider == null) { HPSlider = s; continue; }
                if (s.name == "MP_Slider" && MPSlider == null) { MPSlider = s; continue; }
                if (s.name == "SpecialSkill_Slider" && SpecialAttackSlider == null) { SpecialAttackSlider = s; continue; }
            }
        }
        if (SPFullEffect == null)
            SPFullEffect = GameObject.Find("SPFullEffect");
        if (SPHintKeywords == null)
            SPHintKeywords = GameObject.Find("SpeechText");
        if (levelText == null)
            levelText = gameObject.name[0] == 'M' ? GameObject.Find("MLevelText").GetComponent<TextMeshProUGUI>()
                : GameObject.Find("TLevelText").GetComponent<TextMeshProUGUI>();

        if(HPSlider == null || MPSlider == null || SpecialAttackSlider == null
            || SPFullEffect == null || SPHintKeywords == null || levelText == null)
            LeanTween.delayedCall(0.1f, InitializeUI);
        else
        {
            SetAlpha(SPFullEffect, 0f);
            SPHintKeywords.SetActive(false);
        }
    }

    public void UpdatePlayerUI(float currentHP, float maxHP, float currentMP, float maxMP, float SP, int level)
    {
        HPSlider.value = currentHP;
        MPSlider.value = currentMP;
        HPSlider.maxValue = maxHP;
        MPSlider.maxValue = maxMP;

        HPSlider.transform.GetComponentInChildren<TextMeshProUGUI>().text =
            currentHP.ToString("####") + "/" + maxHP.ToString();
        MPSlider.transform.GetComponentInChildren<TextMeshProUGUI>().text =
            currentMP.ToString("####") + "/" + maxMP.ToString();
        levelText.text = level.ToString();

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
            SetAlpha(SPFullEffect, 0f);
            SPHintKeywords.SetActive(false);
        }
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
