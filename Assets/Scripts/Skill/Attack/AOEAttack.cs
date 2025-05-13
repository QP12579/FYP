using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEAttack : AttackSkill
{
    public float duration = 0.2f;
    protected Dictionary<Collider, Coroutine> _aoeATKs = new Dictionary<Collider, Coroutine>();

    public AttackSkill SetAOESkill(Vector3 targetPosi)
    {
        transform.position = targetPosi; 
        Destroy(gameObject, duration + 0.2f);
        return this;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        IAttackable attackable = other.GetComponent<IAttackable>();
        if (attackable != null)
        {
            Coroutine routine = StartCoroutine(KeepTakeDamage(attackable));
            _aoeATKs[other] = routine;

            if (haveDebuff)
            {
                IDebuffable debuffable = other.GetComponent<IDebuffable>();
                if (debuffable != null)
                {
                    foreach (var debuff in debuffs)
                    {
                        debuff.DebuffTarget(debuffable);
                    }
                }
            }
            PlayerSkillController.instance.gameObject.GetComponent<Player>().GetSP();
        }
    }
    protected void OnTriggerExit(Collider other)
    {
        if (_aoeATKs.TryGetValue(other, out Coroutine routine))
        {
            StopCoroutine(routine);
            _aoeATKs.Remove(other);
        }
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        IAttackable attackable = other.GetComponent<IAttackable>(); 
        if (attackable != null)
        {
            if (_aoeATKs.ContainsKey(other)) return;
            Coroutine routine = StartCoroutine(KeepTakeDamage(attackable));
            _aoeATKs[other] = routine;

            if (haveDebuff)
            {
                IDebuffable debuffable = other.GetComponent<IDebuffable>();
                if (debuffable != null)
                {
                    foreach (var debuff in debuffs)
                    {
                        debuff.DebuffTarget(debuffable);
                    }
                }
            }
            PlayerSkillController.instance.gameObject.GetComponent<Player>().GetSP();
        }

    }

    protected virtual IEnumerator KeepTakeDamage(IAttackable attackable)
    {
        float i = 0;
        while (i < duration)
        {
            attackable.TakeDamage(gameObject.transform.position, damage);
            yield return new WaitForSeconds(0.2f);
            i += 0.2f;
            Debug.Log($"attackable: {_aoeATKs.Count}|| {i/duration}");
        }        
    }
}
