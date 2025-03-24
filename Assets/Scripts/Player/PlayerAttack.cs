using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attack = 5;
    public float waitTime = 0.5f;
    private bool canAtk = false;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        canAtk = true;
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
}