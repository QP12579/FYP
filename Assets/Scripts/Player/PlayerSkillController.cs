using UnityEngine;
using System.Collections.Generic;

public class PlayerSkillController : Singleton<PlayerSkillController>
{
    [System.Serializable]
    public class EquippedSkill
    {
        public KeyCode activationKey;
        public SkillData skillData;
        public GameObject skillPrefab;
        public float cooldownTimer;
    }

    [Header("Skill Slots")]
    public EquippedSkill[] equippedSkills = new EquippedSkill[2];

    [Header("References")]
    public Transform skillSpawnPoint;
    public SkillManager skillManager;

    private bool canShoot = true;
    private void Start()
    {
        canShoot = true;
        // Initialize with default keys (Q and E)
        if(equippedSkills[0].activationKey == null)
        equippedSkills[0].activationKey = KeyCode.Q;
        if(equippedSkills[1].activationKey == null)
        equippedSkills[1].activationKey = KeyCode.E;
    }

    private void Update()
    {
        // Check for skill activation
        for (int i = 0; i < equippedSkills.Length; i++)
        {
            if (equippedSkills[i].skillData == null) continue;

            // Update cooldown
            if (equippedSkills[i].cooldownTimer > 0)
            {
                canShoot = false;
                equippedSkills[i].cooldownTimer -= Time.deltaTime;
                continue;
            }else canShoot = true;

            // Check key press
            if (Input.GetKeyDown(equippedSkills[i].activationKey)&&canShoot)
            {
                ActivateSkill(i);
            }
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

        // Instantiate the skill prefab
        GameObject skillInstance = Instantiate(
            equippedSkill.skillPrefab,
            skillSpawnPoint.position,
            skillSpawnPoint.rotation
        );

        // Set cooldown
        equippedSkill.cooldownTimer = equippedSkill.skillData.cooldown;

        canShoot = false;
        // Optional: Add skill behavior based on type
        switch (equippedSkill.skillData.types[0])
        {
            case SkillType.ATK:
                skillInstance.GetComponent<AttackSkill>().Initialize(equippedSkill.skillData.power);
                break;
            case SkillType.Heal:
                skillInstance.GetComponent<HealSkill>().Initialize(equippedSkill.skillData.power);
                break;
                // Add other skill types as needed
        }
    }

    private GameObject GetPrefabForSkill(SkillData skillData)
    {
        // Load from Resources or use a dictionary
        // Example: return Resources.Load<GameObject>($"Skills/{skillData.Name}");

        // For now, return a default prefab
        return Resources.Load<GameObject>($"Skills/Prefabs/{skillData.Name}");
    }
}