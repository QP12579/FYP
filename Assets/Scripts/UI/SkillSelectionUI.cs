using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSelectionUI : MonoBehaviour
{
    public GameObject skillSelectionPanel;
    public Transform skillButtonContainer;
    public GameObject skillButtonPrefab;

    private int selectedSlotIndex;

    public void ShowSkillSelectionForSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        skillSelectionPanel.SetActive(true);

        // Clear existing buttons
        foreach (Transform child in skillButtonContainer)
        {
            Destroy(child.gameObject);
        }

        // Create buttons for each unlocked skill
        foreach (var skill in SkillManager.instance.GetAllUnlockedSkills())
        {
            GameObject buttonObj = Instantiate(skillButtonPrefab, skillButtonContainer);
            SkillButton button = buttonObj.GetComponent<SkillButton>();
            button.Initialize(skill, () => SelectSkill(skill));
        }
    }

    private void SelectSkill(SkillData skill)
    {
        PlayerSkillController.instance.EquipSkill(selectedSlotIndex, skill);
        skillSelectionPanel.SetActive(false);
    }
}