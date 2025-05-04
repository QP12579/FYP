using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Player : NetworkBehaviour
{
    [Header("HP MP SP")]
    public int MaxHP = 100;
    [HideInInspector]
    public float HP = 100;
    //[HideInInspector]
    public float SP = 0;
    private bool isSPFullEffectAnimating = false;

    [HideInInspector]    public float CurrentMaxHP => MaxHP * (1 + PlayerBuffSystem.instance.GetBuffValue(PlayerBuffSystem.BuffType.MaxHPUp));
    [HideInInspector]    public float CurrentMaxMP => MaxMP * (1 + PlayerBuffSystem.instance.GetBuffValue(PlayerBuffSystem.BuffType.MaxMPUp));

    public float MaxMP = 50;
    [HideInInspector]
    public float MP = 50;

    public int level = 1;

    [SyncVar]
    private float speedModifier = 1.0f;

    [Header("UI")]
    public Slider HPSlider;
    public Slider MPSlider;
    public Slider SpecialAttackSlider;
    public GameObject SPFullEffect;
    public GameObject SPHintKeywords;
    public TextMeshProUGUI levelText;

    private PlayerMovement move;
    [HideInInspector] public Animator animator;

    // Defense
    [HideInInspector] public float abilityPerfectDefenceluck = 0;
    [HideInInspector] public float abilityNormalDefencePlus = 0;
    [HideInInspector] public float abilityAutoDefence = 0;

    //MP
    [HideInInspector] public float abilityDecreaseMP = 0;
    [HideInInspector] public float abilityAutoFillMP = 0;

    private void Start()
    {
        move = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        
        SetAlpha(SPFullEffect, 0f);
        SPHintKeywords.SetActive(false);
    }

    private void Update()
    {
        SP = Mathf.Clamp(SP, 0, 1f);
        SpecialAttackSlider.value = SP;

        if (SpecialAttackSlider.value == 1) 
        {
            if (!isSPFullEffectAnimating)
            {
                AnimateAlpha();
                SPHintKeywords.SetActive(true);
            }
        }
        else
        {
            if (isSPFullEffectAnimating)
            {
                DOTween.Kill(SPFullEffect);
                isSPFullEffectAnimating = false; 
            }
            SetAlpha(SPFullEffect, 0f);
            SPHintKeywords.SetActive(false);
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
       /* HPSlider.value = HP;
        MPSlider.value = MP;
        HPSlider.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = HP.ToString() + "/" + MaxHP.ToString();
        MPSlider.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = MP.ToString() + "/" + MaxMP.ToString();
        levelText.text = level.ToString();*/

        PersistentUI.Instance.UpdatePlayerUI(HP, MaxHP, MP, MaxMP, level);
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        float time = Time.time;
        if (move.blockTimes > time && (move.blockTimes - move.defenceTime) < time)
        {
            if ((move.blockTimes - 2 * (move.defenceTime * (1 + abilityPerfectDefenceluck) /3) ) > time)
            {
                Debug.Log("Perfect Block");
                if (move.isReflect && attacker != null)
                { //Reflect
                    attacker.GetComponent<IAttackable>().TakeDamage(damage * move.reflectDamageMultiplier);
                }
                damage = 0;
                return;
            }
            damage *= move.blockPercentage * (1 - abilityNormalDefencePlus);
            Debug.Log("Normal Block");
        }
        float realDamage = Mathf.Min(damage*( 1 - abilityAutoDefence), HP) ;
        HP -= realDamage;

        UpdatePlayerUIInfo();
        animator.SetTrigger("Hurt");

        if (HP <= 0)
            Die();
    }

    public void Die()
    {
        Destroy(gameObject, 1f);
    }

    public void Heal(float h)
    {
        HP += h;
        UpdatePlayerUIInfo();
    }

    public bool canUseSkill(float mp)
    {
        mp *= 1 - abilityDecreaseMP;
        if(mp > MP) return false;
        MP -= mp;
        UpdatePlayerUIInfo();
        return true;
    }

    public void FillMP(float mp)
    {
        mp += abilityAutoFillMP;
        float realFill = Mathf.Min(MP+mp, MaxMP);
        MP = realFill;
    }
    private void AnimateAlpha()
    {
        isSPFullEffectAnimating = true;

        SPFullEffect.GetComponent<CanvasGroup>().DOFade(1f, 1f).OnComplete(() =>
        {
            DOVirtual.DelayedCall(0.5f, () =>
            {
                SPFullEffect.GetComponent<CanvasGroup>().DOFade(0f, 1f).OnComplete(() =>
                {
                    isSPFullEffectAnimating = false;
                });
            });
        });
    }
    private void SetAlpha(GameObject obj, float alpha)
    {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = obj.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = alpha;
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
            move.SpeedUp(speedModifier);
        }
    }
    private IEnumerator RemoveSpeedEffectAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        speedModifier = 1.0f;
        move.ResetSpeed();
    }

    void PrepareForSceneChange()
    {
        PlayerData.SavePlayerState(this);
    }

    void OnEnable()
    {
        if (isLocalPlayer)
        {
            PlayerData.LoadPlayerState(this);
        }
    }

    // tracking win condition
    [Command]
    public void CmdReportLevelComplete(int levelId, float completionTime)
    {
        
        BattleManager.Instance.RecordLevelCompletion(connectionToClient.connectionId, levelId, completionTime);
    }

    [Command]
    public void CmdRequestSceneChange(string sceneName)
    {
        // Server-side logic before changing scene
        Debug.Log($"Player {gameObject.name} requested scene change to {sceneName}");

        // Tell this specific client to change scene
        TargetChangeScene(connectionToClient, sceneName);
    }

    [TargetRpc]
    private void TargetChangeScene(NetworkConnection target, string sceneName)
    {
        // Client loads the new scene additively
        Debug.Log($"Loading scene: {sceneName}");
        SceneManager.LoadSceneAsync(sceneName);
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
