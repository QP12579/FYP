using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Player : Singleton<Player>
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

    [Header("UI")]
    public Slider HPSlider;
    public Slider MPSlider;
    public Slider SpecialAttackSlider;
    public GameObject SPFullEffect;
    public GameObject SPHintKeywords;
    public TextMeshProUGUI levelText;

    private PlayerMovement move;
    [HideInInspector] public Animator animator;

    private void Start()
    {
        SetAlpha(SPFullEffect, 0f);
        SPHintKeywords.SetActive(false);

        move = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
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
        HPSlider.value = HP;
        MPSlider.value = MP;
        HPSlider.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = HP.ToString() + "/" + MaxHP.ToString();
        MPSlider.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = MP.ToString() + "/" + MaxMP.ToString();
        
        levelText.text = level.ToString();
    }

    public void TakeDamage(float damage, GameObject attacker = null)
    {
        float time = Time.time;
        if (move.blockTimes > time && (move.blockTimes - move.defenceTime) < time)
        {
            if (move.blockTimes - move.defenceTime /3 > time)
            {
                Debug.Log("Perfect Block");
                if (move.isReflect && attacker != null)
                { //Reflect
                    attacker.GetComponent<IAttackable>().TakeDamage(damage * move.reflectDamageMultiplier);
                }
                damage = 0;
                return;
            }
            damage *= move.blockPercentage;
            Debug.Log("Normal Block");
        }
        float realDamage = Mathf.Min(damage, HP);
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
    }

    public bool canUseSkill(float mp)
    {
        if(mp > MP) return false;
        MP -= mp;
        UpdatePlayerUIInfo();
        return true;
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
