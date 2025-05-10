using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement2 : EnemyMovement
{
    public float changeDirectionInterval = 2f;

    private float timeSinceLastChange = 0f;
    private bool isAttacking = false;

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void DifferentMovement()
    {
        if (!isAttacking)
        {
            timeSinceLastChange += Time.deltaTime;
            if (timeSinceLastChange >= changeDirectionInterval)
            {
                ChangeDirection();
                timeSinceLastChange = 0f;
            }

            transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        }
    }

    public void StartAttack()
    {
        isAttacking = true;
    }

    public void StopAttack()
    {
        isAttacking = false;
    }
}
