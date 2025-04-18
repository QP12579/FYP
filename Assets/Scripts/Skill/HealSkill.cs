using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSkill : MonoBehaviour
{
    private float healAmount;

    public void Initialize(float power)
    {
        healAmount = power;
        Destroy(gameObject, 1f);

        // Heal player immediately
        Player.instance.Heal(healAmount);
    }
}
