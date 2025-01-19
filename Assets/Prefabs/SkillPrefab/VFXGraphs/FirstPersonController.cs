using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public int FPS = 120;
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    private Vector3 moveDirection = Vector3.zero;

    float yR, xR, cyR, cxR, yRv, xRv;
    float lookSensitivity = 2;
    float lookSmoothnes = 0.1f;

    void Start()
    {
        Application.targetFrameRate = FPS;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        
    }

    void Update()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if(Input.GetButton("Jump"))
                moveDirection.y = jumpSpeed;
        }
        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);

        yR += Input.GetAxis("Mouse X") * lookSensitivity;
        xR -= Input.GetAxis("Mouse Y") * lookSensitivity;
        xR = Mathf.Clamp(xR, -80, 100);
        cxR = Mathf.SmoothDamp(cxR, xR, ref xRv, lookSmoothnes);
        cyR = Mathf.SmoothDamp(cyR, yR, ref yRv, lookSmoothnes);
        transform.rotation = Quaternion.Euler(xR, yR, 0);
    }
}
