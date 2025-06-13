using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : NetworkBehaviour
{
    [Header(" Components ")]
    public Player player;

    [Header("KeyCode")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("NormalAttack")]
    public float attack = 5;
    public float waitTime = 0.5f;
    private bool canAtk = false;
    private Animator animator;

    [Header("FarAttack")]
    public GameObject ATKPrefab;
    public float FarAtk = 5;
    [SerializeField] private LayerMask groundMask;

    [Header("Critical Strike Settings")]
    [Range(0, 1)] public float criticalRate = 0.2f;       // 默認 20% 暴擊率
    public float criticalMultiplier = 1.5f;              // 默認暴擊傷害 150%
    public GameObject criticalEffectPrefab;              // 暴擊特效（可選）
    public AudioClip criticalSound;                      // 暴擊音效（可選）

    [SerializeField] private AudioClip NrmATK_SFX;
    [SerializeField] private AudioClip FarATK_SFX;
    // Ability
    [HideInInspector]
    public float AbilityATKPlus = 0;
    [HideInInspector]
    public float AbilityATKSpeedPlus = 0;
    [HideInInspector]
    public float AbilityATKArea = 0;
    [HideInInspector]
    private BoxCollider ATKCollider;

    private void Start()
    {
        if (!isLocalPlayer) return;
        animator = GetComponent<Animator>();
        canAtk = true;
        if(player == null)
            player = GetComponentInParent<Player>();
        ATKCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if(!isLocalPlayer) return;

        if (inputActions.FindAction(Constraints.InputKey.FarAttack).triggered && canAtk)
        {
            player.animator.SetTrigger("Attack");
            FarAttack();
            if(FarATK_SFX!=null&&SoundManager.instance!=null)
                SoundManager.instance.PlaySFX(FarATK_SFX);
        }
    }

    public void OnTriggerStay(Collider c)
    {
        if (!isLocalPlayer) return;
        if (inputActions.FindAction(Constraints.InputKey.Attack).triggered && canAtk)
        {
            NrmATK(c);
            if (NrmATK_SFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySFX(NrmATK_SFX);
        }
    }

    public void IncreaseATKArea()
    {
        if (ATKCollider == null) return;
        ATKCollider.size *= 1 + AbilityATKArea;
    }

    private void DetectOnce()
    {
        canAtk = true;
    }

    private float CalculateDamage(bool isFarAttack = false)
    {
        float baseDamage = isFarAttack ? FarAtk : attack;

        float attackBuff = PlayerBuffSystem.instance.GetBuffValue(BuffType.AttackPowerUp);
        float buffedDamage = baseDamage * (1 + attackBuff + AbilityATKPlus);

        bool isCritical = CheckCriticalHit();
        float finalDamage = buffedDamage * (isCritical ? criticalMultiplier : 1f);

        finalDamage *= Random.Range(0.9f, 1.1f);

        if (isCritical) TriggerCriticalEffects();

        return finalDamage;
    }

    private bool CheckCriticalHit()
    {
        // 基礎暴擊率 + Buff 加成（例如 Buff 增加 10% → 0.2 + 0.1 = 30%）
        float totalCriticalRate = criticalRate +
            PlayerBuffSystem.instance.GetBuffValue(BuffType.CriticalRateUp);
        return Random.Range(0f, 1f) <= totalCriticalRate;
    }

    private void TriggerCriticalEffects()
    {
        // 播放特效
        if (criticalEffectPrefab != null)
            Instantiate(criticalEffectPrefab, transform.position, Quaternion.identity);

        // 播放音效
        if (criticalSound != null)
            SoundManager.instance.PlaySFX(criticalSound);

        Debug.Log("<color=yellow>暴擊！</color>");
    }

    void NrmATK(Collider c)
    {
        canAtk = false;
        float finalDamage = CalculateDamage();

        float cooldown = waitTime * (1 - AbilityATKSpeedPlus - PlayerBuffSystem.instance.GetBuffValue(BuffType.CooldownLower));

        LeanTween.delayedCall(cooldown, DetectOnce);  

        player.animator.SetTrigger("NrmAtk");
        animator.SetTrigger("normalATK");

        if (c.gameObject.GetComponent<IAttackable>() != null){
            c.gameObject.GetComponent<IAttackable>().TakeDamage(gameObject.transform.position, finalDamage);
            player.GetSP(false);
        }
    }

    void FarAttack()
    {
        canAtk = false;

        float finalDamage = CalculateDamage(true);

        float cooldownTime = waitTime * (1 - AbilityATKSpeedPlus - PlayerBuffSystem.instance.GetBuffValue(BuffType.CooldownLower));

        LeanTween.delayedCall(cooldownTime, DetectOnce);

        if (ATKPrefab == null) { Debug.Log("No Far Attack Prefab."); return; }

        if (player.canUseSkill(FarAtk))
        {
            GameObject vfx = Instantiate(ATKPrefab, transform.position, Quaternion.identity);
            if (vfx.GetComponent<Bomb>() != null)
            {
                Bomb newBomb = vfx.GetComponent<Bomb>();
                newBomb.damage = finalDamage;
                newBomb.groundMask = groundMask;
                newBomb.gameObject.transform.localScale *= (1+AbilityATKArea);
                newBomb.SetTrapTypeBomb(transform);
            }
        }
    }
}