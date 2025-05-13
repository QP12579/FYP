using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    [Header("HP MP SP")]
    public int MaxHP = 100;
    [HideInInspector]
    public float HP = 100;

    public float MaxMP = 50;

    public float CurrentMaxHP => MaxHP * (1 + PlayerBuffSystem.instance.GetBuffValue(BuffType.MaxHPUp));
    public float CurrentMaxMP => MaxMP * (1 + PlayerBuffSystem.instance.GetBuffValue(BuffType.MaxMPUp));
    [HideInInspector]
    public float MP = 50;
    //[HideInInspector]
    public float SP = 0;

    public int level = 1;

    [Header(" Audio Clip")]
    [SerializeField] private AudioClip UseSP_SFX;
    [SerializeField] private AudioClip GetHurt_SFX;
    [SerializeField] private AudioClip Die_SFX;
    [SerializeField] private AudioClip Defense_SFX;
    [SerializeField] private AudioClip Heal_SFX;

    [SyncVar]
    private float speedModifier = 1.0f;

    private PlayerMovement move;
    private PersistentUI persistentUI;
    [HideInInspector] public Animator animator;

    // Defense
    [HideInInspector] public float abilityPerfectDefenceluck = 0;
    [HideInInspector] public float abilityNormalDefencePlus = 0;
    [HideInInspector] public float abilityDamageReduction = 0;

    //MP
    [HideInInspector] public float abilityDecreaseMP = 0;
    [HideInInspector] public float abilityAutoFillMP = 0;

    private void Start()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        if (move == null)
            move = GetComponentInChildren<PlayerMovement>();
        if (animator == null)
            animator = GetComponent<Animator>();
        if (persistentUI == null)
            persistentUI = FindAnyObjectByType<PersistentUI>();


        if (move == null || animator == null)
            LeanTween.delayedCall(0.1f, InitializeUI);
        else
        {
            LeanTween.delayedCall(1f, AutoFillMP);
        }
    }
    [Command]
    public void CmdDamageEnemy(uint enemyNetId, Vector3 hitPosition, float damageAmount)
    {
        // Find the enemy with this netId
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            if (enemy.NetId == enemyNetId)
            {
                // Apply damage on the server
                enemy.TakeDamage(hitPosition, damageAmount);
                return;
            }
        }

        Debug.LogWarning($"Could not find enemy with netId {enemyNetId} to damage");
    }
    private void Update()
    {
        SP = Mathf.Clamp(SP, 0, 1f);
        if (isLocalPlayer)
            UpdatePlayerUIInfo();

        /////////////////////////  C H E A T //////////////////////////////////
        if (Input.GetKeyDown(KeyCode.G) && isLocalPlayer)
        {
            SkillManager.instance.AddSkillPoints(1);

            Debug.Log("Added SKill points");
        }
        if (Input.GetKeyDown(KeyCode.H) && isLocalPlayer)
         {
            Bag.instance.AddCoins(10);
         }

    }

    public Player()
    {
        level = 1;
        HP = MaxHP;
        MP = MaxMP;
    }

    public void UpdatePlayerUIInfo()
    {
        if (persistentUI == null)
            persistentUI = gameObject.GetOrAddComponent<PersistentUI>();
        if (persistentUI != null)
            persistentUI.UpdatePlayerUI(HP, CurrentMaxHP, MP, CurrentMaxMP, SP, level);

        
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        if (!isLocalPlayer) return;

        float time = Time.time;
        if (move.blockTimes > time && (move.blockTimes - move.defenceTime) < time)
        {
            if ((move.blockTimes - 2 * (move.defenceTime * (1 + abilityPerfectDefenceluck) / 3)) > time)
            {
                Debug.Log("Perfect Block");
                if (move.isReflect && attacker != null)
                { //Reflect
                    attacker.GetComponent<IAttackable>().TakeDamage(gameObject.transform.position, damage * move.reflectDamageMultiplier);
                }
                damage = 0;
                return;
            }
            damage *= move.blockPercentage * (1 - abilityNormalDefencePlus);
            Debug.Log("Normal Block");
        }
        float realDamage = Mathf.Min(damage * (1 - abilityDamageReduction - PlayerBuffSystem.instance.GetBuffValue(BuffType.DamageReduction)), HP);
        HP -= realDamage;

        UpdatePlayerUIInfo();
        animator.SetTrigger("Hurt");

        if (GetHurt_SFX != null && SoundManager.instance!= null)
            SoundManager.instance.PlaySFX(GetHurt_SFX);

        if (HP <= 0)
            Die();
    }

    public void Die()
    {
        if (Die_SFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySFX(Die_SFX);
        Destroy(gameObject, 1f);
    }

    public void Heal(float h)
    {
        float realHill = Mathf.Min(HP + h, MaxHP);
        HP += realHill;

        if (Heal_SFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySFX(Heal_SFX);
        UpdatePlayerUIInfo();
    }

    public bool canUseSkill(float mp)
    {
        mp *= 1 - abilityDecreaseMP;
        if (mp > MP) return false;
        MP -= mp;
        UpdatePlayerUIInfo();
        return true;
    }

    public void GetMP(float mp)
    {
        float realFill = Mathf.Min(MP + mp, MaxMP);
        MP = realFill;
        UpdatePlayerUIInfo();
    }

    public float GetMP()
    {
        return MP;
    }

    public void GetSP(bool isSkillAttack = true)
    {
        SP += isSkillAttack ? 0.1f : 0.05f;
    }

    public void UseSP()
    {
        SP = 0;
        if (UseSP_SFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySFX(UseSP_SFX);
        UpdatePlayerUIInfo();
    }
    public void BuffMPRegen(float mpRegen)
    {
        if (mpRegen < 0) return;
        StartCoroutine(RegenMPRoutine(mpRegen));
    }
    private IEnumerator RegenMPRoutine(float mpRegen)
    {
        float minus = mpRegen / 10;

        for (float timer = mpRegen; timer > 0; timer -= minus)
        {
            GetMP(minus);
            yield return new WaitForSeconds(1f);
        }
    }

    public void AutoFillMP()
    {
        float mp = 1;
        mp += abilityAutoFillMP;
        float realFill = Mathf.Min(MP + mp, MaxMP);
        MP = realFill;
        UpdatePlayerUIInfo();
        LeanTween.delayedCall(1f, AutoFillMP);
    }

    //  affects another player
    [Command]
    public void CmdApplySpeedModifier(uint targetPlayerId, float modifier, float duration)
    {
        // Find target player by network ID
        if (NetworkServer.spawned.TryGetValue(targetPlayerId, out NetworkIdentity targetIdentity))
        {
            Player targetPlayer = targetIdentity.GetComponent<Player>();
            if (targetPlayer != null)
            {
                targetPlayer.RpcApplySpeedEffect(modifier, duration);
            }
        }
    }

    [ClientRpc]
    public void RpcApplySpeedEffect(float modifier, float duration)
    {
        // Store 
        speedModifier = modifier;

        // Apply 
        UpdatePlayerSpeed();

        // Schedule removal after duration
        StartCoroutine(RemoveSpeedEffectAfterDuration(duration));

    }
    private void UpdatePlayerSpeed()
    {
        if (move != null)
        {
            move.SpeedChange();
        }
    }
    private IEnumerator RemoveSpeedEffectAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        speedModifier = 1.0f;
        move.ResetSpeed();
    }






    /* // LevelUP
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Gate"))
            {
                LevelUp();
            }
        }

        void LevelUp()
        {
            //LevelManager.instance.LevelUp();
        }
    */
}