public class Skill
{
    public SkillData skillData;
    public SkillData nextLevelSkillData;
    public bool locked = true;
}

public class SkillData
{
    public string id;
    public string name;
    public int level;
    public float power;
    public string IconPath;

}
