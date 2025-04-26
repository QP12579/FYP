using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SkillChoiceEvent : UnityEvent<List<SkillData>> { }

[System.Serializable]
public class SkillSelectedEvent : UnityEvent<SkillData> { }

public class ChoiceSystem : MonoBehaviour
{
    public SkillManager skillManager;

    [Header("Events")]
    public SkillChoiceEvent OnChoicesGenerated = new SkillChoiceEvent();
    public SkillSelectedEvent OnSkillSelected = new SkillSelectedEvent();

    [Header("Settings")]
    [Tooltip("Number of choices to present")]
    public int choiceCount = 3;
    [Tooltip("Whether to allow duplicate skill types in choices")]
    public bool allowDuplicateTypes = false;

    private List<SkillData> currentChoices = new List<SkillData>();

    void Start()
    {
        if (skillManager == null)
        {
            skillManager = FindObjectOfType<SkillManager>();
        }
    }
    public void GenerateLevelUpChoices()
    {
        currentChoices.Clear();
        List<int> availableIDs = new List<int>(skillManager.skillsByID.Keys);

        // Filter out IDs where all skills are unlocked or no next level available
        availableIDs.RemoveAll(id =>
        {
            // Get highest unlocked level for this ID
            int highestUnlocked = skillManager.unlockedSkills
                .FindAll(s => s.ID == id)
                .Max(s => s.level);

            // Check if there's a next level skill available
            return !skillManager.HasNextLevelSkill(id, highestUnlocked);
        });

        // If no IDs left, return empty list
        if (availableIDs.Count == 0)
        {
            OnChoicesGenerated?.Invoke(currentChoices);
            return;
        }

        // Select random IDs until we have enough choices
        while (currentChoices.Count < choiceCount && availableIDs.Count > 0)
        {
            int randomIDIndex = Random.Range(0, availableIDs.Count);
            int selectedID = availableIDs[randomIDIndex];
            availableIDs.RemoveAt(randomIDIndex);

            // Get highest unlocked level for this ID
            int highestUnlocked = skillManager.unlockedSkills
                .FindAll(s => s.ID == selectedID)
                .DefaultIfEmpty(new SkillData { level = 0 })
                .Max(s => s.level);

            // Get the next level skill
            SkillData nextSkill = skillManager.GetNextLevelSkill(selectedID, highestUnlocked);
            if (nextSkill != null)
            {
                currentChoices.Add(nextSkill);
            }
        }

        OnChoicesGenerated?.Invoke(currentChoices);
    }

    public void SelectSkill(int choiceIndex)
    {
        if (choiceIndex >= 0 && choiceIndex < currentChoices.Count)
        {
            SkillData selectedSkill = currentChoices[choiceIndex];
            skillManager.UnlockSkill(selectedSkill);
            OnSkillSelected?.Invoke(selectedSkill);
            currentChoices.Clear();
        }
    }

    private bool HasMatchingType(SkillData a, SkillData b)
    {
        foreach (var typeA in a.types)
        {
            foreach (var typeB in b.types)
            {
                if (typeA == typeB) return true;
            }
        }
        return false;
    }
}
