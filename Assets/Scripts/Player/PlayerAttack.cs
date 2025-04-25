using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerAttack : MonoBehaviour
{
    [Header(" Components ")]
    public Player player;

    [Header("NormalAttack")]
    public float attack = 5;
    public float waitTime = 0.5f;
    private bool canAtk = false;
    private Animator animator;

    private float blockTimes;
    private void Start()
    {
        animator = GetComponent<Animator>();
        canAtk = true;
        if(player == null)
            player = FindObjectOfType<Player>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            player.animator.SetTrigger("NrmAtk");
        }
        if (Input.GetMouseButtonDown(1))
        {
            player.animator.SetTrigger("Attack");
        }
    }

    public void OnTriggerStay(Collider c)
    {
        if (Input.GetKeyDown(KeyCode.E) && canAtk)
        {
            canAtk = false;
            LeanTween.delayedCall(waitTime, DetectOnce);
            animator.SetTrigger("normalATK");
            if (c.gameObject.GetComponent<IAttackable>()!=null)
                c.gameObject.GetComponent<IAttackable>().TakeDamage(attack);
        }
    }

    private void DetectOnce()
    {
        canAtk = true;
    }
}