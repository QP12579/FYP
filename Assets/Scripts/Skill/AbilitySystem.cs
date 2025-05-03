using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityCase
{
    public int id;
    public int currentLevel = 0;
    public string description1;
    public string description2;
    public List<BaseValueLevel> levelValues;
}

[System.Serializable]
public class BaseValueLevel
{
    public int level;
    public float power1;
    public float power2;
}

public class UIValue
{
    public int level;
    public int maxLevel;
    public string description;
}

public enum AbilityType
{
    ATKDamage,
    SkillDamage,
    MoveSpeed,
    TakeFewerDamage,
    ATKSpeed,
    SkillCoolDown,
    RollSpeed,
    Defense,
}

public class AbilitySystem : Singleton<AbilitySystem>
{
    [Header("Data")]
    [SerializeField]
    private List<AbilityCase> AbilityCases;

    // References
    private Player player;
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerAttack playerAttack;

    // Start is called before the first frame update
    void Start()
    {
        //player = Player.instance;
        movement = FindObjectOfType<PlayerMovement>();
        skillController = PlayerSkillController.instance;
        playerAttack = FindObjectOfType<PlayerAttack>();
    }

    // Ability UI Button information
    public UIValue GetAbilityValue(int id, int level)
    {
        AbilityCase value = AbilityCases.Find(a => a.id == id);
        if (value.currentLevel >= value.levelValues.Count) return null;
        BaseValueLevel levelvalue = value.levelValues.Find(l => l.level == level);

        UIValue UI = new UIValue();
            UI.level = value.currentLevel + 1;
            UI.maxLevel = value.levelValues.Count;
        // description
        switch (id)
        { 
            // d +% d +%
            case 1:
            case 2:
            UI.description = value.description1 + " +" + levelvalue.power1 + "%, " + 
                             value.description2 + " +" + levelvalue.power2 + "%";
            break;
            // d+% d-%
            case 3:
            case 4:
            case 6:
            UI.description = value.description1 + " +" + levelvalue.power1 + "%, " + 
                             value.description2 + " -" + levelvalue.power2 + "%";
                break;
            // d +% d
            case 5:
            case 8:
            UI.description = value.description1 + " +" + levelvalue.power1 + "%" + 
                             value.description2;
                break;
            // d +% d n per second
            case 7:
                UI.description = value.description1 + " -" + levelvalue.power1 + "%, "+
                                 value.description2 + levelvalue.power2 + " per second";
                break;
        }
        return UI;
    } 

    public void UpgradeBaseValue(int id, int level)
    {
        AbilityCase value = AbilityCases.Find(a => a.id == id);
        value.currentLevel = level;
        BaseValueLevel powers = GetLevelValue(value, level);

        switch (id)
        {
            case 1:
                UpATKDamage(powers.power1);
                UpMoveSpeed(powers.power2);
                break;
                case 2:
                UpSkillDamage(powers.power1);
                UpMoveSpeed(powers.power2);
                break;
                case 3:
                UpATKDamage(powers.power1);
                UpSkillDamage(powers.power1);
                autoDefense(powers.power2);
                break;
                case 4:
                UpATKSpeed(powers.power1);
                decreaseSkillCooldown(powers.power2);
                break;
                case 5:
                UpRollSpeed(powers.power1);
                break;
                case 6:
                UpDefense(powers.power1, powers.power2);
                break;
                case 7:
                DecreaseMP(powers.power1);
                AutoFillMP(powers.power2);
                break;
                case 8:
                IncreaseATKArea(powers.power1);
                break;
        }
    }

    private BaseValueLevel GetLevelValue(AbilityCase value, int level)
    {
        BaseValueLevel powerValues = new BaseValueLevel();
            powerValues = value.levelValues.Find(a => a.level == level);
        if(value.currentLevel <= 1)   return powerValues;
        BaseValueLevel oldValues = value.levelValues.Find(a => a.level == level - 1);
        powerValues.power1 -= oldValues.power1;
        powerValues.power2 -= oldValues.power2;

        return powerValues;
    }

    private void UpATKDamage(float p)
    {
        playerAttack.AbilityATKPlus += p;
    }
    private void UpSkillDamage(float p) 
    {
        skillController.AbilitySkillDamagePlus += p;
    }

    private void UpMoveSpeed(float p) 
    {
        movement.SpeedUp(p);
    }

    private void autoDefense(float p)
    {
        player.abilityAutoDefence += p;
    }

    private void UpATKSpeed(float p) 
    { 
        playerAttack.AbilityATKSpeedPlus += p;
    }
    private void UpSkillSpeed(float p) {
        skillController.AbilitySkillSpeedPlus += p;
    }
    private void decreaseSkillCooldown(float p)
    {
        skillController.AbilitySkillSpeedPlus += p;
    }
    private void UpRollSpeed(float p)
    {
        movement.abilityRollingSpeed += p;
    }

    private void UpDefense(float perfectp, float normalp)
    {
        player.abilityPerfectDefenceluck += perfectp;
        player.abilityNormalDefencePlus += normalp;
    }

    private void DecreaseMP(float p)
    {
        player.abilityDecreaseMP += p;
    }

    private void AutoFillMP(float p)
    {
        player.abilityAutoFillMP+= p;
    }

    private void IncreaseATKArea(float p)
    {
        playerAttack.AbilityATKArea += p;
        skillController.AbilitySkillSizePlus += p;
        playerAttack.IncreaseATKArea();
    }
}
