using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : Singleton<Player>
{
    [Header("HP MP")]
    public int MaxHP = 100;
    [HideInInspector]
    public float HP = 100;
    [HideInInspector]    public float CurrentMaxHP => MaxHP * (1 + PlayerBuffSystem.instance.GetBuffValue(PlayerBuffSystem.BuffType.MaxHPUp));
    [HideInInspector]    public float CurrentMaxMP => MaxMP * (1 + PlayerBuffSystem.instance.GetBuffValue(PlayerBuffSystem.BuffType.MaxMPUp));

    public float MaxMP = 50;
    [HideInInspector]
    public float MP = 50;
    public int level = 1;

    [Header("UI")]
    public Slider HPSlider;
    public Slider MPSlider;
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
