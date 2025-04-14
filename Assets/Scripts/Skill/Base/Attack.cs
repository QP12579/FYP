using UnityEngine;

public abstract class Attack: SkillData { public abstract void Function(); }
public interface IAttackable { void TakeDamage(float damage); }

public class AttackSkill : MonoBehaviour
{
    private float damage;

    public void Initialize(float power)
    {
        damage = power;
        Destroy(gameObject, 3f); // Auto-destroy after 3 seconds
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<Enemy>().TakeDamage(damage);
        }
    }
}

// HealSkill.cs
public class HealSkill : MonoBehaviour
{
    private float healAmount;

    public void Initialize(float power)
    {
        healAmount = power;
        Destroy(gameObject, 1f);

        // Heal player immediately
        Player.instance.Heal(healAmount);
    }
}

public class AttackLock : Attack
{
    GameObject SkillPrefab { get; set; }
    GameObject Target { get; set; }
    public override void Function()
    {
        LeanTween.followSpring(SkillPrefab.transform, Target.transform, LeanProp.position, 0.1f);
    }

    public void Damage(float damage)
    {
        IAttackable attackableObject = Target.GetComponent<IAttackable>();
        if (attackableObject != null) attackableObject.TakeDamage(damage);
    }
}
public class IAttackOfEffect : Attack
{
    GameObject SkillPrefab { get; set; }
    float radius { get; set; }
    float duration { get; set; } // How Long with the Area
    int frequency { get; set; } // number of attack in time
    float damage { get; set; }
    public override void Function() // 
    {
            LeanTween.delayedCall(duration / frequency, () => Damage(damage)).setRepeat(frequency);
    }
    public void Damage(float damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(SkillPrefab.transform.position, radius);
        foreach (var item in hitColliders)
        {
            IAttackable attackableObject = item.GetComponent<IAttackable>();
            if (attackableObject!=null) attackableObject.TakeDamage(damage);
        }
    }
}
public class FollowAttack : Attack
{
    GameObject SkillPrefab { get; set; }
    GameObject Target { get; set; }
    float radius { get; set; }
    public override void Function()
    {
        LeanTween.followSpring(SkillPrefab.transform, Target.transform, LeanProp.position, 0.1f);
    }
    public void Damage(float damage)
    {
        Collider[] hitColliders = Physics.OverlapSphere(SkillPrefab.transform.position, radius);
        foreach (var item in hitColliders)
        {
            IAttackable attackableObject = item.GetComponent<IAttackable>();
            if (attackableObject != null) attackableObject.TakeDamage(damage);
        }
    }
}