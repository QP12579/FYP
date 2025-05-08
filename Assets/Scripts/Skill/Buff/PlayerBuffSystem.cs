using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuffDataManager;
using static DebuffDataManager;

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

    [Serializable]
    public class ActiveDebuff
    {
        public DeBuffType type;
        public float value;
        public float duration;
        public float timer;
        public int level;
    }

    private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
    private Dictionary<BuffType, int> buffLevels = new Dictionary<BuffType, int>();

    private List<ActiveDebuff> activedebuffs = new List<ActiveDebuff>();
    private Dictionary<DeBuffType, int> debuffLevels = new Dictionary<DeBuffType, int>();

    private Dictionary<BuffType, Coroutine> _activeBuffRoutines = new Dictionary<BuffType, Coroutine>();
    private Dictionary<DeBuffType, Coroutine> _activeDebuffRoutines = new Dictionary<DeBuffType, Coroutine>();

    private Player player;
    private PlayerMovement movement;
    private PlayerSkillController skillController;
    private PlayerAttack playerAttack;

    [HideInInspector] public bool hasActiveBuffs = false;
    [HideInInspector] public bool hasActiveDebuffs = false;

    public static event Action OnBuffsUpdated;

    private void Start()
    {
        GetPlayerInfo();
    }

    private void GetPlayerInfo()
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
            LeanTween.delayedCall(0.1f, GetPlayerInfo);
        else
            Debug.Log("PlayerBuffSystem Reference ready.");
    }

    private void Update()
    {
        if (hasActiveBuffs) UpdateActiveBuffs();
        if(hasActiveDebuffs) UpdateActiveDeBuffs();
    }
    // Main method to add buff (auto-levels)
    public void AddBuff(BuffType type)
    {
        BuffData data = BuffDataManager.instance.GetBuffData(type);
        if (data == null) return;

        Debug.Log($"Get {type} buff.");

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
        hasActiveBuffs = activeBuffs.Count > 0;
    }

    private void ApplyBuffEffect(BuffType type, float value)
    {
        switch (type)
        {
            case BuffType.AttackPowerUp:
                skillController.BuffApplyATKDamage(value);
                break;
            case BuffType.HPRegen:
                ApplyHOT(type, value, 10f);
                break;
            case BuffType.MPRegen:
                player.BuffMPRegen(value);
                break;
            case BuffType.MoveSpeedUp:
                movement.SpeedChange();
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
                Debug.Log($"Removed {activeBuffs[i].type} .");
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
            case BuffType.HPRegen:
                RemoveBuffEffect(type);
                break;
            case BuffType.ItemDropUp:
                break;
            case BuffType.DoubleCoinDropUp:
                break;
            default:
                break;
        }
        buffLevels.Remove(type);
        hasActiveBuffs = activeBuffs.Count > 0;
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

        ActiveBuff activeBuff = activeBuffs.Find(b => b.type == type);
        if (activeBuff != null)
        {
            RemoveBuff(type);
            activeBuffs.Remove(activeBuff);
        }

        Debug.Log($"Reset {type} level");
    }

    // add Debuff (same with buff method)
    public void AddDeBuff(DeBuffType type)
    {
        DebuffData data = DebuffDataManager.instance.GetDeBuffData(type);
        if (data == null) return;

        Debug.Log($"Get {type} buff.");
        // Auto-level logic
        int currentLevel = GetDebuffCurrentLevel(type);
        int newLevel = Mathf.Min(currentLevel + 1, data.maxLevel);
        float effectValue = data.values[newLevel - 1];

        // Apply or refresh buff
        ActiveDebuff existingBuff = activedebuffs.Find(b => b.type == type);
        if (existingBuff != null)
        {
            existingBuff.value = effectValue;
            existingBuff.timer = data.duration;
            existingBuff.level = newLevel;
        }
        else
        {
            activedebuffs.Add(new ActiveDebuff
            {
                type = type,
                value = effectValue,
                duration = data.duration,
                timer = data.duration,
                level = newLevel
            });
        }

        debuffLevels[type] = newLevel;
        ApplyDeBuffEffect(type, effectValue);
        hasActiveDebuffs = activedebuffs.Count > 0;
    }

    private void ApplyDeBuffEffect(DeBuffType type, float value)    // apply debuff
    {
        switch (type)
        {
            case DeBuffType.Blooding:
                ApplyDOT(type, value, 10f);
                break;
            case DeBuffType.Dizziness:
                movement.Dizziness(value);
                break;
            case DeBuffType.Slow:
                movement.SpeedChange();
                break;
            default:
                break;
        }
    }

    private int GetDebuffCurrentLevel(DeBuffType type)
    {
        return debuffLevels.ContainsKey(type) ? debuffLevels[type] : 0;
    }

    private void UpdateActiveDeBuffs()
    {
        // Update active buff timers
        for (int i = activedebuffs.Count - 1; i >= 0; i--)
        {
            activedebuffs[i].timer -= Time.deltaTime;
            if (activedebuffs[i].timer <= 0)
            {
                RemoveDeBuff(activedebuffs[i].type);
                activedebuffs.RemoveAt(i);
            }
        }
    }

    private void RemoveDeBuff(DeBuffType type)  // remove debuff
    {
        switch (type)
        {
            case DeBuffType.Blooding:
                RemoveDebuffEffect(type);
                break;
            case DeBuffType.Dizziness:
                movement.canMove = true;
                break;
            case DeBuffType.Slow:
                movement.SpeedChange();
                break;
            default:
                break;
        }
        debuffLevels.Remove(type);
        hasActiveDebuffs = activedebuffs.Count > 0;
    }

    public float GetDeBuffValue(DeBuffType type)
    {
        var buff = activedebuffs.Find(b => b.type == type);
        return buff != null ? buff.value : 0;
    }

    public void ResetDeBuffLevel(DeBuffType type)
    {
        if (debuffLevels.ContainsKey(type))
        {
            debuffLevels[type] = 0;
        }

        ActiveDebuff activedebuff = activedebuffs.Find(b => b.type == type);
        if (activedebuff != null)
        {
            RemoveDeBuff(type);
            activedebuffs.Remove(activedebuff);
        }

        Debug.Log($"Reset {type} level");
    }

    // HP / MP Regen
    private void ApplyHOT(BuffType type, float totalHeal, float duration)
    {
        if (_activeBuffRoutines.ContainsKey(type))
        {
            StopCoroutine(_activeBuffRoutines[type]);
        }

        Coroutine routine = StartCoroutine(HOT_Routine(totalHeal, duration));
        _activeBuffRoutines[type] = routine;
    }

    private void ApplyDOT(DeBuffType type, float totalDamage, float duration)
    {
        if (_activeDebuffRoutines.ContainsKey(type))
        {
            StopCoroutine(_activeDebuffRoutines[type]);
        }

        Coroutine routine = StartCoroutine(DOT_Routine(totalDamage, duration));
        _activeDebuffRoutines[type] = routine;
    }

    private IEnumerator HOT_Routine(float totalHeal, float duration)
    {
        float interval = 1f;
        float healPerTick = totalHeal / (duration / interval);

        for (float t = 0; t < duration; t += interval)
        {
            player.Heal(healPerTick);
            yield return new WaitForSeconds(interval);
        }

        _activeBuffRoutines.Remove(BuffType.HPRegen);
    }
    private IEnumerator DOT_Routine(float totalDamage, float duration)
    {
        float interval = 1f;
        float damagePerTick = totalDamage / (duration / interval) * -1;

        for (float t = 0; t < duration; t += interval)
        {
            player.Heal(damagePerTick);
            yield return new WaitForSeconds(interval);
        }

        _activeDebuffRoutines.Remove(DeBuffType.Blooding);
    }
    private void RemoveBuffEffect(BuffType type)
    {
        if (_activeBuffRoutines.TryGetValue(type, out Coroutine routine))
        {
            StopCoroutine(routine);
            _activeBuffRoutines.Remove(type);
            Debug.Log($"Clear {type} Buff");
        }
    }
    private void RemoveDebuffEffect(DeBuffType type)
    {
        if (_activeDebuffRoutines.TryGetValue(type, out Coroutine routine))
        {
            StopCoroutine(routine);
            _activeDebuffRoutines.Remove(type);
            Debug.Log($"Clear {type} Debuff");
        }
    }

    public void ClearAllBuffEffect()
    {
        foreach (var kvp in _activeBuffRoutines)
        {
            StopCoroutine(kvp.Value);
        }
        _activeBuffRoutines.Clear();
    }
    public void ClearAllDebuffEffect()
    {
        foreach (var kvp in _activeDebuffRoutines)
        {
            StopCoroutine(kvp.Value);
        }
        _activeDebuffRoutines.Clear();
    }
}