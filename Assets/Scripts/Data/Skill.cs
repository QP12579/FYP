using UnityEngine;

public enum SkillType
{
    ATK,    // Attack
    Buff,   // Buff
    DeBuff,
    DFN,    // Defense
    Heal,   // Healing
    DTC     // Detection
}

public enum AttackType
{
    AOE,
    Single,
    Follow
}

public enum BuffType
{
    PowerUP,
    DropUP
}

public enum DeBuffType
{
    Poison,
    Dizziness,
    Blooding,
    Slow,
    Blind,

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
}