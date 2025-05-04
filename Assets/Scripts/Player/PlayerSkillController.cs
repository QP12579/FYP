using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class PlayerSkillController : MonoBehaviour
{
    public static PlayerSkillController instance;
    [System.Serializable]
    public class EquippedSkill
    {
        public KeyCode activationKey;
        public SkillData skillData;
        public GameObject skillPrefab;
        [HideInInspector] public float cooldownTimer;
    }

    [Header("Skill Slots")]
    public EquippedSkill[] equippedSkills = new EquippedSkill[2];

    [Header("References")]
    public Transform skillSpawnPoint;
    public SkillManager skillManager;
    public Player player;
    public PlayerMovement move;

    [HideInInspector]
    public float AbilitySkillDamagePlus = 0;
    [HideInInspector]
    public float AbilitySkillSpeedPlus = 0;
    [HideInInspector]
    public float AbilitySkillSizePlus = 0;


    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (player == null)
            player = GetComponentInParent<Player>();
        if(move == null)
            move = GetComponentInChildren<PlayerMovement>();
        if(skillManager == null) 
        skillManager = FindObjectOfType<SkillManager>();
        if(skillManager == null)    skillManager = gameObject.AddComponent<SkillManager>();
        // Initialize with default keys (Q and E)
        if(equippedSkills[0].activationKey == KeyCode.None)
        equippedSkills[0].activationKey = KeyCode.Q;
        if(equippedSkills[1].activationKey == KeyCode.None)
        equippedSkills[1].activationKey = KeyCode.E;
        equippedSkills[0].cooldownTimer = 0;
        equippedSkills[1].cooldownTimer = 0;
    }

    private void Update()
    {
        // Check key press
        if (Input.GetKeyDown(equippedSkills[0].activationKey) && equippedSkills[0].cooldownTimer <= 0)
        {
            ActivateSkill(0);
        }
        if (Input.GetKeyDown(equippedSkills[1].activationKey) && equippedSkills[1].cooldownTimer <= 0)
        {
            ActivateSkill(1);
        }
    }

    public void EquipSkill(int slotIndex, SkillData skillData)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Length) return;

        equippedSkills[slotIndex].skillData = skillData;
        equippedSkills[slotIndex].skillPrefab = GetPrefabForSkill(skillData);
        equippedSkills[slotIndex].cooldownTimer = 0;

        Debug.Log($"Equipped {skillData.Name} to slot {slotIndex + 1}");
    }

    private void ActivateSkill(int slotIndex)
    {
        var equippedSkill = equippedSkills[slotIndex];
        if (equippedSkill.skillPrefab == null) return;

        if (!player.canUseSkill(equippedSkill.skillData.MP)) return ;

        // Set cooldown
        equippedSkill.cooldownTimer = equippedSkill.skillData.cooldown * ( 1 - AbilitySkillSpeedPlus);
        StartCoroutine(CoolDownTimer(slotIndex, equippedSkill.cooldownTimer));

        // Instantiate the skill prefab
        GameObject skillInstance = Instantiate(
            equippedSkill.skillPrefab,
            skillSpawnPoint.position,
            skillSpawnPoint.rotation
        );

        if (AbilitySkillSizePlus > 0) skillInstance.gameObject.transform.localScale *= 1 + AbilitySkillSizePlus;

        // Optional: Add skill behavior based on type
        switch (equippedSkill.skillData.types[0])
        {
            case SkillType.ATK:
                AttackSkill skill = skillInstance.GetComponent<AttackSkill>();
                skill.Initialize(equippedSkill.skillData.power *(1 + AbilitySkillDamagePlus));
                skill.SetAttackType(skillSpawnPoint);
                if (equippedSkill.skillData.types.Length > 1 && equippedSkill.skillData.types[1] == SkillType.DeBuff)
                    skill.HaveDebuff();
                break;
            case SkillType.Heal:
                skillInstance.GetComponent<HealSkill>().Initialize(equippedSkill.skillData.power);
                break;
            case SkillType.DFN:
                Destroy(skillInstance);
                switch (equippedSkill.skillData.level)
                {
                    case 1:  //Slide
                        move.Rolling();
                        move.BlockAttack(true);
                        break;
                    //Dash
                    case 2:
                        move.Rolling(true, equippedSkill.skillData.power * (1 + AbilitySkillDamagePlus));
                        move.BlockAttack(true);
                        break;
                    //Reflect
                    case 3:
                        move.BlockAttack(true);
                        break;
                }
                break;
            default:

            
                break;
        }
    }

    private GameObject GetPrefabForSkill(SkillData skillData)
    {
        // Load from Resources or use a dictionary
        // Example: return Resources.Load<GameObject>($"Skills/{skillData.Name}");
        GameObject prefab = Resources.Load<GameObject>($"Skills/Prefabs/{skillData.Name}");
        if (prefab == null) {
            Debug.Log("No prefab.");
            return null; }
        // For now, return a default prefab
        return prefab;
    }

    IEnumerator CoolDownTimer(int i, float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        equippedSkills[i].cooldownTimer = 0;
    }
}