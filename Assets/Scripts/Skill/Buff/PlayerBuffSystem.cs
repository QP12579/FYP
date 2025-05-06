using System;
using System.Collections.Generic;
using UnityEngine;
using static BuffDataManager;

    [System.Serializable]
    public class Buff
    {
        public BuffType type;
        public float value;     // 效果數值 (百分比或固定值)
        public float duration;  // 持續時間
        public float timer;     // 剩餘時間
    }

public class PlayerBuffSystem : Singleton<PlayerBuffSystem>
{
    [System.Serializable]
    public class ActiveBuff
    {
        public BuffType type;
        public float value;
        public float duration;
        public float timer;
        public int level;
    }

    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
    private Dictionary<BuffType, int> buffLevels = new Dictionary<BuffType, int>();

    private Player player;
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerAttack playerAttack;
    
    private bool hasActiveBuffs = false;
    private bool hasRegenBuffs = false;

    public static event Action OnBuffsUpdated;

    private void Start()
    {
        GetPlayerInfo();
    }

    private void GetPlayerInfo()
    {
        if(player == null)
        player = FindObjectOfType<Player>();
        if (movement == null)
            movement = FindObjectOfType<PlayerMovement>();
        if (skillController == null)
            skillController = FindObjectOfType<PlayerSkillController>();
        if (playerAttack == null)
            playerAttack = FindObjectOfType<PlayerAttack>();
        if (player == null || movement == null || skillController == null || playerAttack == null)
            LeanTween.delayedCall(0.1f, GetPlayerInfo);
        else
            Debug.Log("PlayerBuffSystem Reference ready.");
    }

    // Main method to add buff (auto-levels)
    public void AddBuff(BuffType type)
    {
        BuffData data = BuffDataManager.instance.GetBuffData(type);
        if (data == null) return;

        // Auto-level logic
        int currentLevel = GetCurrentLevel(type);
        int newLevel = Mathf.Min(currentLevel + 1, data.maxLevel);
        float effectValue = data.values[newLevel - 1];

        // Apply or refresh buff
        ActiveBuff existingBuff = activeBuffs.Find(b => b.type == type);
        if (existingBuff != null)
        {
            existingBuff.value = effectValue;
            existingBuff.timer = data.duration;
            existingBuff.level = newLevel;
        }
        else
        {
            activeBuffs.Add(new ActiveBuff
            {
                type = type,
                value = effectValue,
                duration = data.duration,
                timer = data.duration,
                level = newLevel
            });
        }

        buffLevels[type] = newLevel;
        ApplyBuffEffect(type, effectValue);
    }

    private void ApplyBuffEffect(BuffType type, float value)
    {
        switch (type)
        {
            case BuffType.AttackPowerUp:
                skillController.BuffApplyATKDamage(value);
                break;
            case BuffType.HPRegen:
                player.BuffHPRegen(value);
                break;
            case BuffType.MPRegen:
                player.BuffMPRegen(value);
                break;
            case BuffType.MoveSpeedUp:
                movement.SpeedUp(value);
                break;
            case BuffType.ItemDropUp:
                break;
            case BuffType.DoubleCoinDropUp:
                break;
        }
    }

    private int GetCurrentLevel(BuffType type)
    {
        return buffLevels.ContainsKey(type) ? buffLevels[type] : 0;
    }

    private void Update()
    {
        UpdateActiveBuffs();
    }

    private void UpdateActiveBuffs()
    {
        // Update active buff timers
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].timer -= Time.deltaTime;
            if (activeBuffs[i].timer <= 0)
            {
                RemoveBuff(activeBuffs[i].type);
                activeBuffs.RemoveAt(i);
            }
        }
    }

    private void RemoveBuff(BuffType type)
    {
        switch (type)
        {
            case BuffType.AttackPowerUp:
                skillController.ResetBuffATKDamage();
                break;
            case BuffType.MoveSpeedUp:
                movement.ResetSpeed();
                break;
            case BuffType.ItemDropUp:
                break;
            case BuffType.DoubleCoinDropUp:
                break;
            default:
                break;
        }
        buffLevels.Remove(type);
    }

    public float GetBuffValue(BuffType type)
    {
        var buff = activeBuffs.Find(b => b.type == type);
        return buff != null ? buff.value : 0;
    }

    public void ResetBuffLevel(BuffType type)
    {
        if (buffLevels.ContainsKey(type))
        {
            buffLevels[type] = 0; 
        }

        // 2. 立即移除當前生效的該類型 Buff
        ActiveBuff activeBuff = activeBuffs.Find(b => b.type == type);
        if (activeBuff != null)
        {
            RemoveBuff(type); // 會觸發對應的 Reset 邏輯
            activeBuffs.Remove(activeBuff);
        }

        Debug.Log($"已重置 {type} 的等級");
    }
}