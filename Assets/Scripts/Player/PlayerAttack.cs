using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    public float attack = 5;

    public void OnTriggerStay(Collider c)
    {
        if(Input.GetKeyDown(KeyCode.E))
            if (c.gameObject.GetComponent<Enemy>())
                c.gameObject.GetComponent<Enemy>().TakeDamage(attack);
        
    }
}
