using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffSystem : Singleton<PlayerBuffSystem>
{
    [System.Serializable]
    public class Buff
    {
        public BuffType type;
        public float value;     // 效果數值 (百分比或固定值)
        public float duration;  // 持續時間
        public float timer;     // 剩餘時間
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
        DodgeRateUp         // 迴避率提升
    }

    private Player player;
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerAttack playerAttack;
    private List<Buff> activeBuffs = new List<Buff>(); 
    
    private bool hasActiveBuffs = false;
    private bool hasRegenBuffs = false;

    private void Start()
    {
        //player = Player.instance;
        movement = FindObjectOfType<PlayerMovement>();
        skillController = FindObjectOfType<PlayerSkillController>();
        playerAttack = FindObjectOfType<PlayerAttack>();
    }

    public void AddBuff(BuffType type, float value, float duration)
    {
        var existingBuff = activeBuffs.Find(b => b.type == type);
        if (existingBuff != null)
        {
            existingBuff.timer = duration;
            existingBuff.value = Mathf.Max(existingBuff.value, value); 
        }
        else
        {
            activeBuffs.Add(new Buff
            {
                type = type,
                value = value,
                duration = duration,
                timer = duration
            });
            ApplyBuffEffect(type, value);
        }
        hasActiveBuffs = activeBuffs.Count > 0;
        hasRegenBuffs = HasRegenBuff();
    }

    private bool HasRegenBuff()
    {
        return activeBuffs.Exists(b => b.type == BuffType.HPRegen || b.type == BuffType.MPRegen);
    }

    private void ApplyBuffEffect(BuffType type, float value)
    {
        switch (type) 
        {
                case BuffType.AttackPowerUp:
                break;
            case BuffType.DamageReduction:
                break;
                case BuffType.MaxHPUp: break;
                case BuffType.MaxMPUp: break;
            case BuffType.HPRegen:
                break;
                case BuffType.MPRegen:
                break;
            case BuffType.MoveSpeedUp:
                break;
        }
    }

    private void UpdateBuffs()
    {
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
                playerAttack.attack /= (1 + GetBuffValue(type));
                break;
            case BuffType.MoveSpeedUp:
                movement.ResetSpeed();
                break;
        }
    }

    // 處理 HP/MP 自動回復
    private void HandleRegenBuffs()
    {
        float hpRegen = GetBuffValue(BuffType.HPRegen);
        float mpRegen = GetBuffValue(BuffType.MPRegen);
        if (hpRegen > 0) player.Heal(hpRegen * Time.deltaTime);
        if (mpRegen > 0) player.MP = Mathf.Min(player.MaxMP, player.MP + mpRegen * Time.deltaTime);
    }

    public float GetBuffValue(BuffType type)
    {
        var buff = activeBuffs.Find(b => b.type == type);
        return buff != null ? buff.value : 0;
    }
}