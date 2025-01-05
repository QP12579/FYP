using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemymovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float changeDirectionInterval = 2f;

    private Vector3 moveDirection;
    private float timeSinceLastChange = 0f;

    // Start is called before the first frame update
    void Start()
    {
        ChangeDirection();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastChange += Time.deltaTime;
        gameObject.GetComponent<Animator>().SetFloat("moveSpeed", timeSinceLastChange);
        if (timeSinceLastChange >= changeDirectionInterval)
        {
            ChangeDirection();
            timeSinceLastChange = 0f;
        }

        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Flip the enemy to face the movement direction
        if (moveDirection.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveDirection.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void ChangeDirection()
    {
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector3(Mathf.Cos(randomAngle * Mathf.Deg2Rad), 0, Mathf.Sin(randomAngle * Mathf.Deg2Rad)).normalized;
    }
}
