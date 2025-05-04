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
    public TextMeshProUGUI levelText;

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
    }
    public void UpdatePlayerUI(float currentHP, float maxHP, float currentMP, float maxMP, int level)
    {
        HPSlider.value = currentHP;
        HPSlider.maxValue = maxHP;
        MPSlider.value = currentMP;
        MPSlider.maxValue = maxMP;

        HPSlider.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
            currentHP.ToString() + "/" + maxHP.ToString();
        MPSlider.transform.Find("Text").GetComponent<TextMeshProUGUI>().text =
            currentMP.ToString() + "/" + maxMP.ToString();
        levelText.text = level.ToString();
    }
}
