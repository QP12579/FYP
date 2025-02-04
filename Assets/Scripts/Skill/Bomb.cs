using Mirror.BouncyCastle.Asn1.Pkcs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float speed = 5f;
    public BombType type;
    private Animator anim;
    private Rigidbody rb;
    public LayerMask groundMask;

    [HideInInspector] public float damage = 10;

    private void Start()
    {
        anim = GetComponent<Animator>(); 
        rb = gameObject.GetComponent<Rigidbody>();
        if (type != BombType.trap)
            StartCoroutine(MoveAndThrowVFX(this.gameObject));

        transform.localScale = Vector3.one * 1.2f;
    }

    public Bomb SetTrapTypeBomb (Transform weaponPosi)
    {
        if (type != BombType.trap) return this;

        transform.position = weaponPosi.position;
        //Let vfx move front to mouse position
        Vector3 direction = (GetMouseWorldPosition() - transform.position).normalized;
        direction.y = 0;

        GetComponent<Rigidbody>().velocity = direction * speed * Time.timeScale;

        return this;
    }

    public void ShootLength(float length)
    {
        Destroy(gameObject, length);
    }

    private void OnTriggerEnter(Collider c)
    {
        if (type == BombType.trap && c.gameObject.layer.ToString() == "Terrain")
        {
            rb.velocity = Vector3.zero;
        }

        if (c.gameObject.CompareTag("Enemy") && c.gameObject.GetComponent<EnemyController>() != null)
        {
            c.GetComponent<EnemyController>().TakeDamage(damage);
            if (type == BombType.bomb)
            {
                anim.SetTrigger("Explosion");
                rb.velocity = Vector3.zero;
                Destroy(gameObject, 2f);
            }
            if (type == BombType.Shoot)
            {
                Destroy(gameObject);
            }
            if(type == BombType.trap)
            {
                anim.SetTrigger("Explosion");
                Destroy(gameObject, 2f);
            }
        }
    }

    private void OnTriggerStay(Collider c)
    {
        if (type == BombType.area)
        {
            if (c.GetComponent<EnemyController>())
                c.GetComponent<EnemyController>().TakeDamage(damage);
        }
                
    }

    private void OnCollisionEnter(Collision c)
    {
        if (type == BombType.trap && c.gameObject.layer.ToString() == "Terrain")
        {
            rb.velocity = Vector3.zero;
        }

        if (c.gameObject.CompareTag("Enemy") && c.gameObject.GetComponent<EnemyController>()!=null)
        {
            c.gameObject.GetComponent<EnemyController>().TakeDamage(damage);
            if (type == BombType.bomb)
            {
                anim.SetTrigger("Explosion");
                rb.velocity = Vector3.zero;
                Destroy(gameObject, 2f);
            }
            if (type == BombType.Shoot)
            {
                Destroy(gameObject);
            }
            if (type == BombType.trap)
            {
                anim.SetTrigger("Explosion");
                Destroy(gameObject, 2f);
            }
            //if (type == BombType.area)
              //  StartCoroutine(AreaHurt(c.gameObject, damage, 2f));
        }
    }

    private IEnumerator AreaHurt(GameObject c, float damge, float time)
    {
        float i = 0;
        while (i < time)
        {
            c.GetComponent<EnemyController>().TakeDamage(damage);
            yield return new WaitForSeconds(0.2f);
            i+= 0.2f;
        }
        Destroy(gameObject);
    }

    private IEnumerator MoveAndThrowVFX(GameObject vfx)
    {
        float elapsedTime = 0.1f;
        float maxThrowTime = 1f; // 最大拋出時間
        float throwHeight = 1f; // 最大拋高

        while (Input.GetMouseButton(1) && elapsedTime < maxThrowTime)
        {
            vfx.transform.position = transform.position;
            elapsedTime += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        Vector3 startPosition = vfx.transform.position;
        Vector3 targetPosition = GetMouseWorldPosition();

        float throwAmount = Mathf.Lerp(0, throwHeight, elapsedTime / maxThrowTime);

        float duration = 1f;
        float moveElapsedTime = 0f;

        while (moveElapsedTime < duration)
        {
            float t = moveElapsedTime / duration;

            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition * elapsedTime, t);
            currentPosition.y += Mathf.Sin(t * Mathf.PI) * throwAmount;

            vfx.transform.position = currentPosition;

            moveElapsedTime += Time.deltaTime;
            yield return null; 
        }
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

public enum BombType
{
    bomb,
    Shoot,
    trap,
    area
}