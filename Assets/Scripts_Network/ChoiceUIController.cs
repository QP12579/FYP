using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillChoiceUIController : MonoBehaviour
{
    public GameObject choicePanel;
    public List<Button> choiceButtons;
    public List<Image> choiceIcons;
    public List<TMP_Text> choiceNames;
    public List<TMP_Text> choiceDescriptions;

    private ChoiceSystem choiceSystem;

    void Start()
    {
        choiceSystem = FindObjectOfType<ChoiceSystem>();

        // Setup button listeners
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            int index = i; // Important for closure
            choiceButtons[i].onClick.AddListener(() => choiceSystem.SelectSkill(index));
        }

        HideChoices(null);
    }

    private void ShowChoices(List<SkillData> choices)
    {
        choicePanel.SetActive(true);

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            bool hasChoice = i < choices.Count;
            choiceButtons[i].gameObject.SetActive(hasChoice);

            if (hasChoice)
            {
                SkillData skill = choices[i];
                choiceIcons[i].sprite = skill.Icon;
                choiceNames[i].text = skill.Name;
                choiceDescriptions[i].text = skill.Description;
            }
        }
    }

    private void HideChoices(SkillData _)
    {
        choicePanel.SetActive(false);
    }

    // Call this when player levels up
    public void OnPlayerLevelUp()
    {
        choiceSystem.GenerateLevelUpChoices();
    }
}