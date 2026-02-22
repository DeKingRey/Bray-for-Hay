using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameManager.GameState playState;
    [SerializeField] private GameManager.GameState gameOverState;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;

    [Header("Jumping")]
    [SerializeField] private float jumpMultiplier = 40f;
    [SerializeField] private float maxJumpTime = 0.25f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float fallMultiplier;

    [Header("Sound")]
    [SerializeField] private float createSoundInterval = 0.15f;
    [SerializeField] private float sprintSoundRadius = 10f;
    [SerializeField] private float walkSoundRadius = 5f;

    private float soundTimer = 0f;

    private CharacterController controller;

    private Vector3 moveDirection;

    private bool canMove = true;
    private bool isRunning;
    private bool isMoving;
    private bool isJumping;
    private bool isFalling;

    private float jumpPower;


    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (GameManager.Instance.State != playState) return;

        HandleMovement();

        soundTimer += Time.deltaTime;
        if (soundTimer >= createSoundInterval)
        {
            // Radius dependent on whether sprinting or walking
            float radius = isMoving ? (isRunning ? sprintSoundRadius : walkSoundRadius) : 0;
            SoundManager.Instance.CreateSoundBubble(radius);
            
            soundTimer = 0f;
        }
    }

    void HandleMovement()
    {
        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Press Left Shift to run
        isRunning = Input.GetKey(KeyCode.LeftShift);

        // Current speed is dependent on whether the player is sprinting (speed is then multiplied by input)
        float currentSpeedX = canMove ? (isRunning ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical") : 0;
        float currentSpeedZ = canMove ? (isRunning ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * currentSpeedX) + (right * currentSpeedZ);

        isMoving = currentSpeedX != 0 || currentSpeedZ != 0;

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

    void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Weapon")) GameManager.Instance.ChangeState(gameOverState, 0);

        if (obj.CompareTag("Hay"))
        {
            Debug.Log("hay!");
            Destroy(obj.gameObject);
        }
    }
}
