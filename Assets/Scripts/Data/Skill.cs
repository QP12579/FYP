public class Skill
{
    public SkillData skillData;
    public SkillData nextLevelSkillData;
    public bool locked = true;
}

[System.Serializable]
public class SkillData
{
    public string id;
    public int level;
    public string name;
    public string description;
    public float power;
    public string IconPath;

}
