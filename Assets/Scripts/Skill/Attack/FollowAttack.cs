using UnityEngine;

public class FollowAttack : AttackSkill
{
    public Rigidbody rb;
    public float speed = 5;

    public AttackSkill SetFollowAttack(Vector3 targetPosi) 
    {
        //Let vfx move front to mouse position
        Vector3 direction = (targetPosi - transform.position).normalized;
        Debug.Log(targetPosi);
        direction.y = 0;
        if (direction.x < 0 && GetComponent<SpriteRenderer>() != null) gameObject.GetComponent<SpriteRenderer>().flipX = true;
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.velocity = direction * speed;

        ShootLength(5f);
        return this;
    }

    public void ShootLength(float length)
    {
        Destroy(gameObject, length);
    }

    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
        Destroy(gameObject, 1f);
    }
}
