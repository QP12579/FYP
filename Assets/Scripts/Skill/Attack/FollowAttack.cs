using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowAttack : AttackSkill
{
    float speed = 5;
    // Start is called before the first frame update

    public FollowAttack SetFollowAttack() 
    {
        //Let vfx move front to mouse position
        Vector3 direction = (GetMouseWorldPosition() - transform.position).normalized;
        direction.y = 0;
        if (direction.x < 0 && GetComponent<SpriteRenderer>() != null) gameObject.GetComponent<SpriteRenderer>().flipX = true;
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.velocity = direction * speed;
        Destroy(gameObject, coolDown);
        return this;
    }
    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, groundMask))
        {
            return hitInfo.point;
        }
        return Vector3.zero;
    }
}
