using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header(" Components ")]
    public Player player;

    [Header("NormalAttack")]
    [SerializeField] private KeyCode NATKKey = KeyCode.Mouse0;
    public float attack = 5;
    public float waitTime = 0.5f;
    private bool canAtk = false;
    private Animator animator;

    [Header("FarAttack")]
    [SerializeField] private KeyCode FATKKey = KeyCode.Mouse1;
    public GameObject ATKPrefab;
    public float FarAtk = 5;
    [SerializeField] private LayerMask groundMask;

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
        animator = GetComponent<Animator>();
        canAtk = true;
        if(player == null)
            player = FindObjectOfType<Player>();
        ATKCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(FATKKey) && canAtk)
        {
            player.animator.SetTrigger("Attack");
            FarAttack();
        }
    }

    public void OnTriggerStay(Collider c)
    {
        if (Input.GetKeyDown(NATKKey) && canAtk)
        {
            NrmATK(c);
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

    void NrmATK(Collider c)
    {
            canAtk = false;
            LeanTween.delayedCall(waitTime * (1 - AbilityATKSpeedPlus) , DetectOnce);
            player.animator.SetTrigger("NrmAtk");
            animator.SetTrigger("normalATK");
            if (c.gameObject.GetComponent<IAttackable>()!=null)
                c.gameObject.GetComponent<IAttackable>().TakeDamage(attack * (1 + AbilityATKPlus));
    }

    void FarAttack()
    {
        canAtk = false;
        LeanTween.delayedCall(waitTime * (1 - AbilityATKSpeedPlus), DetectOnce);
        if (ATKPrefab == null) { Debug.Log("No Far Attack Prefab."); return; }
        if (player.canUseSkill(FarAtk))
        {
            GameObject vfx = Instantiate(ATKPrefab, transform.position, Quaternion.identity);
            if (vfx.GetComponent<Bomb>() != null)
            {
                Bomb newBomb = vfx.GetComponent<Bomb>();
                newBomb.damage = FarAtk * (1 + AbilityATKPlus);
                newBomb.groundMask = groundMask;
                newBomb.gameObject.transform.localScale *= (1+AbilityATKArea);
                newBomb.SetTrapTypeBomb(transform);
            }
        }
    }
}