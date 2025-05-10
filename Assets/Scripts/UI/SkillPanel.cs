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
    [SerializeField] private List<SkillButton> ID5SkillButtons = new List<SkillButton>();

    [Header("Tooltip Settings")]
    [SerializeField] private GameObject tooltipPanelPrefab;
    [SerializeField] private float tooltipOffsetX = 10f;
    [SerializeField] private float tooltipOffsetY = 10f;

    [Header("Button Color Settings")]
    [SerializeField] private Color NormalColor = Color.white;
    [SerializeField] private Color CanUnlockColor = Color.gray;
    [SerializeField] private Color UnlockedColor = Color.gray;
    [SerializeField] private Color lockedColor = Color.gray;
    [SerializeField] private Color selectedColor = Color.white;

    [Header("Audio Clip")]
    [SerializeField] private AudioClip SFX_BuySkill;
    [SerializeField] private AudioClip SFX_SelectSkill;
    [SerializeField] private AudioClip SFX_BuyFail;
    [SerializeField] private AudioClip SFX_LockedSkill;
    [SerializeField] private AudioClip SFX_EquipSkill;
    [SerializeField] private AudioClip SFX_EquipFail;
    [SerializeField] private AudioClip SFX_EmptyEquipSkill;

    [Header("References")]
    [SerializeField] private UIController uiController;
    private SkillManager skillManager;
    private PlayerSkillController playerSkillController;
    private int _skillPoints;
    private SkillButton currentlySelectedSkill;
    private GameObject currentTooltip;
    private RectTransform tooltipRectTransform;
    private bool isUIPanelEnable;
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
            //Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializePanel();
        FindRefences();
    }

    private void OnEnable()
    {
        FindRefences();
    }

    private void FindRefences()
    {
        if (skillManager != null && playerSkillController != null) return;
        skillManager = SkillManager.instance;
        playerSkillController = FindObjectOfType<PlayerSkillController>();
        if (playerSkillController == null || skillManager == null)
            LeanTween.delayedCall(0.5f, FindRefences);
    }

    private void InitializeTooltip()
    {
        // 初始化 Tooltip
        if (tooltipPanelPrefab != null)
        {
            currentTooltip = Instantiate(tooltipPanelPrefab, transform);
            currentTooltip.SetActive(false);
            currentTooltip.gameObject.transform.SetParent(transform, false);

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
        if (currentTooltip != null)
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
            if (uiController.state != UIPanelState.SkillAbilityPanel)
                HideTooltip();
        }
    }

    public void ShowTooltip(SkillButton button)
    {
        if (currentTooltip == null)
            InitializeTooltip();
        if (currentTooltip == null)
        {
            Debug.LogError("Current Tooltip is null!");
            return;
        }

        SkillData skillData = skillManager.GetSkillByID(button.id, button.level);
        if(skillData ==  null ) skillData = skillManager.GetID5SkillData(button.button.name);
        if (skillData == null)
        {
            Debug.LogWarning($"Skill data not found for ID: {button.id}, Level: {button.level}");
            return;
        }

        // 更新 Tooltip 內容
        if (tooltipNameText != null) tooltipNameText.text = skillData.Name;
        else Debug.LogError("tooltipNameText is null!");
        if (tooltipDescriptionText != null) tooltipDescriptionText.text = skillData.Description;
        else Debug.LogError("tooltipDescriptionText is null!");

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
        foreach(var button in ID5SkillButtons)
        {
            UpdateButtonState(button);
        }

        // 初始化已裝備技能顯示
        UpdateEquippedSkillDisplay();
    }

    private void UpdateButtonState(SkillButton button)
    {
        SkillData skillData = null;
        if (button.id == 5)
        {
            skillData = skillManager.GetID5SkillData(button.button.name);
        }
        else skillData = skillManager.GetSkillByID(button.id, button.level);
        if (skillData == null) { Debug.Log("Button's Data cannot found in Database."); return; }

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
            colors.normalColor = UnlockedColor;
            colors.highlightedColor = selectedColor;
            button.button.colors = colors;
        }
        else
        {
            bool canUnlock = skillManager.CanUnlockSkill(skillData);
            button.button.interactable = canUnlock && _skillPoints > 0;

            var colors = button.button.colors;
            colors.normalColor = canUnlock ? CanUnlockColor : lockedColor;
            colors.highlightedColor = NormalColor;
            button.button.colors = colors;
        }
    }

    public void OnSkillButtonClick(SkillButton button)
    {
        SkillData skillData = skillManager.GetSkillByID(button.id, button.level);
        if (skillData == null) skillData = skillManager.GetID5SkillData(button.button.name);
        int needSkillPT = skillData.ID == 5? 2 : 1;
        if (skillData == null || _skillPoints < needSkillPT ) 
        {
            if (SFX_BuyFail != null)
                SoundManager.instance.PlaySFX(SFX_BuyFail);
            return; }

        if (!button.isUnlocked)
        {
            // 嘗試解鎖技能
            if (skillManager.UnlockSkill(skillData))
            {
                _skillPoints -= needSkillPT;
                UpdateSkillPointDisplay();
                button.isUnlocked = true;
                UpdateButtonState(button);
                RefreshAllButtons();
                if (SFX_BuySkill != null)
                    SoundManager.instance.PlaySFX(SFX_BuySkill);
            }
        }
        if(button.isUnlocked)
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
            if (SFX_SelectSkill != null)
                SoundManager.instance.PlaySFX(SFX_SelectSkill);
        }
    }

    public void OnEquipButtonClick(int slotIndex)
    {
        if (currentlySelectedSkill == null) { Debug.Log("Current no selected skill so Returned??? OAO"); return; }
        SkillData skillData = null;

        // 普通技能
        if (currentlySelectedSkill.id != 5)
        {
            skillData = skillManager.GetSkillByID(currentlySelectedSkill.id, currentlySelectedSkill.level);
        }
        // 特殊技能 (ID 5)
        else
        {
            skillData = skillManager.GetID5SkillData(currentlySelectedSkill.button.name);
        }

        if (skillData == null)
        {
            Debug.Log("DataNull so Returned??? OAO"); return; }
        // check equipped rule
        bool canEquip = skillManager.CanEquipToSlot(skillData, slotIndex);


        if (canEquip)
        {
            int otherSlot = slotIndex == 0 ? 1 : 0;
            bool isOtherSameSkill = playerSkillController.equippedSkills[otherSlot].skillData.ID == currentlySelectedSkill.id &&
                playerSkillController.equippedSkills[otherSlot].skillData.level == currentlySelectedSkill.level;
            bool isSameSkill = currentlySelectedSkill.id != 5?
                (playerSkillController.equippedSkills[slotIndex].skillData.ID == currentlySelectedSkill.id &&
                playerSkillController.equippedSkills[slotIndex].skillData.level == currentlySelectedSkill.level) 
                : playerSkillController.equippedSkills[slotIndex].skillData.Name.Equals(currentlySelectedSkill.name);


            if (isOtherSameSkill || isSameSkill)
            {
                SkillData emptyData = new SkillData
                {
                    ID = 0,
                    level = 0,
                    Icon = null,
                    Name = "",
                    Description = "",
                    power = 0,
                    MP = 0,
                    cooldown = 0,
                };

                playerSkillController.EquipSkill(isSameSkill? slotIndex : otherSlot, emptyData);
                if(SFX_EmptyEquipSkill!=null)
                    SoundManager.instance.PlaySFX(SFX_EmptyEquipSkill);
            }
            if(!isSameSkill)
            {
                playerSkillController.EquipSkill(slotIndex, skillData);
                if(SFX_EquipSkill!=null)
                    SoundManager.instance.PlaySFX(SFX_EquipSkill);
            }
            UpdateEquippedSkillDisplay();
        }
        else
        {
            if(SFX_EquipFail!=null)
                SoundManager.instance.PlaySFX(SFX_EquipFail);
            Debug.Log("This Skill Cannot equip in this slot.");
        }
    }

    private void UpdateButtonVisual(SkillButton button)
    {
        var colors = button.button.colors;

        if (button.isSelected)
        {
            colors.normalColor = selectedColor;
            colors.highlightedColor = NormalColor;
        }
        else if (button.isUnlocked)
        {
            colors.normalColor = UnlockedColor;
            colors.highlightedColor = NormalColor;
        }

        button.button.colors = colors;
    }

    private void UpdateEquippedSkillDisplay()
    {
        for (int i = 0; i < equippedSkillIcons.Length; i++)
        {
            var skillData = playerSkillController.equippedSkills[i].skillData;
            if (skillData != null && skillData.Icon != null)
            {
                equippedSkillIcons[i].sprite = skillData.Icon;
                equippedSkillIcons[i].color = NormalColor;
            }
            else
            {
                equippedSkillIcons[i].color = Color.clear;
            }
        }
    }

    public void OnResetAllClick()
    {
        int refundedPoints = skillManager.ResetAllSkills();
        _skillPoints += refundedPoints;

        // 清除已裝備技能
        for (int i = 0; i < playerSkillController.equippedSkills.Length; i++)
        {
            SkillData emptyData = new SkillData(){
                ID = 0,
                level = 0,
                Icon = null,
                Name = "",
                Description = "",
                power = 0,
                MP = 0,
                cooldown = 0
            };
            playerSkillController.EquipSkill(i, emptyData);
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

    public void AddSkillPoints(int p)
    {
        _skillPoints += p;
        UpdateSkillPointDisplay();
    }
}