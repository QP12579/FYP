using UnityEngine;
namespace Skill
{
    [System.Serializable]
    public class SkillData
    {
        public string skillName;
        public Sprite icon;
        public int skillLevel;
        public string description;
        public SkillT skillType;
        public float DamageOrHeal;
        public GameObject skillPrefab;
    }
    public enum SkillT
    {
        Arrow,
        Slide,
        Claw,
        Buff,
        Human,
        Magic,
        Technology,
        Heal
    }
}