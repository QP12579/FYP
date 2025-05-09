using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AbilityCase
{
    public int id;
    [HideInInspector] public int currentLevel = 0;
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

public class AbilitySystem : MonoBehaviour
{
    [Header("Data")]
    public TextAsset abilityTSV;

    private string abilityDataPath = "Ability/AbilityData";

    [SerializeField]
    private List<AbilityCase> AbilityCases;

    // References
    private Player player;
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        AbilityCases = new List<AbilityCase>();
        if (abilityTSV == null)
            abilityTSV = Resources.Load<TextAsset>(abilityDataPath);

        if (abilityTSV != null)
        {
            LoadAbilitysFromTSV();
        }
        else
        {
            Debug.LogError($"Failed to load ability TSV file at path: {abilityDataPath}");
        }
    }

    public bool IsAbilityTSVLoadedCompletely()
    {
        if (AbilityCases == null || AbilityCases.Count < 8)
            return false;
        return true;
    }

    private void LoadAbilitysFromTSV() // id \t description1, description2, level, power1, power2 ...
    {
        string[] lines = abilityTSV.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i].Trim())) continue;

            string[] fields = lines[i].Split('\t');

            AbilityCase abilityData = new AbilityCase()
            {
                id = int.Parse(fields[0]),
                currentLevel = 0,
                description1 = fields[1],
                description2 = fields[2],
                levelValues = new List<BaseValueLevel>()
            };

            for (int j = 3; j < fields.Length; j += 3)
            {
                if (j + 2 >= fields.Length) break;
                BaseValueLevel valueLevel = new BaseValueLevel()
                {
                    level = int.Parse(fields[j]),
                    power1 = float.Parse(fields[j + 1]),
                    power2 = float.Parse(fields[j + 2]),
                };
                abilityData.levelValues.Add(valueLevel);
            }
            AbilityCases.Add(abilityData);
        }
    }

    void Start()
    {
        CheckAbilityReference();
    }

    private void CheckAbilityReference()
    {
        if (player == null)
            player = FindObjectOfType<Player>();
        if (movement == null)
            movement = FindObjectOfType<PlayerMovement>();
        if (skillController == null)
            skillController = FindObjectOfType<PlayerSkillController>();
        if (playerAttack == null)
            playerAttack = FindObjectOfType<PlayerAttack>();
        if (player == null || movement == null || skillController == null || playerAttack == null)
            LeanTween.delayedCall(0.5f, CheckAbilityReference);
    }

    // Ability UI Button information
    public UIValue GetAbilityValue(int id, int level)
    {
        int displayLevel = level == 0 ? 1 : level;
        if (AbilityCases == null || AbilityCases.Count == 0)
        {
            Debug.LogError("AbilityCases is not initialized or empty!");
            return CreateDefaultUIValue(id);
        }
        AbilityCase value = AbilityCases.Find(a => a != null && a.id == id);
        if (value == null)
        {
            Debug.LogError($"Ability with id {id} not found!");
            return CreateDefaultUIValue(id);
        }
        if (value.levelValues == null || value.levelValues.Count == 0)
        {
            Debug.LogError($"LevelValues for ability {id} is not initialized or empty!");
            return CreateDefaultUIValue(id);
        }
        if (value.currentLevel >= value.levelValues.Count)
        {
            Debug.LogWarning($"Current level {value.currentLevel} exceeds max level for ability {id}");
            return CreateDefaultUIValue(id);
        }

        BaseValueLevel levelvalue = value.levelValues.Find(l => l != null && l.level == displayLevel);
        if (levelvalue == null)
        {
            Debug.LogError($"Level {level} data for ability {id} not found!");
            return CreateDefaultUIValue(id);
        }
        UIValue UI = new UIValue();
        UI.level = displayLevel;
        UI.maxLevel = value.levelValues.Count;

        // description
        switch (id)
        {
            // d +% d +%
            case 1:
            case 2:
                UI.description = $"{value.description1} +{levelvalue.power1}%, \n{value.description2} +{levelvalue.power2}%";
                break;
            // d+% d-%
            case 3:
            case 4:
            case 6:
                UI.description = $"{value.description1} +{levelvalue.power1}%, \n{value.description2} -{levelvalue.power2}%";
                break;
            // d +% d
            case 5:
            case 8:
                UI.description = $"{value.description1} +{levelvalue.power1}% {value.description2}";
                break;
            // d +% d n per second
            case 7:
                UI.description = $"{value.description1} -{levelvalue.power1}%, \n{value.description2} {levelvalue.power2} per second";
                break;
            default:
                UI.description = "Unknown ability type";
                break;
        }
        return UI;
    }

    public void UpgradeBaseValue(int id, int level)
    {
        AbilityCase value = AbilityCases.Find(a => a.id == id);
        value.currentLevel = level;
        BaseValueLevel powers = GetLevelValue(value, level);

        float p1 = powers.power1 / 100;
        float p2 = powers.power2 / 100;
        switch (id)
        {
            case 1:
                UpATKDamage(p1);
                UpMoveSpeed(p2);
                break;
            case 2:
                UpSkillDamage(p1);
                UpMoveSpeed(p2);
                break;
            case 3:
                UpATKDamage(p1);
                UpSkillDamage(p1);
                autoDefense(p2);
                break;
            case 4:
                UpATKSpeed(p1);
                decreaseSkillCooldown(p2);
                break;
            case 5:
                UpRollSpeed(p1);
                break;
            case 6:
                UpDefense(p1, p2);
                break;
            case 7:
                DecreaseMP(p1);
                AutoFillMP(powers.power2);
                break;
            case 8:
                IncreaseATKArea(p1);
                break;
        }
    }

    private UIValue CreateDefaultUIValue(int id)
    {
        UIValue defaultUI = new UIValue();
        defaultUI.level = 0;
        defaultUI.maxLevel = 3;
        defaultUI.description = $"Ability {id} data not available";
        return defaultUI;
    }

    private BaseValueLevel GetLevelValue(AbilityCase value, int level)
    {
        BaseValueLevel powerValues = value.levelValues.Find(a => a.level == level);
        if (value.currentLevel <= 1) return powerValues;

        BaseValueLevel oldValues = new BaseValueLevel();
           oldValues = value.levelValues.Find(a => a.level == (level - 1));
        float power1 = powerValues.power1 - oldValues.power1;
        float power2 = powerValues.power2 - oldValues.power2;

        BaseValueLevel newValues = new BaseValueLevel()
        {
            level = level,
            power1 = power1,
            power2 = power2,
        };

        return newValues;
    }

    private void UpATKDamage(float p)
    {
        playerAttack.AbilityATKPlus += p;
        Debug.Log($"AbilityATKPlus:{playerAttack.AbilityATKPlus}");
    }
    private void UpSkillDamage(float p)
    {
        skillController.AbilitySkillDamagePlus += p;
        Debug.Log($"AbilitySkillDamagePlus:{skillController.AbilitySkillDamagePlus}");
    }

    private void UpMoveSpeed(float p)
    {
        movement.AbilitySpeedUp(p);
        Debug.Log($"MoveSpeed:{p}");
    }

    private void autoDefense(float p)
    {
        player.abilityDamageReduction += p;
        Debug.Log($"Defense:{player.abilityDamageReduction}");
    }

    private void UpATKSpeed(float p)
    {
        playerAttack.AbilityATKSpeedPlus += p;
        Debug.Log($"UpATKSpeed:{playerAttack.AbilityATKSpeedPlus}");
    }

    private void decreaseSkillCooldown(float p)
    {
        skillController.AbilitySkillSpeedPlus += p;
        Debug.Log($"AbilitySkillSpeedPlus:{skillController.AbilitySkillSpeedPlus}");
    }
    private void UpRollSpeed(float p)
    {
        movement.abilityRollingSpeed += p;
        Debug.Log($"abilityRollingSpeed:{movement.abilityRollingSpeed}");
    }

    private void UpDefense(float perfectp, float normalp)
    {
        player.abilityPerfectDefenceluck += perfectp;
        player.abilityNormalDefencePlus += normalp;
        Debug.Log($"abilityPerfectDefenceluck:{player.abilityPerfectDefenceluck}\nabilityNormalDefencePlus:{player.abilityNormalDefencePlus}");
    }

    private void DecreaseMP(float p)
    {
        player.abilityDecreaseMP += p;
        Debug.Log($"abilityDecreaseMP:{player.abilityDecreaseMP}");
    }

    private void AutoFillMP(float p)
    {
        p -= 1;
        if (p <= 0) player.abilityAutoFillMP = 0;
        else player.abilityAutoFillMP += p;
        Debug.Log($"AutoFillMP: {player.abilityAutoFillMP}");
    }

    private void IncreaseATKArea(float p)
    {
        if (p <= 0)
        {
            playerAttack.AbilityATKArea = 0;
            skillController.AbilitySkillSizePlus = 0;
        }
        else
        {
            playerAttack.AbilityATKArea += p;
            skillController.AbilitySkillSizePlus += p;
        }
        playerAttack.IncreaseATKArea();
    }

    public int RevertUpgrade(int id, int currentLevel)
    {
        AbilityCase value = AbilityCases.Find(a => a.id == id);
        if (value == null || currentLevel <= 0) return 0;

        int pointsToRefund = currentLevel;
        value.currentLevel = 0;

        //
        playerAttack.AbilityATKPlus = 0;
        skillController.AbilitySkillDamagePlus = 0;
        movement.ResetAbilitySpeed();
        player.abilityDamageReduction = 0;
        playerAttack.AbilityATKSpeedPlus = 0;
        skillController.AbilitySkillSpeedPlus = 0;
        player.abilityPerfectDefenceluck = 0;
        player.abilityNormalDefencePlus = 0;
        player.abilityDecreaseMP = 0;
        AutoFillMP(0);


        //
        return pointsToRefund;
    }

    private float GetTotalPower(AbilityCase ability, int upToLevel, int powerIndex)
    {
        float total = 0f;
        for (int i = 0; i < upToLevel && i < ability.levelValues.Count; i++)
        {
            var levelData = ability.levelValues[i];
            total += (powerIndex == 1) ? levelData.power1 : levelData.power2;
        }
        return total;
    }
}
