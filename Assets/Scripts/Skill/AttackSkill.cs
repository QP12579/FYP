using UnityEngine;

public class AttackSkill : MonoBehaviour
{
    public float damage;
    public AttackType type;
    [SerializeField] private LayerMask EnemyMask;
    [SerializeField] public LayerMask groundMask;

    private bool haveDebuff = false;
    private DebuffSkill debuff;

    public void Initialize(float power)
    {
        damage = power;
    }

    public void HaveDebuff()
    {
        haveDebuff = true;
        debuff = GetComponent<DebuffSkill>();
    }

    public AttackSkill SetAttackType(Transform weaponPosi)
    {
        transform.position = weaponPosi.position;
        switch (type)
        {
            case AttackType.Follow:
            FollowAttack follow = GetComponent<FollowAttack>();
            follow.SetFollowAttack(GetTargetPosi());

            Debug.Log("Follow.");
            return this;

            case AttackType.AOE:
                AOEAttack aoe = GetComponent<AOEAttack>();
                aoe.SetAOESkill(GetTargetPosi());
                return this;
            case AttackType.Single:
                return this;
            default:
                return this;
        }
    }

    public Vector3 GetTargetPosi()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f, EnemyMask);
        Vector3 currentTarget = Vector3.zero;
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            currentTarget = hits[0].point;
        }
        else if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundMask))
        {
            return hitInfo.point;
        }

        return currentTarget;
    }

    public float GetDamageValue()
    {
        return damage;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        IAttackable attackable = other.GetComponent<IAttackable>();
        if (attackable != null)
        {
            attackable.TakeDamage(damage);
            if (haveDebuff)
            {
                IDebuffable debuffable = other.GetComponent<IDebuffable>();
                if (debuffable != null)
                {
                    debuff.DebuffTarget(debuffable);
                }
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        IAttackable attackable = other.gameObject.GetComponent<IAttackable>();
        if (attackable != null)
        {
            attackable.TakeDamage(damage);
            if (haveDebuff)
            {
                IDebuffable debuffable = other.gameObject.GetComponent<IDebuffable>();
                if (debuffable != null)
                {
                    debuff.DebuffTarget(debuffable);
                }
            }
        }
    }

}
