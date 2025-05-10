using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemymovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float changeDirectionInterval = 2f;

    private Vector3 moveDirection;
    private float timeSinceLastChange = 0f;
    private SpriteRenderer sr;
    private bool isAttacking = false;

    // Start is called before the first frame update
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        ChangeDirection();
    }

    // Update is called once per frame
    void Update()
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

            // Flip the enemy to face the movement direction
            sr.flipX = moveDirection.x > 0;
        }
    }
    [Server]
    void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }

    // 碰撞檢測：當碰撞到牆壁時改變方向
    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Collided with wall, changing direction.");
            ChangeDirection(); // 碰撞到牆壁時改變方向
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
