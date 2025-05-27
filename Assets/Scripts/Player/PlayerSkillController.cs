using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;
using Mirror;

public class PlayerSkillController : NetworkBehaviour
{
    public static PlayerSkillController instance { get; private set; }
    [System.Serializable]
    public class EquippedSkill
    {
        // public KeyCode activationKey;
        public SkillData skillData;
        public GameObject skillPrefab;
        [HideInInspector] public float cooldownTimer;
    }

    [SerializeField]
    private InputActionAsset inputActions;

    [Header("Skill Slots")]
    public EquippedSkill[] equippedSkills = new EquippedSkill[2];

    [Header("References")]
    public Transform skillSpawnPoint;
    public Transform BuffSpawnPoint;
    public Player player;
    public PlayerMovement move;

    [HideInInspector]
    public float AbilitySkillDamagePlus = 0;
    [HideInInspector]
    public float AbilitySkillSpeedPlus = 0;
    [HideInInspector]
    public float AbilitySkillSizePlus = 0;
    private float buffATKPlus = 0;
    private float buffSkillCooldownMinus = 0;

    private string PlayerMorT;

    private void Start()
    {

        if (!isLocalPlayer)
        {
           Destroy(this);
        }

        if (isLocalPlayer)
        {
            instance = this;
            Initialize();
        }
        
        
    }

    private void Initialize()
    {

        SkillPanel.instance.FindRefences(this);
        SkillManager.instance.FindingRefences(this);

        if (player == null)
            player = GetComponentInParent<Player>();
        if(move == null)
            move = GetComponentInChildren<PlayerMovement>();
      
        // Initialize with default keys (Q and E)
        // if(equippedSkills[0].activationKey == KeyCode.None)
        // equippedSkills[0].activationKey = KeyCode.Q;
        // if(equippedSkills[1].activationKey == KeyCode.None)
        // equippedSkills[1].activationKey = KeyCode.E;
        equippedSkills[0].cooldownTimer = 0;
        equippedSkills[1].cooldownTimer = 0;

        PlayerMorT = gameObject.name[0] == 'M' ? "Magic" : "Tech";
    }

    private void Update()
    {
        // Check key press
        // if (Input.GetKeyDown(equippedSkills[0].activationKey) && equippedSkills[0].cooldownTimer <= 0)
        if (inputActions.FindAction(Constraints.InputKey.Skill1).triggered && equippedSkills[0].cooldownTimer <= 0)
        {
            ActivateSkill(0);
        }
        // if (Input.GetKeyDown(equippedSkills[1].activationKey) && equippedSkills[1].cooldownTimer <= 0)
        if (inputActions.FindAction(Constraints.InputKey.Skill2).triggered && equippedSkills[1].cooldownTimer <= 0)
        {
            ActivateSkill(1);
        }
    }

    public void EquipSkill(int slotIndex, SkillData skillData)
    {

        if (slotIndex < 0 || slotIndex >= equippedSkills.Length)
        {
            Debug.Log("Returned??? OAO");
            return;
        }

        equippedSkills[slotIndex].skillData = skillData;
        equippedSkills[slotIndex].skillPrefab = GetPrefabForSkill(skillData);
        equippedSkills[slotIndex].cooldownTimer = 0;

        Debug.Log($"Equipped {skillData.Name} to slot {slotIndex + 1}");
    }

    private void ActivateSkill(int slotIndex)

    {
        var equippedSkill = equippedSkills[slotIndex];

        if (!player.canUseSkill(equippedSkill.skillData.MP)) return ;

        // Set cooldown
        equippedSkill.cooldownTimer = equippedSkill.skillData.cooldown * ( 1 - AbilitySkillSpeedPlus - buffSkillCooldownMinus);
        StartCoroutine(CoolDownTimer(slotIndex, equippedSkill.cooldownTimer));

        // Defense
        if (equippedSkill.skillData.types[0] == SkillType.DFN)
        {
            switch (equippedSkill.skillData.level)
            {
                case 1:  //Slide
                    move.Rolling();
                    move.BlockAttack(false, true);
                    break;
                //Dash
                case 2:
                    move.Rolling(true, equippedSkill.skillData.power * (1 + AbilitySkillDamagePlus));
                    move.BlockAttack(false, true);
                    break;
                //Reflect
                case 3:
                    move.BlockAttack(true);
                    break;
            }
            return;
        }
        if (equippedSkill.skillPrefab == null) return;
        else if (equippedSkill.skillData.types[0] == SkillType.Heal )
        {
            GameObject HealSkill = Instantiate(
                equippedSkill.skillPrefab,
                BuffSpawnPoint.position,
                BuffSpawnPoint.rotation
            );

            HealSkill.transform.SetParent(BuffSpawnPoint, false);
            HealSkill.transform.localPosition = Vector3.zero;

            HealSkill.GetComponent<HealSkill>().Initialize(equippedSkill.skillData.power);
            return;
        }
        else if (equippedSkill.skillData.types[0] == SkillType.Buff)
        {
            AddRandomBuff(equippedSkill.skillPrefab, 3);
            return;
        }

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
                if(skill == null) skill = skillInstance.AddComponent<AttackSkill>();
                skill.Initialize(equippedSkill.skillData.power *(1 + AbilitySkillDamagePlus + buffATKPlus));
                skill.SetAttackType(skillSpawnPoint);
                if (equippedSkill.skillData.types.Length > 1 && equippedSkill.skillData.types[1] == SkillType.DeBuff)
                    skill.HaveDebuff();
                break;
            default:
                break;
        }
    }
    private List<T> GetRandomElements<T>(T[] array, int count)
    {

        List<T> list = new List<T>(array);
        List<T> result = new List<T>();

        while (result.Count < count && list.Count > 0)
        {
            int index = Random.Range(0, list.Count);
            result.Add(list[index]);
            list.RemoveAt(index); 
        }
        return result;
    }

    private GameObject GetPrefabForSkill(SkillData skillData)
    {
 
        // Load from Resources or use a dictionary
        // Example: return Resources.Load<GameObject>($"Skills/{skillData.Name}");
        GameObject prefab = Resources.Load<GameObject>($"Skills/Prefabs/VFX_{skillData.Name}");
        if (prefab == null) prefab = Resources.Load<GameObject>($"Skills/Prefabs/VFX_{skillData.Name}_{PlayerMorT}");
        else if (prefab == null) prefab = Resources.Load<GameObject>($"Skills/Prefabs/{skillData.Name}");
        else if (prefab == null) {
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

    public void BuffApplyATKDamage(float p)
    {
        buffATKPlus = p;
    }

    public void ResetBuffATKDamage() { buffATKPlus = 0; }

    public void BuffApplySkillCooldown(float p) { buffSkillCooldownMinus = p; }

    public void ResetBuffSkillCooldown() { buffSkillCooldownMinus = 0; }

    public void AddRandomBuff(GameObject buffPrefab, int count)
    {
        if(!PlayerBuffSystem.instance.hasActiveBuffs)
        { 
        GameObject BuffSkill = Instantiate(
            buffPrefab,
            BuffSpawnPoint.position,
            BuffSpawnPoint.rotation
            );

        BuffSkill.transform.SetParent(BuffSpawnPoint, false);
        BuffSkill.transform.localPosition = Vector3.zero;

        }
        BuffType[] allBuffTypes = (BuffType[])System.Enum.GetValues(typeof(BuffType));
        List<BuffType> randomBuffs = GetRandomElements(allBuffTypes, 3);

        for (int i = 0; i < randomBuffs.Count; i++)
        {
            PlayerBuffSystem.instance.AddBuff(randomBuffs[i]);
        }        
    }
    
    public void AddRandomDebuff(GameObject debuffPrefab, int count)
    {
        if (!PlayerBuffSystem.instance.hasActiveBuffs)
        {
            GameObject BuffSkill = Instantiate(
            debuffPrefab,
            BuffSpawnPoint.position,
            BuffSpawnPoint.rotation
            );

            BuffSkill.transform.SetParent(BuffSpawnPoint, false);
            BuffSkill.transform.localPosition = Vector3.zero;
        }
        DeBuffType[] allBuffTypes = (DeBuffType[])System.Enum.GetValues(typeof(DeBuffType));
        List<DeBuffType> randomBuffs = GetRandomElements(allBuffTypes, 3);

        for (int i = 0; i < randomBuffs.Count; i++)
        {
            PlayerBuffSystem.instance.AddDeBuff(randomBuffs[i]);
        }
    }
}