using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    public float maxHP = 100;
    public float currentHP;

    public Image hpBar; // Reference to the Image component
    public float animationSpeed = 0.1f; // Speed of the animation

    private float targetFillAmount;

    // Start is called before the first frame update
    void Start()
    {
        currentHP = maxHP;
        targetFillAmount = 1f;
        UpdateHPBar();
    }

    // Update is called once per frame
    void Update()
    {
        // Update the position of the hpBar to follow the enemy
        if (hpBar != null)
        {
            hpBar.transform.position = transform.position + Vector3.up;
            // Smoothly animate the fill amount
            hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, targetFillAmount, animationSpeed * Time.deltaTime);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        targetFillAmount = (float)currentHP / maxHP;
        UpdateHPBarColor();
        if (currentHP <= 0)
        {
            Die();
        }
    }

    void UpdateHPBar()
    {
        if (hpBar != null)
        {
            hpBar.fillAmount = targetFillAmount;
            UpdateHPBarColor();
        }
    }

    void UpdateHPBarColor()
    {
        if (hpBar != null)
        {
            if (currentHP > maxHP * 0.5f)
            {
                hpBar.color = Color.green;
            }
            else if (currentHP > maxHP * 0.25f)
            {
                hpBar.color = Color.yellow;
            }
            else
            {
                hpBar.color = Color.red;
            }
        }
    }

    void Die()
    {
        // Handle enemy death (e.g., play animation, destroy object)
        Debug.Log("Enemy died");
        Destroy(hpBar.gameObject); // Destroy the HP bar
        Destroy(gameObject);
    }

    //This part move to Bomb Script
    /*void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bomb"))
        {
            // Assuming the bullet has a script with a damage value
            int damage = other.GetComponent<Bomb>().damage;
            TakeDamage(damage);
            //Destroy(other.gameObject); // Destroy the bullet after it hits the enemy
        }
    }*/
}
