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
    private Animator anim;
    private bool isGrounded;
    [HideInInspector] public bool isFaceFront;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        rb = gameObject.GetComponent<Rigidbody>();
        sr = gameObject.GetComponent<SpriteRenderer>();
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

        if (x != 0 && x < 0)
        {
            sr.flipX = true;
        }
        else if (x != 0 && x > 0) 
        {
            sr.flipX = false;
        }
        if (y != 0 && y < 0)
        {
            isFaceFront = true;
        }
        else if (y != 0 && y > 0) 
        {
            isFaceFront = false;
        }

        if(isGrounded && Input.GetKeyDown(KeyCode.Z))
        {
            anim.SetTrigger("Rolling");
            rb.AddForce(new Vector3(x, 0, y) * speed);
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
        anim.SetBool("isGrounded", isGrounded);
    }
}
