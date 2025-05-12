using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float scaleSpeed = 2f; // 控制縮放變化速度

    private float timer = 0f;
    private Vector3 direction = Vector3.forward;

    private void Start()
    {
        // 嘗試尋找場景中的玩家
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            direction = (player.transform.position - transform.position).normalized;
        }
        // 若找不到玩家，維持預設方向
    }

    private void Update()
    {
        Move();
        AnimateScale();
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void Move()
    {
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    // 讓子彈縮放在1~2之間循環
    private void AnimateScale()
    {
        float scale = 3f + Mathf.PingPong(Time.time * scaleSpeed, 5f); // 1~2之間
        transform.localScale = new Vector3(scale, scale, scale);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 呼叫玩家的 TakeDamage 方法
            var playerController = other.GetComponent<Player>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
