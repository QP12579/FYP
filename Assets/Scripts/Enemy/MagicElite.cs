using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Magicmovement))]
public class MagicElite : MonoBehaviour
{
    [Header("Magic Elite Settings")]
    public float attackRange = 8f;
    public float meleeRange = 2f;
    public float attackFrequency = 1.5f;

    [Header("Magic Attack Settings")]
    public GameObject magicProjectilePrefab;
    public float projectileSpeed = 12f;
    public float magicDamage = 20f;

    [Header("Laser Attack Settings")]
    public GameObject magicLaserPrefab;
    public float laserDuration = 1.5f;
    public float laserDamage = 30f;

    [Header("Melee Attack Settings")]
    public float meleeDamage = 15f;

    [Header("Animation Durations")]
    public float atk1AnimDuration = 0.5f; // 近戰動畫長度
    public float atk2AnimDuration = 0.5f; // 魔法彈動畫長度
    public float atk3AnimDuration = 0.5f; // 雷射動畫長度

    private Magicmovement movement;
    private Player player;
    private Animator anim;
    private float attackDelay;
    private float attackTimer;

    private void Start()
    {
        movement = GetComponent<Magicmovement>();
        anim = GetComponentInChildren<Animator>();
        player = FindObjectOfType<Player>();
        attackDelay = 1f / attackFrequency;
        attackTimer = 0f;
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= meleeRange)
        {
            movement.Stop();
            if (attackTimer >= attackDelay)
            {
                MeleeAttack();
                attackTimer = 0f;
            }
            else
            {
                attackTimer += Time.deltaTime;
            }
        }
        else if (distance <= attackRange)
        {
            movement.Stop();
            if (attackTimer >= attackDelay)
            {
                int attackType = Random.Range(0, 2); // 0: magic ball, 1: laser
                if (attackType == 0)
                    MagicAttack();
                else
                    StartCoroutine(LaserAttack());
                attackTimer = 0f;
            }
            else
            {
                attackTimer += Time.deltaTime;
            }
        }
        else
        {
            movement.Move();
        }
    }

    private bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= range;
    }

    private void MagicAttack()
    {
        Debug.Log("MagicElite performs magic attack!");
        if (anim != null)
        {
            anim.SetBool("ATK2", true);
            StartCoroutine(ResetBoolAfterDelay("ATK2", atk2AnimDuration));
        }

        if (magicProjectilePrefab != null && player != null)
        {
            Vector3 spawnPos = transform.position + (player.transform.position - transform.position).normalized;
            GameObject proj = Instantiate(magicProjectilePrefab, spawnPos, Quaternion.identity);
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 dir = (player.transform.position - spawnPos).normalized;
                rb.velocity = dir * projectileSpeed;
            }
            // 建議：在魔法彈腳本中處理碰撞與 player.TakeDamage(magicDamage)
        }
    }

    private IEnumerator LaserAttack()
    {
        Debug.Log("MagicElite performs laser attack!");
        if (anim != null)
        {
            anim.SetBool("ATK3", true);
            StartCoroutine(ResetBoolAfterDelay("ATK3", atk3AnimDuration));
        }

        if (magicLaserPrefab != null && player != null)
        {
            GameObject laser = Instantiate(magicLaserPrefab, transform.position, Quaternion.LookRotation(player.transform.position - transform.position));
            if (IsPlayerInLaserLine(player.transform.position))
            {
                player.TakeDamage(laserDamage);
            }
            yield return new WaitForSeconds(laserDuration);
            if (laser != null)
                Destroy(laser);
        }
        else
        {
            yield return null;
        }
    }

    private bool IsPlayerInLaserLine(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        Ray ray = new Ray(transform.position, dir);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, attackRange))
        {
            if (hit.collider.GetComponent<Player>() != null)
                return true;
        }
        return false;
    }

    private void MeleeAttack()
    {
        Debug.Log("MagicElite performs melee attack!");
        if (anim != null)
        {
            anim.SetBool("ATK1", true);
            StartCoroutine(ResetBoolAfterDelay("ATK1", atk1AnimDuration));
        }

        if (player != null && IsPlayerInRange(meleeRange))
        {
            player.TakeDamage(meleeDamage);
        }
    }

    private IEnumerator ResetBoolAfterDelay(string param, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (anim != null)
            anim.SetBool(param, false);
    }
}
