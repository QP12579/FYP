using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSkill : MonoBehaviour
{
    public float damage;
    public LayerMask EnemyMask;

    public void Initialize(float power)
    {
        damage = power;
    }

    public float GetDamageValue()
    {
        return damage;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IAttackable attackable = other.GetComponent<IAttackable>();
        if(attackable != null)
        {
            attackable.TakeDamage(damage);
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        IAttackable attackable = other.gameObject.GetComponent<IAttackable>();
        if (attackable != null)
        {
            attackable.TakeDamage(damage);
        }
    }
}
