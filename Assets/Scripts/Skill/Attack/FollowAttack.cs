using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAttack : AttackSkill
{
    public Rigidbody rb;
    public float speed = 5;
    // Start is called before the first frame update
    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetFollowAttack();
        Instantiate(this);
    }

    public void SetFollowAttack() 
    {
        //Let vfx move front to mouse position
        Vector3 direction = (GetTargetPosi().position - transform.position).normalized;
        direction.y = 0;
        if (direction.x < 0 && GetComponent<SpriteRenderer>() != null) gameObject.GetComponent<SpriteRenderer>().flipX = true;
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.velocity = direction * speed;
    }

    private Transform GetTargetPosi()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 30f, EnemyMask);
        Transform currentTarget = null;
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            currentTarget = hits[0].transform;
        }
        else if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, EnemyMask))
            return hitInfo.transform;

        return currentTarget;
    }

    protected override void OnCollisionEnter(Collision other)
    {
        base.OnCollisionEnter(other);
        Destroy(gameObject, 2f);
    }
}
