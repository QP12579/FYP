using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityAndSkillsPanel : BasePanel
{
    [System.Serializable]
    public class BaseValue
    {
        public Button button;
        public int id;
        [HideInInspector]
        public int level = 0;
        [HideInInspector]
        public int maxLevel = 3;
        public TextMeshProUGUI description;
        public TextMeshProUGUI levelText;
        public List<BaseValue> unlockRequirements;
        public List<BaseValue> unlockTargets;
    }

    [Header("BaseValue")]
    [SerializeField] private TextMeshProUGUI AbilityPointText;
    [HideInInspector] public int AbilityPoint = 0; 
    public List<BaseValue> BaseValueButtons = new List<BaseValue>();

    private AbilitySystem ability;
    protected override void Start()
    {
        base.Start();
        ability = FindObjectOfType<AbilitySystem>();
        foreach(var basebutton in BaseValueButtons)
        {
            basebutton.level = 0;
            UpdateButtonDisplay(basebutton);
            basebutton.button.onClick.AddListener(() => OnBaseButtonClick(basebutton));
            UpdateButtonInteractable(basebutton);
        }
    }

    public void OnBaseButtonClick(BaseValue button)
    {
        if (button.level >= button.maxLevel) return;
        if (AbilityPoint <= 0) return;

        button.level++;
        button.levelText.text = button.level + "/" + button.maxLevel;

        UpdateButtonDisplay(button);
        UpdateButtonInteractable(button);
        ability.UpgradeBaseValue(button.id, button.level);
    }

    void UpdateButtonDisplay(BaseValue button)
    {
        UIValue ui = ability.GetAbilityValue(button.id, button.level);
        button.maxLevel = ui.maxLevel;
        button.levelText.text = $"{button.level}/{button.maxLevel}";
        button.description.text = ui.description;
    }
    void UpdateButtonInteractable(BaseValue button)
    {
        if (button.level >= button.maxLevel)
        {
            button.button.interactable = false;
            return;
        }

        bool isUnlocked = true;
        foreach (var req in button.unlockRequirements)
        {
            if (req.level < 1)
            {
                isUnlocked = false;
                break;
            }
        }

        button.button.interactable = isUnlocked;
    }

    void CheckUnlockedConditions(BaseValue button)
    {
        foreach(var target in button.unlockTargets)
        {
            UpdateButtonInteractable(target);
        }
    }
}
