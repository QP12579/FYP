using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSkill : MonoBehaviour
{
    public float damage;
    public float coolDown;
    public Rigidbody rb;
    public LayerMask groundMask;

    public void Initialize(float power, float cool)
    {
        damage = power;
        coolDown = cool;
        rb = GetComponent<Rigidbody>();
    }

    public void OnTriggerEnter(Collider other)
    {
        IAttackable attackable = other.GetComponent<IAttackable>();
        if(attackable != null)
        {
            attackable.TakeDamage(damage);
        }
    }

    public void OnCollisionEnter(Collision other)
    {
        IAttackable attackable = other.gameObject.GetComponent<IAttackable>();
        if (attackable != null)
        {
            attackable.TakeDamage(damage);
        }
    }
}
