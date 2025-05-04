using DG.Tweening;
using Mirror.BouncyCastle.Ocsp;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityAndSkillsPanel : MonoBehaviour
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
        public List<int> unlockRequirementsID;
        public List<int> unlockTargetsID;
    }

    [Header("BaseValue")]
    [SerializeField] private TextMeshProUGUI AbilityPointText;
    public int AbilityPoint = 0; 
    public List<BaseValue> BaseValueButtons = new List<BaseValue>();

    private AbilitySystem ability;
    private bool isAbilityFileLoaded = false;
    private void Start()
    {
        ability = FindObjectOfType<AbilitySystem>();
        CheckAbilityTSV();

        if (isAbilityFileLoaded) Initialize();
        else LeanTween.delayedCall(0.5f, CheckAbilityTSV);
    }

    private void Initialize()
    {        
        foreach(var basebutton in BaseValueButtons)
        {
            basebutton.level = 0;
            UpdateButtonDisplay(basebutton);
            UpdateButtonInteractable(basebutton);
        }
    }

    private void CheckAbilityTSV()
    {
        isAbilityFileLoaded = ability.IsAbilityTSVLoadedCompletely();
        if(isAbilityFileLoaded) Initialize();
        else LeanTween.delayedCall(0.5f, CheckAbilityTSV);
    }

    public void OnBaseButtonClick(int id)
    {
        BaseValue button = BaseValueButtons.Find(i => i.id == id);
        if (button.level >= button.maxLevel) return;
        if (AbilityPoint <= 0) return;

        if(AbilityPointText!=null)
            AbilityPointText.text = AbilityPoint.ToString();
        print($"before:{button.level}");
        button.level++;
        print($"after:{button.level}");
        button.levelText.text = button.level + "/" + button.maxLevel;

        UpdateButtonDisplay(button);
        UpdateButtonInteractable(button);
        CheckUnlockedConditions(button);
        ability.UpgradeBaseValue(button.id, button.level);
        print("End1");
    }

    void UpdateButtonDisplay(BaseValue button)
    {
        if (ability == null)
        {
            Debug.LogError("AbilitySystem reference is null!");
            return;
        }

        UIValue ui = ability.GetAbilityValue(button.id, button.level);

        if (ui == null)
        {
            Debug.LogError($"Failed to get UI value for ability {button.id} level {button.level}");
            return;
        }

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

        if (button.unlockRequirementsID.Count == 0) { button.button.interactable = true; return; }

        bool isUnlocked = false;
        foreach (var req in button.unlockRequirementsID)
        {
            BaseValue baseValue = BaseValueButtons.Find(r => r.id == req);
            if (baseValue.level > 0)
            {
                isUnlocked = true;
                break;
            }
        }

        button.button.interactable = isUnlocked;
    }

    void CheckUnlockedConditions(BaseValue button)
    {
        foreach(var target in button.unlockTargetsID)
        {
            BaseValue baseValue = BaseValueButtons.Find(t => t.id == target);
            UpdateButtonInteractable(baseValue);
        }
    }
}
