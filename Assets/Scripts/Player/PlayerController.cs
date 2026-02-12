using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Jumping")]
    [SerializeField] private float jumpMultiplier = 40f;
    [SerializeField] private float maxJumpTime = 0.25f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float fallMultiplier;

    private CharacterController controller;

    private Vector3 moveDirection;
    private bool canMove = true;
    private bool isJumping;
    private bool isFalling;
    private float jumpPower;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Current speed is dependent on whether the player is sprinting (speed is then multiplied by input)
        float currentSpeedX = canMove ? (isRunning ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float currentSpeedZ = canMove ? (isRunning ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * currentSpeedX) + (right * currentSpeedZ);

        bool isMoving = currentSpeedX != 0 || currentSpeedZ != 0;

        #endregion

        #region Handles Jumping
        if (Input.GetButton("Jump") && canMove && controller.isGrounded)
        {
            isJumping = true;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Lets player hold jump to jump higher
        if (isJumping)
        {
            jumpPower += Time.deltaTime;
            moveDirection.y = jumpPower * jumpMultiplier;
            if (jumpPower >= maxJumpTime || !Input.GetButton("Jump"))
            {
                isFalling = true;
                isJumping = false;
                jumpPower = 0;
            }
        }
        
        // Applies gravity when in air, increases speed if falling  
        if (!controller.isGrounded)
        {
            moveDirection.y -= gravity * (isFalling ? fallMultiplier : 1f) * Time.deltaTime;
        } else isFalling = false;

        #endregion

        controller.Move(moveDirection * Time.deltaTime);
    }
}
