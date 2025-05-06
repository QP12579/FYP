using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillPanel : MonoBehaviour
{
    public static SkillPanel instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI skillPointText;
    [SerializeField] private Button resetAllButton;
    [SerializeField] private List<SkillButton> skillButtons = new List<SkillButton>();
    [SerializeField] private Button[] equipButtons = new Button[2];
    [SerializeField] private Image[] equippedSkillIcons = new Image[2];

    [Header("Special Skill Settings")]
    [SerializeField] private List<SkillButton> specialSkillButtons = new List<SkillButton>();

    [Header("Tooltip Settings")]
    [SerializeField] private GameObject tooltipPanelPrefab;
    [SerializeField] private float tooltipOffsetX = 10f;
    [SerializeField] private float tooltipOffsetY = 10f;

    private SkillManager skillManager;
    private int _skillPoints;
    private SkillButton currentlySelectedSkill;
    private GameObject currentTooltip;
    private RectTransform tooltipRectTransform;
    private TextMeshProUGUI tooltipNameText;
    private TextMeshProUGUI tooltipDescriptionText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        skillManager = SkillManager.instance;
        InitializePanel();

        // 初始化 Tooltip
        if (tooltipPanelPrefab != null)
        {
            currentTooltip = Instantiate(tooltipPanelPrefab, transform);
            currentTooltip.SetActive(false);

            // 獲取 Tooltip 組件
            tooltipRectTransform = currentTooltip.GetComponent<RectTransform>();
            var texts = currentTooltip.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                tooltipNameText = texts[0];
                tooltipDescriptionText = texts[1];
            }
        }
    }

    private void Update()
    {
        // 更新 Tooltip 位置
        if (currentTooltip != null && currentTooltip.activeSelf)
        {
            Vector2 mousePosition = Input.mousePosition;
            tooltipRectTransform.position = new Vector3(
                mousePosition.x + tooltipOffsetX,
                mousePosition.y + tooltipOffsetY,
                0
            );

            // 確保 Tooltip 不會超出屏幕
            Vector3[] corners = new Vector3[4];
            tooltipRectTransform.GetWorldCorners(corners);

            float rightEdge = corners[2].x;
            float topEdge = corners[1].y;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            if (rightEdge > screenWidth)
            {
                tooltipRectTransform.position -= new Vector3(rightEdge - screenWidth, 0, 0);
            }

            if (topEdge > screenHeight)
            {
                tooltipRectTransform.position -= new Vector3(0, topEdge - screenHeight, 0);
            }
        }
    }

    public void ShowTooltip(SkillButton button)
    {
        if (currentTooltip == null) return;

        SkillData skillData = skillManager.GetSkillByID(button.id, button.level);
        if (skillData == null) return;

        // 更新 Tooltip 內容
        if (tooltipNameText != null) tooltipNameText.text = skillData.Name;
        if (tooltipDescriptionText != null) tooltipDescriptionText.text = skillData.Description;

        // 顯示 Tooltip
        currentTooltip.SetActive(true);

        // 立即更新位置
        Vector2 mousePosition = Input.mousePosition;
        tooltipRectTransform.position = new Vector3(
            mousePosition.x + tooltipOffsetX,
            mousePosition.y + tooltipOffsetY,
            0
        );
    }

    public void HideTooltip()
    {
        if (currentTooltip != null)
        {
            currentTooltip.SetActive(false);
        }
    }

    private void InitializePanel()
    {
        _skillPoints = skillManager.SkillPoints;
        UpdateSkillPointDisplay();

        foreach (var button in skillButtons)
        {
            // 初始化按鈕狀態
            UpdateButtonState(button);
        }

        // 初始化已裝備技能顯示
        UpdateEquippedSkillDisplay();
        InitializeSpecialSkills();
    }
    private void InitializeSpecialSkills()
    {
        // 清除現有特殊技能按鈕
        foreach (var button in specialSkillButtons)
        {
            button.button.interactable = false;
        }
    }

    public void OnSpecialSkillButtonClick(SkillButton button)
    {
        // 選擇特殊技能
        if (currentlySelectedSkill != null)
        {
            currentlySelectedSkill.isSelected = false;
            UpdateButtonVisual(currentlySelectedSkill);
        }

        currentlySelectedSkill = button;
        currentlySelectedSkill.isSelected = true;
        UpdateButtonVisual(currentlySelectedSkill);
    }
    private void UpdateButtonState(SkillButton button)
    {
        SkillData skillData = null;
        if (button.id == 5)
        {
            var specialSkills = skillManager.GetUnlockedSpecialSkills();
            foreach (var btn in specialSkillButtons)
            {
                if (btn == button)
                {
                    skillData = specialSkills.Find(s => s.Name == btn.name); // find button name is == to skill name
                    break;
                }
            }
        }
        else skillData = skillManager.GetSkillByID(button.id, button.level);
        if (skillData == null) return;

        button.isUnlocked = skillManager.IsSkillUnlocked(skillData);
        button.isSelected = false;

        // 更新圖標
        if (skillData.Icon != null)
        {
            button.iconImage.sprite = skillData.Icon;
        }

        // 更新按鈕狀態
        if (button.isUnlocked)
        {
            button.button.interactable = true;
            var colors = button.button.colors;
            colors.normalColor = Color.white;
            button.button.colors = colors;
        }
        else
        {
            // 檢查是否可以解鎖
            bool canUnlock = skillManager.CanUnlockSkill(skillData);
            button.button.interactable = canUnlock && _skillPoints > 0;

            var colors = button.button.colors;
            colors.normalColor = canUnlock ? Color.yellow : Color.gray;
            button.button.colors = colors;
        }
    }

    public void OnSkillButtonClick(SkillButton button)
    {
        SkillData skillData = skillManager.GetSkillByID(button.id, button.level);
        if (skillData == null) return;

        if (!button.isUnlocked)
        {
            // 嘗試解鎖技能
            if (skillManager.UnlockSkill(skillData))
            {
                _skillPoints--;
                UpdateSkillPointDisplay();
                button.isUnlocked = true;
                UpdateButtonState(button);
                RefreshAllButtons();
            }
        }
        else
        {
            // 選擇已解鎖的技能
            if (currentlySelectedSkill != null)
            {
                currentlySelectedSkill.isSelected = false;
                UpdateButtonVisual(currentlySelectedSkill);
            }

            currentlySelectedSkill = button;
            currentlySelectedSkill.isSelected = true;
            UpdateButtonVisual(currentlySelectedSkill);
        }
    }

    private void OnEquipButtonClick(int slotIndex)
    {
        if (currentlySelectedSkill == null) return;
        SkillData skillData = null;

        // 普通技能
        if (currentlySelectedSkill.id != 5)
        {
            skillData = skillManager.GetSkillByID(currentlySelectedSkill.id, currentlySelectedSkill.level);
        }
        // 特殊技能 (ID 5)
        else
        {
            // 從特殊技能按鈕列表中獲取對應的 SkillData
            var specialSkills = skillManager.GetUnlockedSpecialSkills();
            foreach (var btn in specialSkillButtons)
            {
                if (btn == currentlySelectedSkill)
                {
                    skillData = specialSkills.Find(s => s.Name == btn.name); // find button name is == to skill name
                    break;
                }
            }
        }

        if (skillData == null) return;
        // check equipped rule
        bool canEquip = false;
        if (slotIndex == 0 && (currentlySelectedSkill.id == 1 || currentlySelectedSkill.id == 3 || currentlySelectedSkill.id == 4))
        {
            canEquip = true;
        }
        else if (slotIndex == 1 && (currentlySelectedSkill.id == 2 || currentlySelectedSkill.id == 4 || currentlySelectedSkill.id == 5))
        {
            canEquip = true;
        }

        if (canEquip)
        {
            PlayerSkillController.instance.EquipSkill(slotIndex, skillData);
            UpdateEquippedSkillDisplay();
        }
        else
        {
            Debug.Log("This Skill Cannot equip in this slot.");
        }
    }

    private void UpdateButtonVisual(SkillButton button)
    {
        var colors = button.button.colors;

        if (button.isSelected)
        {
            colors.normalColor = Color.green;
            colors.highlightedColor = Color.green;
        }
        else if (button.isUnlocked)
        {
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.white;
        }

        button.button.colors = colors;
    }

    private void UpdateEquippedSkillDisplay()
    {
        for (int i = 0; i < equippedSkillIcons.Length; i++)
        {
            var skillData = PlayerSkillController.instance.equippedSkills[i].skillData;
            if (skillData != null && skillData.Icon != null)
            {
                equippedSkillIcons[i].sprite = skillData.Icon;
                equippedSkillIcons[i].gameObject.SetActive(true);
            }
            else
            {
                equippedSkillIcons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OnResetAllClick()
    {
        int refundedPoints = skillManager.ResetAllSkills();
        _skillPoints += refundedPoints;

        // 清除已裝備技能
        for (int i = 0; i < PlayerSkillController.instance.equippedSkills.Length; i++)
        {
            PlayerSkillController.instance.EquipSkill(i, null);
        }

        UpdateSkillPointDisplay();
        RefreshAllButtons();
        UpdateEquippedSkillDisplay();

        // 清除當前選擇
        if (currentlySelectedSkill != null)
        {
            currentlySelectedSkill.isSelected = false;
            UpdateButtonVisual(currentlySelectedSkill);
            currentlySelectedSkill = null;
        }
    }

    public void RefreshAllButtons()
    {
        foreach (var button in skillButtons)
        {
            UpdateButtonState(button);
        }
    }

    private void UpdateSkillPointDisplay()
    {
        if (skillPointText != null)
        {
            skillPointText.text = _skillPoints.ToString();
        }
    }
}