using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSkill : MonoBehaviour
{
    private float healAmount;
    private Player player;

    public void Start()
    {
        player = GetComponentInParent<Player>();
    }

    public void Initialize(float power)
    {
        healAmount = power;
        // Heal player immediately
        player.Heal(healAmount);
        Destroy(gameObject, 1f);
    }
}
