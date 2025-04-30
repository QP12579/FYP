using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed;
    private float baseSpeed => speed;
    public float groundDist;
    public float jumpForce = 500;

    public LayerMask terrainLayer;

    [Header("KeyCode")]
    [SerializeField] private KeyCode RollKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode JumpKey = KeyCode.Space;
    [SerializeField] private KeyCode DEFKey = KeyCode.F;

    [Header("Defense/Rolling")]
    public bool isReflect = false;
    public float reflectDamageMultiplier = 1.0f;
    [HideInInspector] public bool RollingATK = false;

    public float defenceTime = 0.5f;
    public float defenceDelayTime = 1f;
    public float rollingTime = 0.5f;

    [HideInInspector] public float blockPercentage = 0.5f;
    [HideInInspector] public float blockTimes;
    [HideInInspector] public float damage;

    private Rigidbody rb;
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public Animator anim;
    private bool isGrounded;
    private bool oneTime;
    public bool canMove;
    [HideInInspector] public bool isFaceFront;

    // x, y
    private float x, y, rx, ry;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        oneTime = true;
        canMove = true;
    }

    // Update is called once per frame
    void Update()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        if (canMove)
        {
            if (Input.GetKeyDown(DEFKey) && Time.time > blockTimes + defenceDelayTime) //delay time
            {
                BlockAttack();
            }
        }
        ChracterFacing();
        GroundCheck();
    }

    private void ChracterFacing()
    {
        if (x != 0 && x < 0)
        {
            sr.flipX = true;
            rx = -1;
            ry = 0;
            anim.SetFloat("face", 0);
        }
        else if (x != 0 && x > 0)
        {
            sr.flipX = false;
            rx = 1;
            ry = 0;
            anim.SetFloat("face", 0);
        }
        else
        {
            if (y != 0 && y < 0)
            {
                rx = 0;
                ry = -1;
                isFaceFront = true;
                sr.flipX = false;
                anim.SetFloat("face", 1);
            }
            else if (y != 0 && y > 0)
            {
                rx = 0;
                ry = 1;
                isFaceFront = false;
                sr.flipX = false;
                anim.SetFloat("face", -1);
            }
        }
    }

    private void FixedUpdate()
    {
        if (canMove)
        {
            Vector3 moveDir = new Vector3(x, 0, y);
            anim.SetFloat("moveSpeed", x);
            anim.SetFloat("vmoveSpeed", y);
            rb.velocity = new Vector3(moveDir.x * speed * Time.deltaTime, rb.velocity.y, moveDir.z * speed * Time.deltaTime);
            if (isGrounded && Input.GetKeyDown(JumpKey))
            {
                rb.AddForce(Vector3.up * jumpForce);
            }
            if (isGrounded && Input.GetKeyDown(RollKey))
            {
                Rolling();
            }
        }
    }

    public void BlockAttack(bool Reflect = false)
    {
        isReflect = Reflect;
        anim.SetTrigger("Defence");
        canMove = false;
        blockTimes = Time.time + defenceTime;
        Debug.Log("blockTimes:" + blockTimes + "\nTime: " + Time.time);
        LeanTween.delayedCall(defenceTime, CanMove);
    }

    void CanMove()
    {
        canMove = true;
    }

    public void Rolling(bool isAttack = false, float damage = 0)
    {
        this.damage = damage;
        RollingATK = isAttack;
        canMove = false;
        anim.SetTrigger("Rolling");
        if(x == 0 && y == 0) 
            rb.velocity = new Vector3(rx, 0, ry) * speed * Time.deltaTime;
        else
            rb.velocity = new Vector3(x, 0, y) * speed * Time.deltaTime;

        LeanTween.delayedCall(rollingTime, CanMove);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (RollingATK && !canMove)
            collision.gameObject.GetComponent<IAttackable>().TakeDamage(damage);
    }

    public void SpeedUp(float upPower)
    {
        speed = baseSpeed * (1 + upPower);
    }

    public void ResetSpeed()
    {
        speed = baseSpeed;
    }

    [Header("Ground Check")]
    public float groundCheckRadius = 0.3f;
    public float groundCheckOffset = 0.1f;

    void GroundCheck()
    {
        oneTime = isGrounded;
        isGrounded = Physics.SphereCast(transform.position + Vector3.up * groundCheckOffset,
                                       groundCheckRadius,
                                       Vector3.down,
                                       out _,
                                       groundDist,
                                       terrainLayer);

        if (oneTime && !isGrounded)
            anim.SetTrigger("isReadyToFall");
        
        anim.SetBool("isGrounded", isGrounded);
    }
}
