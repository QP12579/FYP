using Mirror.BouncyCastle.Asn1.Pkcs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public BombType type;
    private Animator anim;
    private Rigidbody rb;
    [HideInInspector] public float damage = 10;

    private void Start()
    {
        anim = GetComponent<Animator>(); 
        rb = gameObject.GetComponent<Rigidbody>();
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

        if (c.gameObject.CompareTag("Enemy"))
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

    private void OnCollisionEnter(Collision c)
    {
        if (type == BombType.trap && c.gameObject.layer.ToString() == "Terrain")
        {
            rb.velocity = Vector3.zero;
        }

        if (c.gameObject.CompareTag("Enemy"))
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
        }
    }

}

public enum BombType
{
    bomb,
    Shoot,
    trap
}