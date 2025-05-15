using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(elitemovement), typeof(Animator))]
public class EliteEnemy : Enemy
{
    private IEnemyState currentState;

    [Header("Audio Clip")]
    public AudioClip SlowATK_SFX;
    public AudioClip ShootATK_SFX;
    public AudioClip ATK4_SFX;

    [Header("Elite Enemy Settings")]
    public float patrolSpeed = 3f;
    public float attackRange = 2f;
    public float restDuration = 2f;
    public float earthquakeRange = 5f;
    public float earthquakeDamage = 20f;

    [Header("Hand Prefab")]
    [SerializeField] private Mechhand mechhandPrefab;

    private elitemovement eliteMovement;
    private Animator anim;
    private bool lastAttackWasFast = false;
    public bool LastAttackWasFast => lastAttackWasFast;

    // 動畫參數持續時間，可依實際動畫長度調整
    private float fastAttackAnimDuration = 1f;
    private float slowAttackAnimDuration = 1f;
    private float shootHandAnimDuration = 1f;
    private float earthquakeAnimDuration = 1f;

    protected override void Start()
    {
        base.Start();

        anim = GetComponent<Animator>();
        if (anim == null)
        {
            Debug.LogError("Animator component is missing on EliteEnemy!");
            return;
        }

        eliteMovement = GetComponent<elitemovement>();
        if (eliteMovement == null)
        {
            Debug.LogError("elitemovement component is missing on EliteEnemy!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player object not found in the scene!");
            return;
        }

        ChangeState(new PatrolState(this));
    }

    protected override void Update()
    {
        base.Update();
        currentState?.UpdateState();
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.ExitState();
        currentState = newState;
        currentState?.EnterState();
    }

    public void StopMovement()
    {
        eliteMovement.Stop();
    }

    public void ResumeMovement()
    {
        eliteMovement.Move();
    }

    public void FastMeleeAttack()
    {
        Debug.Log("Performing fast melee attack!");
        lastAttackWasFast = true;
        if (anim != null)
        {
            anim.SetBool("FastATK", true);
            StartCoroutine(FastAttackCoroutine());
            if (ATK_SFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySFX(ATK_SFX, transform);
        }
    }

    private IEnumerator FastAttackCoroutine()
    {
        yield return new WaitForSeconds(fastAttackAnimDuration);
        if (player != null && IsPlayerInRange(attackRange))
        {
            player.TakeDamage(10);
        }
        if (anim != null)
            anim.SetBool("FastATK", false);
    }

    public void SlowMeleeAttack()
    {
        Debug.Log("Performing slow melee attack!");
        lastAttackWasFast = false;
        if (anim != null)
        {
            anim.SetBool("SlowATK", true);
            StartCoroutine(SlowAttackCoroutine());
            if (SlowATK_SFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySFX(SlowATK_SFX, transform);
        }
    }

    private IEnumerator SlowAttackCoroutine()
    {
        yield return new WaitForSeconds(slowAttackAnimDuration);
        if (player != null && IsPlayerInRange(attackRange))
        {
            player.TakeDamage(30);
        }
        if (anim != null)
            anim.SetBool("SlowATK", false);
    }

    public void ShootHandAttack()
    {
        Debug.Log("Performing shoot hand attack!");
        StopMovement();

        if (mechhandPrefab != null && player != null)
        {
            Mechhand hand = Instantiate(mechhandPrefab, transform.position, transform.rotation);
            hand.ShootAt(player.transform.position, ResumeMovement);
        }
        else
        {
            ResumeMovement();
        }

        if (anim != null)
        {
            anim.SetBool("ShootHand", true);
            StartCoroutine(ResetBoolAfterDelay("ShootHand", shootHandAnimDuration));
            if (ShootATK_SFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySFX(ShootATK_SFX, transform);
        }
    }

    public void EarthquakeAttack()
    {
        Debug.Log("Performing earthquake attack!");
        StopMovement();

        if (anim != null)
        {
            anim.SetBool("ATK4", true);
            StartCoroutine(ResetBoolAfterDelay("ATK4", earthquakeAnimDuration));

            if (ATK4_SFX != null && SoundManager.instance != null)
                SoundManager.instance.PlaySFX(ATK4_SFX, transform);
        }

        // 延遲到動畫最後一幀才造成傷害
        StartCoroutine(DealEarthquakeDamageAfterDelay(earthquakeAnimDuration));
        // ResumeMovement() 移到傷害結束後
    }

    private IEnumerator DealEarthquakeDamageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, earthquakeRange);
        foreach (var hitCollider in hitColliders)
        {
            Player targetPlayer = hitCollider.GetComponent<Player>();
            if (targetPlayer != null)
            {
                targetPlayer.TakeDamage(earthquakeDamage);
                Debug.Log($"Player took {earthquakeDamage} damage from earthquake attack!");
            }
        }

        ResumeMovement();
    }

    private IEnumerator ResetBoolAfterDelay(string param, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (anim != null)
        {
            anim.SetBool(param, false);
        }
    }

    public bool IsPlayerInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= range;
    }

    // Added method to expose player's position
    public Vector3 GetPlayerPosition()
    {
        return player.transform.position;
    }
}

public interface IEnemyState
{
    void EnterState();
    void UpdateState();
    void ExitState();
}

public class PatrolState : IEnemyState
{
    private EliteEnemy enemy;

    public PatrolState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        Debug.Log("Entering Patrol State");
        enemy.ResumeMovement();
    }

    public void UpdateState()
    {
        if (enemy.IsPlayerInRange(enemy.attackRange))
        {
            enemy.ChangeState(new AttackState(enemy));
        }
    }

    public void ExitState()
    {
        Debug.Log("Exiting Patrol State");
        enemy.StopMovement();
    }
}

// Fix for the error CS1061: 'bool' does not contain a definition for 'transform'  
// The issue is caused by the incorrect usage of 'enemy.isLocalPlayer' which is a bool.  
// The correct property to access the player's transform should be 'enemy.player.transform'.  

public class ChaseState : IEnemyState
{
    private EliteEnemy enemy;

    public ChaseState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        Debug.Log("Chasing the player!");
    }

    public void UpdateState()
    {
        if (enemy.IsPlayerInRange(enemy.attackRange))
        {
            enemy.ChangeState(new AttackState(enemy));
            return;
        }

        Vector3 directionToPlayer = (enemy.GetPlayerPosition() - enemy.transform.position).normalized;
        enemy.transform.position += directionToPlayer * Time.deltaTime * enemy.patrolSpeed;
    }

    public void ExitState()
    {
        enemy.StopMovement();
    }
}

public class AttackState : IEnemyState
{
    private EliteEnemy enemy;
    private float attackCooldown = 2f;
    private float attackTimer;

    // 各攻擊冷卻秒數
    private float fastSlowCD = 2f;
    private float earthquakeCD = 5f;
    private float shootHandCD = 4f;

    // 各攻擊剩餘冷卻
    private float fastSlowTimer = 0f;
    private float earthquakeTimer = 0f;
    private float shootHandTimer = 0f;

    // 0: 無，1: 強制快攻，2: 強制慢攻
    private int pendingForceAttackType = 0;

    public AttackState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        attackTimer = 0f;
    }

    public void UpdateState()
    {
        attackTimer += Time.deltaTime;
        if (fastSlowTimer > 0) fastSlowTimer -= Time.deltaTime;
        if (earthquakeTimer > 0) earthquakeTimer -= Time.deltaTime;
        if (shootHandTimer > 0) shootHandTimer -= Time.deltaTime;

        if (attackTimer >= attackCooldown)
        {
            PerformAttack();
            attackTimer = 0f;
        }

        if (!enemy.IsPlayerInRange(enemy.attackRange))
        {
            enemy.ChangeState(new PatrolState(enemy));
        }
    }

    private void PerformAttack()
    {
        // 強制快/慢攻（無視冷卻）
        if (pendingForceAttackType == 1)
        {
            enemy.FastMeleeAttack();
            pendingForceAttackType = 2;
            fastSlowTimer = fastSlowCD;
            return;
        }
        else if (pendingForceAttackType == 2)
        {
            enemy.SlowMeleeAttack();
            pendingForceAttackType = 1;
            fastSlowTimer = fastSlowCD;
            return;
        }

        // 收集可用攻擊
        List<int> available = new List<int>();
        if (fastSlowTimer <= 0) available.Add(0); // 0:快/慢
        if (earthquakeTimer <= 0) available.Add(1); // 1:地震
        if (shootHandTimer <= 0) available.Add(2); // 2:射手

        if (available.Count == 0)
            return; // 全部冷卻中，這回合不攻擊

        int attackType = available[Random.Range(0, available.Count)];
        if (attackType == 0)
        {
            // 隨機快或慢
            if (Random.value < 0.5f)
            {
                enemy.FastMeleeAttack();
                pendingForceAttackType = 2;
            }
            else
            {
                enemy.SlowMeleeAttack();
                pendingForceAttackType = 1;
            }
            fastSlowTimer = fastSlowCD;
        }
        else if (attackType == 1)
        {
            enemy.EarthquakeAttack();
            earthquakeTimer = earthquakeCD;
        }
        else if (attackType == 2)
        {
            enemy.ShootHandAttack();
            shootHandTimer = shootHandCD;
        }
    }

    public void ExitState()
    {
    }
}

public class RestState : IEnemyState
{
    private EliteEnemy enemy;
    private float restTimer;

    public RestState(EliteEnemy enemy)
    {
        this.enemy = enemy;
    }

    public void EnterState()
    {
        restTimer = 0f;
        Debug.Log("Resting...");
    }

    public void UpdateState()
    {
        restTimer += Time.deltaTime;

        if (restTimer >= enemy.restDuration)
        {
            enemy.ChangeState(new PatrolState(enemy));
        }
    }

    public void ExitState()
    {
        Debug.Log("Finished resting.");
    }
}
