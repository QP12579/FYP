using Cinemachine;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    public float speed;
    private float baseSpeed => speed;
    private float abilitySpeed = 0;
    public float groundDist;
    public float jumpForce = 500;

    public LayerMask terrainLayer;

    [SerializeField] private GameObject spawn;
    private Vector3 SpawnPoint;
    private float LowerYPosi = -1;

    [Header("KeyCode")]
    [SerializeField] private InputActionAsset inputActions;

    [Header("Defense/Rolling")]
    public bool isReflect = false;
    public float reflectDamageMultiplier = 1.0f;
    public bool RollingATK = false;

    public float defenceTime = 0.5f;
    public float defenceDelayTime = 1f;
    public float rollingTime = 0.5f;
    public float rollingSpeed = 1.3f;
    [HideInInspector] public float abilityRollingSpeed = 0;

    [HideInInspector] public float blockPercentage = 0.5f;
    [HideInInspector] public float blockTimes;
    [HideInInspector] public float damage;

    private Rigidbody rb;
     public SpriteRenderer sr;
     public Animator anim;
    [SerializeField] private bool isGrounded;

    private bool jumpRequest = false;
    private bool oneTime;
    public bool canMove;
    [HideInInspector] public bool isFaceFront;

    [Header(" Audio Clip")]
    [SerializeField] private AudioClip Running_SFX;
    [SerializeField] private AudioClip Jump_SFX;
    [SerializeField] private AudioClip Rolling_SFX;

    private CinemachineVirtualCamera virtualCamera;

    // x, y
    private float x, y, rx, ry;

   

    void Awake()
    {
        anim = gameObject.GetComponentInParent<Animator>();
        rb = gameObject.GetComponentInParent<Rigidbody>();
        sr = gameObject.GetComponentInParent<SpriteRenderer>();
        oneTime = true;
        canMove = true;
        if (spawn == null)
        {
            //spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
        }
           // SpawnPoint = spawn.transform.position;
    }
    public override void OnStartAuthority()
    {
        base.OnStartAuthority();

        // Setup camera only for local player
        StartCoroutine(SetupCameraDelayed());
    }

    private System.Collections.IEnumerator SetupCameraDelayed()
    {
        yield return new WaitForSeconds(0.1f);

        virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();

        if (virtualCamera != null)
        {
            virtualCamera.Follow = this.transform;
            Debug.Log("Camera assigned to local player");
        }
        else
        {
            Debug.LogError("No CinemachineVirtualCamera found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (!isLocalPlayer)
        {
           
            return;
        }

        bool isActive = this.gameObject.activeSelf;
        if (!isActive)
        {
            this.gameObject.SetActive(true);
        }

        // x = inputActions.FindAction(Constraints.InputKey.Move).ReadValue<Vector2>().x;
        // y = inputActions.FindAction(Constraints.InputKey.Move).ReadValue<Vector2>().y;

        // if  (isGrounded && canMove && inputActions.FindAction(Constraints.InputKey.Jump).triggered)
        // {
        //     jumpRequest = true;
        //     if(Jump_SFX!=null&&SoundManager.instance!=null)
        //         SoundManager.instance.PlaySFX(Jump_SFX);
        // }

        // if (canMove)
        // {
            
        //     if (inputActions.FindAction(Constraints.InputKey.Defense).triggered && Time.time > blockTimes + defenceDelayTime) //delay time
        //     {
        //         BlockAttack();
        //     }
        // }
        ChracterFacing();
        GroundCheck();
    }

    public void OnMove (InputAction.CallbackContext context)
    {
        x = context.ReadValue<Vector2>().x;
        y = context.ReadValue<Vector2>().y;

        Debug.Log($"Move: [{x}, {y}]");
    }

    public void OnJump (InputAction.CallbackContext context)
    {
        if  (isGrounded && canMove)
        {
            jumpRequest = true;
            if(Jump_SFX!=null&&SoundManager.instance!=null)
                SoundManager.instance.PlaySFX(Jump_SFX);
        }
    }

    public void OnDefense (InputAction.CallbackContext context)
    {
        if (canMove && Time.time > blockTimes + defenceDelayTime) //delay time
        {
            BlockAttack();
        }
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
            if (isGrounded && jumpRequest)
            {
                rb.AddForce(Vector3.up * jumpForce);
                jumpRequest = false;
            }
            else if (!isGrounded)
            {
                jumpRequest = false;
            }
            
            if (isGrounded && inputActions.FindAction(Constraints.InputKey.Roll).triggered)
            {
                Rolling();
            }
        }
    }

    public void BlockAttack(bool Reflect = false, bool isSkill = false)
    {
        isReflect = Reflect;
        anim.SetTrigger("Defence");
        if(!isSkill)
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
            rb.velocity = new Vector3(x, 0, y) * speed * (1 + rollingSpeed + abilityRollingSpeed) * Time.deltaTime;

        if (Rolling_SFX != null && SoundManager.instance != null)
            SoundManager.instance.PlaySFX(Rolling_SFX);
        LeanTween.delayedCall(rollingTime, CanMove);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (RollingATK && !canMove)
            collision.gameObject.GetComponent<IAttackable>().TakeDamage(gameObject.transform.position, damage);
    }

    public void SpeedChange()
    {
        speed = baseSpeed * (1 + abilitySpeed + PlayerBuffSystem.instance.GetBuffValue(BuffType.MoveSpeedUp)
            - PlayerBuffSystem.instance.GetDeBuffValue(DeBuffType.Slow));
    }

    public void AbilitySpeedUp(float upP)
    {
        abilitySpeed += upP;
    }

    public void ResetAbilitySpeed()
    {
        abilityRollingSpeed = 0;
        abilitySpeed = 0;
    }

    public void ResetSpeed()
    {
        speed = baseSpeed * (1 + abilitySpeed);
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
        if(!isGrounded)
            if(transform.position.y < LowerYPosi)
                GoBackToSpawnPoint();

    }

    void GoBackToSpawnPoint()
    {
        transform.position = SpawnPoint;
    }

    public void Dizziness(float time)
    {
        canMove = false;
        anim.SetTrigger("Hurt");
        LeanTween.delayedCall(time, CanMove);
    }
}
