using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class SkillManager : Singleton<SkillManager>
{
    public TextAsset skillsTSV;
    public string skillDataPath = "Skills/SkillData";
    public Dictionary<int, List<SkillData>> skillsByID = new Dictionary<int, List<SkillData>>();
    public List<SkillData> unlockedSkills = new List<SkillData>();
    public List<SkillData> specialSkills = new List<SkillData>(); // ID 5 skills

    void Awake()
    {
        if (skillsTSV == null)
            skillsTSV = Resources.Load<TextAsset>(skillDataPath);
        LoadSkillsFromTSV();
    }

    private void LoadSkillsFromTSV()
    {
        string[] lines = skillsTSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++) // Skip header
        {
            if (string.IsNullOrEmpty(lines[i].Trim())) continue;

            string[] fields = lines[i].Split('\t');

            // Parse skill types
            string[] typeStrings = fields[4].Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            SkillType[] types = new SkillType[typeStrings.Length];
            for (int j = 0; j < typeStrings.Length; j++)
            {
                if (Enum.TryParse(typeStrings[j], out SkillType type))
                {
                    types[j] = type;
                }
            }

            SkillData skill = new SkillData() // ID, level, iconPath, name, type, power, cooldown, description, prefabPath
            {
                ID = int.Parse(fields[0]),
                level = int.Parse(fields[1]),
                Name = fields[3],
                types = types,
                power = int.Parse(fields[5]),
                cooldown = int.Parse(fields[6]),
                Description = fields[7],
                prefabPath = fields[8],
                iconPath = fields[2] // Assuming this is the path to load the sprite later
                
            };

            // Load icon sprite (you'll need to implement this based on your project)
            skill.Icon = Resources.Load<Sprite>(skill.iconPath);

            // Organize by ID
            if (skill.ID == 5)
            {
                specialSkills.Add(skill);
            }
            else
            {
                if (!skillsByID.ContainsKey(skill.ID))
                {
                    skillsByID[skill.ID] = new List<SkillData>();
                }
                skillsByID[skill.ID].Add(skill);
            }
        }
        // After loading all skills, sort them by level within each ID
        foreach (var id in skillsByID.Keys)
        {
            skillsByID[id].Sort((a, b) => a.level.CompareTo(b.level));
        }
    }

    public List<SkillData> GetAllUnlockedSkills() => unlockedSkills;

    public bool IsSkillUnlocked(SkillData skill) => unlockedSkills.Contains(skill);

    public List<SkillData> GetSkillsByID(int id)
    {
        if (skillsByID.ContainsKey(id)) return skillsByID[id];
        return new List<SkillData>();
    }

    public SkillData GetRandomSpecialSkill()
    {
        if (specialSkills.Count == 0) return null;
        return specialSkills[UnityEngine.Random.Range(0, specialSkills.Count)];
    }
    public bool CanUnlockSkill(SkillData skill)
    {
        // Check if already unlocked
        if (unlockedSkills.Contains(skill)) return false;

        // For level 1 skills, always unlockable
        if (skill.level == 1) return true;

        // For higher levels, check if previous level is unlocked
        int previousLevel = skill.level - 1;
        return unlockedSkills.Exists(s => s.ID == skill.ID && s.level == previousLevel);
    }

    public void UnlockSkill(SkillData skill)
    {
        if (!CanUnlockSkill(skill)) return; 
        unlockedSkills.Add(skill);
        Debug.Log($"Unlocked: {skill.Name} (ID:{skill.ID}, Lvl:{skill.level})");
    }

    public bool HasNextLevelSkill(int id, int currentLevel)
    {
        if (!skillsByID.ContainsKey(id)) return false;
        return skillsByID[id].Exists(s => s.level == currentLevel + 1);
    }

    public SkillData GetNextLevelSkill(int id, int currentLevel)
    {
        if (!skillsByID.ContainsKey(id)) return null;
        return skillsByID[id].Find(s => s.level == currentLevel + 1);
    }
}