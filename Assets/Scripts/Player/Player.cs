using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class Player : NetworkBehaviour
{
    [Header("HP MP SP")]
    public int MaxHP = 100;
    [SyncVar(hook = nameof(OnHPChanged))]
    public float HP = 100;

    public float MaxMP = 50;

    public float CurrentMaxHP => MaxHP * (1 + PlayerBuffSystem.instance.GetBuffValue(BuffType.MaxHPUp));
    public float CurrentMaxMP => MaxMP * (1 + PlayerBuffSystem.instance.GetBuffValue(BuffType.MaxMPUp));
    [SyncVar(hook = nameof(OnMPChanged))]
    public float MP = 50;
    [SyncVar(hook = nameof(OnSPChanged))]
    public float SP = 0;

    [SyncVar(hook = nameof(OnLevelChanged))]
    public int level = 1;

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

    // For tracking UI initialization
    private bool uiInitialized = false;

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (isLocalPlayer)
        {
            // Set up player-specific UI for the local player
            SetupPlayerUI();
        }
    }

    private void Start()
    {
        InitializeComponents();

        if (isLocalPlayer && isClient)
        {
            // Only start MP regeneration for local player
            LeanTween.delayedCall(1f, AutoFillMP);
        }
    }

    [Client]
    private void SetupPlayerUI()
    {
        if (!isLocalPlayer) return;

        Debug.Log($"Setting up UI for player {gameObject.name}");

        // Simply find the existing UI in the scene
        // Don't instantiate new UIs for NetworkIdentities
        GameObject uiObject = GameObject.Find("UISavedPrefab");

        if (uiObject == null)
        {
            Debug.LogError("UI prefab not found in scene!");
            return;
        }

        // Just make sure the UI is active for this player
        if (!uiObject.activeSelf)
            uiObject.SetActive(true);

        // Find our PersistentUI component or add one
        persistentUI = GetComponent<PersistentUI>();
        if (persistentUI == null)
            persistentUI = gameObject.AddComponent<PersistentUI>();

        // Initialize UI for this player
        persistentUI.InitializeUI();

        Debug.Log("UI setup complete for local player");
    }

    private void InitializeComponents()
    {
        if (move == null)
            move = GetComponentInChildren<PlayerMovement>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (move == null || animator == null)
            LeanTween.delayedCall(0.1f, InitializeComponents);
    }

    private void InitializeUI()
    {
        if (!isLocalPlayer) return;

        // Try to find our PersistentUI component on this player object
        persistentUI = GetComponent<PersistentUI>();

        // If we don't have one yet, add it
        if (persistentUI == null)
        {
            persistentUI = gameObject.AddComponent<PersistentUI>();
            Debug.Log($"Added PersistentUI component to player {gameObject.name}");
        }

        // If we successfully have our UI component, initialize and update
        if (persistentUI != null)
        {
            uiInitialized = true;
            UpdatePlayerUIInfo();
            Debug.Log($"UI initialized for player {gameObject.name}");
        }
        else
        {
            // Try again later if something went wrong
            Debug.LogWarning($"Failed to initialize UI for player {gameObject.name}, retrying...");
            LeanTween.delayedCall(0.2f, InitializeUI);
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

        if (isLocalPlayer && Input.GetKeyDown(KeyCode.U))
        {
            Debug.Log("Forcing UI update");
            UpdatePlayerUIInfo();
        }
    }

    public Player()
    {
        level = 1;
        HP = MaxHP;
        MP = MaxMP;
    }

    // Hook methods for SyncVars
    void OnHPChanged(float oldValue, float newValue)
    {
        Debug.Log($"HP changed from {oldValue} to {newValue}");
        if (isLocalPlayer)
            UpdatePlayerUIInfo();
    }

    void OnMPChanged(float oldValue, float newValue)
    {
        if (isLocalPlayer && uiInitialized)
        {
            UpdatePlayerUIInfo();
        }
    }

    void OnSPChanged(float oldValue, float newValue)
    {
        if (isLocalPlayer && uiInitialized && newValue == 1f)
        {
            UpdatePlayerUIInfo();
        }
    }

    void OnLevelChanged(int oldValue, int newValue)
    {
        if (isLocalPlayer && uiInitialized)
        {
            UpdatePlayerUIInfo();
        }
    }

    [Client]
    public void UpdatePlayerUIInfo()
    {
        // Make sure we're only updating for local player
        if (!isLocalPlayer) return;

        Debug.Log($"Updating UI for player {gameObject.name}, HP: {HP}");

        // Find UI directly from the scene each time until proper initialization completes
        if (persistentUI == null)
        {
            // Try to get the component from this player first
            persistentUI = GetComponent<PersistentUI>();

            // If that doesn't work, find any PersistentUI (temporary solution)
            if (persistentUI == null)
                persistentUI = FindObjectOfType<PersistentUI>();

            Debug.Log($"Found UI: {(persistentUI != null ? "Yes" : "No")}");
        }

        if (persistentUI != null)
        {
            persistentUI.UpdatePlayerUI(HP, CurrentMaxHP, MP, CurrentMaxMP, SP, level);
        }
    }

    [Server]
    public void TakeDamage(float damage, GameObject attacker = null)
    {
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

        // UI will be updated by the SyncVar hook

        // Play hurt animation on all clients
        RpcPlayHurtAnimation();

        if (HP <= 0)
            Die();
    }

    [ClientRpc]
    private void RpcPlayHurtAnimation()
    {
        if (animator != null)
            animator.SetTrigger("Hurt");
    }

    [Server]
    public void Die()
    {
        // Implement proper network destruction
        NetworkServer.Destroy(gameObject);
    }

    [Server]
    public void Heal(float h)
    {
        float realHill = Mathf.Min(HP + h, CurrentMaxHP);
        HP = realHill;
        // UI will be updated by the SyncVar hook
    }

   
    public bool canUseSkill(float mp)
    {
        mp *= 1 - abilityDecreaseMP;
        if (mp > MP) return false;
        MP -= mp;
        // UI will be updated by the SyncVar hook
        return true;
    }

    [Server]
    public void GetMP(float mp)
    {
        float realFill = Mathf.Min(MP + mp, CurrentMaxMP);
        MP = realFill;
        // UI will be updated by the SyncVar hook
    }

    public float GetMP()
    {
        return MP;
    }

    [Server]
    public void GetSP(bool isSkillAttack = true)
    {
        SP += isSkillAttack ? 0.1f : 0.05f;
        // UI will be updated by the SyncVar hook when SP reaches 1
    }

    [Server]
    public void UseSP()
    {
        SP = 0;
        // UI will be updated by the SyncVar hook
    }

    [Server]
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

    [Client]
    public void AutoFillMP()
    {
        if (!isLocalPlayer) return;

        // Request MP regeneration from server
        CmdAutoFillMP();

        // Schedule next regeneration
        LeanTween.delayedCall(1f, AutoFillMP);
    }

    [Command]
    private void CmdAutoFillMP()
    {
        float mp = 1;
        mp += abilityAutoFillMP;
        float realFill = Mathf.Min(MP + mp, CurrentMaxMP);
        MP = realFill;
        // UI will be updated by the SyncVar hook
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