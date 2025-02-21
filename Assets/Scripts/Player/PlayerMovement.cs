using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed;
    public float groundDist;

    public LayerMask terrainLayer;
    private Rigidbody rb;
    [HideInInspector] public SpriteRenderer sr;
    [HideInInspector] public Animator anim;
    private bool isGrounded;
    private bool oneTime;
    [HideInInspector] public bool isFaceFront;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
        sr = gameObject.GetComponent<SpriteRenderer>();
        oneTime = true;
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(x, 0, y);
        anim.SetFloat("moveSpeed", x);
        anim.SetFloat("vmoveSpeed", y);
        rb.velocity = new Vector3( moveDir.x * speed * Time.deltaTime, rb.velocity.y, moveDir.z * speed * Time.deltaTime);

        if(isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            anim.SetTrigger("Rolling");
            rb.AddForce(new Vector3(x, 0, y) * speed);
        }
        if(isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * 500);
        }

        ChracterFacing(x, y);
    }

    private void ChracterFacing(float x, float y)
    {
        if (x != 0 && x < 0)
        {
            sr.flipX = true;
            anim.SetFloat("face", 0);
        }
        else if (x != 0 && x > 0)
        {
            sr.flipX = false;
            anim.SetFloat("face", 0);
        }
        else
        {
            if (y != 0 && y < 0)
            {
                isFaceFront = true;
                sr.flipX = false;
                anim.SetFloat("face", 1);
            }
            else if (y != 0 && y > 0)
            {
                isFaceFront = false;
                sr.flipX = false;
                anim.SetFloat("face", -1);
            }
            else if (x != 0 && y == 0)
            {
                anim.SetFloat("face", 0);
            }
        }
    }

    private void FixedUpdate()
    {
        GroundCheck();
    }

    void GroundCheck()
    {
        RaycastHit hit;
        Vector3 castPos = transform.position;

        isGrounded = Physics.Raycast(castPos, Vector3.down, out hit, groundDist, terrainLayer);
        if (oneTime != isGrounded && !isGrounded)
        {
            anim.SetTrigger("isReadyToFall");
            oneTime = false;
        }
        if (isGrounded) oneTime = true;
        anim.SetBool("isGrounded", isGrounded);
    }
}
