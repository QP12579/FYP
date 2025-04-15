using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerAttack : MonoBehaviour
{
    [Header(" Components ")]
    public Player playerMP;

    [Header("Attack")]
    public float attack = 5;
    public float waitTime = 0.5f;
    private bool canAtk = false;
    private Animator animator;

    [Header("Skills")]
    public Transform weaponPosi;
    [SerializeField] private LayerMask groundMask;

    private float blockTimes;
    private void Start()
    {
        animator = GetComponent<Animator>();
        canAtk = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            playerMP.animator.SetTrigger("NrmAtk");
        }
        if (Input.GetMouseButtonDown(1))
        {
            playerMP.animator.SetTrigger("Attack");
            SpawnVFX();
        }
    }

    public void OnTriggerStay(Collider c)
    {
        if (Input.GetKeyDown(KeyCode.E) && canAtk)
        {
            canAtk = false;
            LeanTween.delayedCall(waitTime, DetectOnce);
            animator.SetTrigger("normalATK");
            print("NATK");
            if (c.gameObject.GetComponent<Enemy>())
                c.gameObject.GetComponent<Enemy>().TakeDamage(attack);
        }
    }

    private void DetectOnce()
    {
        canAtk = true;
    }
    void SpawnVFX()
    {/*
        if (Skills == null) { Debug.Log("No Skill."); return; }
        SkillData skillData = new SkillData();
        skillData = Skills[Skills.Count - 1]; // use the new Skill
        playerMP.MP -= skillData.skillLevel;
        playerMP.UpdatePlayerUIInfo();

        // Create VFX
        if (skillData.skillPrefab == null) { Debug.Log("No Prefab in this skill."); return; }
        GameObject vfx = Instantiate(skillData.skillPrefab, transform.position, Quaternion.identity);
        if (vfx.GetComponent<Bomb>() != null)
        {
            Bomb newBomb = vfx.GetComponent<Bomb>();
            newBomb.damage = skillData.DamageOrHeal;
            newBomb.groundMask = groundMask;
            newBomb.SetTrapTypeBomb(weaponPosi);
        }*/
    }
}