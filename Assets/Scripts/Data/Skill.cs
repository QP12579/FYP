using UnityEngine;

public enum SkillType
{
    ATK,    // Attack
    DFN,    // Defense
    Buff,   // Buff
    Heal,   // Healing
    DTC     // Detection
}

[System.Serializable]
public class SkillData
{
    public int ID;
    public int level;
    public Sprite Icon;
    public string Name;
    public SkillType[] types;
    public string Description;
    public float power = 1f;
    public string iconPath;
    public float cooldown = 5f;
    public string prefabPath;
}