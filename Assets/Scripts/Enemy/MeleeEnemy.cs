using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(EnemyMovement1))]
public class MeleeEnemy : Enemy
{
    [Header("Attack")]
    [SerializeField] private int damage;
    [SerializeField] private float attackFrequency;

    private float attackDelay;
    private float attackTimer;
    private Animator anim;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
        attackDelay = 1f / attackFrequency;
    }

    protected override void FindingPlayer()
    {
        base.FindingPlayer();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (attackTimer >= attackDelay)
            TryAttack();
        else
            attackTimer += Time.deltaTime;
    }

    private void TryAttack()
    {
        if (player == null)
        {
            Debug.Log("Did not found Player. Stopping attack.");
            FindingPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer < 0) distanceToPlayer *= -1;
        if (distanceToPlayer < playerDetectionRadius)
        {
            Attack();
        }
    }

    private void Attack()
    {
        Debug.Log("Dealing " + damage + " damage to player ");
        attackTimer = 0;
        player.TakeDamage(damage);

        if (anim != null)
            anim.SetTrigger("isAttack");
    }

    public void EnemyGetHit(Vector3 hitDirection, float damage)
    {
        Debug.Log("Enemy got hit");
        GetHit(hitDirection, damage);
    }
}
