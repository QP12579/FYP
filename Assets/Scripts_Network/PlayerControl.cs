using UnityEngine;
using Mirror;
using Cinemachine;

public class PlayerCharacterController : NetworkBehaviour
{
    // Movement variables
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    // Component references
    private Rigidbody rb;
    private Animator animator;
    private CinemachineVirtualCamera virtualCamera;

    // State tracking
    private bool isGrounded;
    private float horizontalInput;
    private float verticalInput;
    private bool jumpInput;

    void Awake()
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
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

    void Update()
    {
        // Only process input for the local player
        if (!isLocalPlayer) return;

        // Get movement input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump input (if your game has jumping)
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpInput = true;
        }

        // Check if grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        // Update animations if you have an animator
        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));
            animator.SetBool("IsGrounded", isGrounded);
        }

        // Handle character rotation based on movement direction
        if (horizontalInput != 0 || verticalInput != 0)
        {
            // For top-down or isometric view, you might want to rotate the character
            // If using a side view, you might just flip the model

            // Example for character that faces direction of movement:
            Vector3 lookDirection = new Vector3(horizontalInput, 0, verticalInput).normalized;
            if (lookDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    void FixedUpdate()
    {
        // Only move the local player
        if (!isLocalPlayer) return;

        // Movement in X and Z plane (for 2.5D)
        Vector3 movement = new Vector3(horizontalInput, 0, verticalInput).normalized * moveSpeed;

        // Apply movement
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);

        // Jump
        if (jumpInput)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpInput = false;
        }
    }

    // For network synchronization of position/movement
    [ClientRpc]
    public void RpcTeleportTo(Vector3 position)
    {
        transform.position = position;
    }

    [Command]
    private void CmdSyncPosition(Vector3 position)
    {
        // Optional: Add server validation here
        RpcTeleportTo(position);
    }
}