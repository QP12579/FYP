using System.Collections.Generic;
using System;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    public TextAsset skillsTSV;
    public Dictionary<int, List<SkillData>> skillsByID = new Dictionary<int, List<SkillData>>();
    public List<SkillData> unlockedSkills = new List<SkillData>();
    public List<SkillData> ID5Skills = new List<SkillData>(); // ID 5 skills

    private string skillDataPath = "Skills/SkillData";

    [SerializeField] private int _skillPoints;
    public int SkillPoints => _skillPoints;

    public void AddSkillPoints(int amount)
    {
        _skillPoints += amount;
        SkillPanel.instance.RefreshAllButtons();
    }
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

            SkillData skill = new SkillData() // ID, level, iconSpace, name, type, mp, power, cooldown, description
            {
                ID = int.Parse(fields[0]),
                level = int.Parse(fields[1]),
                Name = fields[3],
                types = types,
                MP = int.Parse(fields[5]),
                power = int.Parse(fields[6]),
                cooldown = int.Parse(fields[7]),
                Description = fields[8],
                //iconPath = fields[2] // Assuming this is the path to load the sprite later
                
            };

            // Load icon sprite (you'll need to implement this based on your project)
            skill.Icon = Resources.Load<Sprite>($"Skills/SkillUIIcon/UI_Skill_Icon_{skill.Name}");

            // Organize by ID
            if (skill.ID == 5)
            {
                ID5Skills.Add(skill);
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

    public SkillData GetSkillByID(int id, int level)
    {
        if (skillsByID.ContainsKey(id))
            return skillsByID[id].Find(l =>l.level == level);
        return null;
    }

    public SkillData GetRandomSpecialSkill()
    {
        if (ID5Skills.Count == 0) return null;
        return ID5Skills[UnityEngine.Random.Range(0, ID5Skills.Count)];
    }

    public SkillData GetID5SkillData(string name)
    {
        foreach (var skill in ID5Skills) 
        {
            if(skill.Name == name)
                return skill;
        }
        Debug.Log("Cannot found SpecialSkillData");
        return null;
    }

    public bool AddSpecialSkill(SkillData specialSkill)
    {
        if (specialSkill == null || specialSkill.ID != 5) return false;

        // 檢查是否已擁有該特殊技能
        if (unlockedSkills.Exists(s => s.ID == 5 && s.Name == specialSkill.Name))
        {
            Debug.Log("已經擁有這個特殊技能");
            return false;
        }

        unlockedSkills.Add(specialSkill);
        SkillPanel.instance?.RefreshAllButtons();
        return true;
    }
    public List<SkillData> GetUnlockedSpecialSkills()
    {
        return unlockedSkills.FindAll(s => s.ID == 5);
    }
    public bool HasSpecialSkill(string skillName)
    {
        return unlockedSkills.Exists(s => s.ID == 5 && s.Name == skillName);
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

    public bool UnlockSkill(SkillData skill)
    {
        if (unlockedSkills.Contains(skill)) return false;

        bool canUnlock = skill.level == 1 ||
                       (unlockedSkills.Exists(s => s.ID == skill.ID && s.level == skill.level - 1)) ||
                       skill.ID == 5;

        if (canUnlock && _skillPoints > 0)
        {
            unlockedSkills.Add(skill);
            _skillPoints--;
            SkillPanel.instance.RefreshAllButtons();
            return true;
        }
        return false;
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
    public bool HasAnyUnlockableSkills()
    {
        foreach (var skillList in skillsByID.Values)
        {
            foreach (var skill in skillList)
            {
                if (CanUnlockSkill(skill)) return true;
            }
        }
        return false;
    }
    public List<SkillData> GetAllUnlockableSkills()
    {
        List<SkillData> unlockableSkills = new List<SkillData>();
        foreach (var skillList in skillsByID.Values)
        {
            foreach (var skill in skillList)
            {
                if (CanUnlockSkill(skill)) unlockableSkills.Add(skill);
            }
        }
        return unlockableSkills;
    }
    public bool ResetSkill(int id, int level)
    {
        SkillData skillToRemove = unlockedSkills.Find(s => s.ID == id && s.level == level);
        if (skillToRemove != null)
        {
            unlockedSkills.Remove(skillToRemove);
            _skillPoints++;
            return true;
        }
        return false;
    }
    public int ResetAllSkills()
    {
        int refundedPoints = unlockedSkills.Count;
        unlockedSkills.Clear();
        _skillPoints += refundedPoints;
        return refundedPoints;
    }
    public bool CanEquipToSlot(SkillData skill, int slotIndex)
    {
        if (skill == null) return false;

        if (slotIndex == 0 && (skill.ID == 1 || skill.ID == 3 || skill.ID == 4))
        {
            return true;
        }
        else if (slotIndex == 1 && (skill.ID == 2 || skill.ID == 4 || skill.ID == 5))
        {
            return true;
        }

        return false;
    }
    public SkillData GetEquippedSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PlayerSkillController.instance.equippedSkills.Length)
            return null;

        return PlayerSkillController.instance.equippedSkills[slotIndex].skillData;
    }
}