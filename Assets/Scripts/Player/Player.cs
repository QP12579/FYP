using Skill;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("HP MP")]
    public int MaxHP = 100;
    public float HP = 100;
    public int MaxMP = 50;
    public int MP = 50;
    public int level = 1;

    [Header("UI")]
    public Slider HPSlider;
    public Slider MPSlider;
    public TextMeshProUGUI levelText;

    [Header("Skills")]
    public List<WeaponData> Weapons = null;
    public List<SkillData> Skills = null;
    public Transform weaponPosi;
    [SerializeField] private LayerMask groundMask;

    public float defenceTime = 0.5f;
    public float defenceDelayTime = 1f;
    private float blockPercentage = 0.5f;
    private float blockTimes;
    private PlayerMovement movement;
    private Animator animator;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
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

    public void TakeDamage(float damage)
    {
        float time = Time.time;
        if (blockTimes > time && (blockTimes - defenceTime) < time)
        {
            if (blockTimes - defenceTime/3 > time)
            {
                Debug.Log("Perfect Block");
                damage = 0;
                return;
            }
            damage *= blockPercentage;
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
        }*/

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("NrmAtk");
        }
        if (Input.GetMouseButtonDown(1))
        {
            animator.SetTrigger("Attack");
            SpawnVFX();
        }
        if (Input.GetKeyDown(KeyCode.Q) && Time.time > blockTimes + defenceDelayTime) //delay time
        {
            animator.SetTrigger("Defence");
            BlockAttack(Time.time);
        }
    }

    void SpawnVFX()
    {
        if (Skills == null) { Debug.Log("No Skill.");  return; }
        SkillData skillData = new SkillData();
        skillData = Skills[Skills.Count - 1]; // use the new Skill
        MP -= skillData.skillLevel;
        UpdatePlayerUIInfo();

        // Create VFX
        if(skillData.skillPrefab == null) { Debug.Log("No Prefab in this skill."); return; }
        GameObject vfx = Instantiate(skillData.skillPrefab, transform.position, Quaternion.identity);
        if (vfx.GetComponent<Bomb>() != null)
        {
            Bomb newBomb = vfx.GetComponent<Bomb>();
            newBomb.damage = skillData.DamageOrHeal;
            newBomb.groundMask = groundMask;
            newBomb.SetTrapTypeBomb(weaponPosi);
        }
        //Destroy(vfx, level + 1);
    }

    void BlockAttack(float blockTime)
    {
        blockTimes = blockTime + defenceTime;
        Debug.Log("blockTimes:" + blockTimes + "\nTime: " + blockTime);
    }
}
