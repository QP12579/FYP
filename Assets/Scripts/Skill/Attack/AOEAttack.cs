using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttack : AttackSkill
{
    public float duration = 0.2f;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        StartCoroutine(AreaHurt(other.gameObject, damage, duration));
    }

    public AttackSkill SetAOESkill(Vector3 targetPosi)
    {
        transform.position = targetPosi;
        return this;
    }

    private IEnumerator AreaHurt(GameObject c, float damge, float time)
    {
        float i = 0;
        while (i < time)
        {
            c.GetComponent<IAttackable>().TakeDamage(damage);
            yield return new WaitForSeconds(0.2f);
            i += 0.2f;
        }
        Destroy(gameObject);
    }
}
