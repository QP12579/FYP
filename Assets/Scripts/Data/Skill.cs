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
    AttackPowerUp,      // 攻擊力提升
    DamageReduction,    // 抗傷
    MaxHPUp,            // HP最大值提升
    MaxMPUp,            // MP最大值提升
    HPRegen,            // HP自動回復
    MPRegen,            // MP自動回復
    MoveSpeedUp,        // 移動速度提升
    CooldownLower,      // 冷卻縮減
    CriticalRateUp,     // 暴擊率提升
    CriticalDamageUp,
    DodgeRateUp,         // 迴避率提升
    ItemDropUp,
    DoubleCoinDropUp
}

public enum DeBuffType
{
    Blooding,
    Dizziness,
    Slow
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
    public int MP = 5;
    public float cooldown = 5f;
}