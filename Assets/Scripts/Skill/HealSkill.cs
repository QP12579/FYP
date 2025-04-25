using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSkill : MonoBehaviour
{
    private float healAmount;

    public void Initialize(float power)
    {
        healAmount = power;
        // Heal player immediately
        Player.instance.Heal(healAmount);
        Destroy(gameObject, 1f);
    }
}
