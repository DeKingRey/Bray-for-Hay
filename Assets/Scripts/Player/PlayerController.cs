using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FirstGearGames.SmoothCameraShaker;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameManager.GameState playState;
    [SerializeField] private GameManager.GameState gameOverState;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float defaultSprintSpeed;
    [SerializeField] private float slideStrength = 8f;

    [Space(10)]

    [Header("Crouch Settings")]
    [SerializeField] private float crouchSpeed;

    [Tooltip("Controller heights when crouched/uncrouched, 0 is standing, 1 is crouched")]
    [SerializeField] private float[] crouchHeights;

    [Tooltip("Camera y pos's when crouched/uncrouched, 0 is standing, 1 is crouched")]
    [SerializeField] private float[] crouchCameraY;


    [Tooltip("How much smaller the player gets when crouching")]
    [SerializeField] private float crouchScaleY;

    [Space(10)]

    [Header("Stamina Settings")]

    [Tooltip("The max stamina - the y intercept")]
    [SerializeField] private float maxStamina;
    
    [Tooltip("Delay before stamina starts regenerating")]
    [SerializeField] private float regainStaminaDelay;

    [Tooltip("Change in stamina per second when losing stamina")]
    [SerializeField] private float staminaDrainRate;

    [Tooltip("Change in stamina per second when gaining stamina")]
    [SerializeField] private float staminaRegenRate;
    [SerializeField] private float sliderSmoothSpeed = 10f;

    [Space(10)]

    [Header("Jump Settings")]
    [SerializeField] private float jumpMultiplier = 40f;
    [SerializeField] private float maxJumpTime = 0.25f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float jumpStaminaLossMultiplier;

    [Space(10)]

    [Header("Audio Detection Settings")]
    [SerializeField] private float createSoundInterval = 0.15f;
    [SerializeField] private float sprintSoundRadius = 10f;
    [SerializeField] private float walkSoundRadius = 5f;
    [SerializeField] private float crouchSoundRadius = 0f;

    [Space(10)]

    [Header("Sound Effects")]
    [SerializeField] private AudioSource walkSourceSfx;
    [SerializeField] private AudioClip[] walkSfxs;
    [SerializeField] private AudioClip[] sprintSfxs;
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField] private AudioClip landSfx;

    [Tooltip("How long the player must be falling to play the land SFX")]
    [SerializeField] private float fallThresholdTime = 0.5f;

    [Header("Juice")] 

    [Space(10)]
    [SerializeField] private ShakeData landShake;

    [Space(5)]

    [Tooltip("Time it takes for speed lines to appear while falling")]
    [SerializeField] private float speedLinesTime;
    [SerializeField] private ParticleSystem speedParticles;

    [HideInInspector] public bool isHidden;

    private float walkSfxTimer = 0f;
    private float soundTimer = 0f;

    private CharacterController controller;
    private Animator anim;

    private Vector3 moveDirection;

    private Slider staminaSlider; 
    private float currentStamina;
    private float smoothedSprintValue;
    
    private bool staminaDelayActive = false;
    private bool canRegainStamina = false;

    private bool canMove = true;
    private bool isSprinting;
    private bool isCrouching;
    private bool isMoving;
    private bool isJumping;
    private bool isFalling;

    private float jumpPower;
    private bool canPlayLandSfx;
    private float fallTime = 0f;
    private float sprintTime = 0f;
    private Transform camHolder;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        staminaSlider = GameObject.FindWithTag("Stamina Slider").GetComponent<Slider>();
        anim = GetComponentInChildren<Animator>();
        camHolder = GameObject.FindWithTag("Camera Holder").transform;

        currentStamina = maxStamina;

        speedParticles.Stop();
    }

    void Update()
    {
        if (GameManager.Instance.State != playState) return;

        HandleMovement();

        soundTimer += Time.deltaTime;
        if (soundTimer >= createSoundInterval && !isCrouching)
        {
            // Radius dependent on whether sprinting or walking
            float radius = isMoving ? (isSprinting ? sprintSoundRadius : isCrouching ? crouchSoundRadius : walkSoundRadius) : 0;
            SoundManager.Instance.CreateSoundBubble(radius);
            
            soundTimer = 0f;
        }
    }

    void HandleMovement()
    {
        #region Handles Movement
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Left Shift to run, left control to crouch
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        isCrouching = Input.GetKey(KeyCode.LeftControl);

        // Ensures you don't do two movement techniques at once
        if (isSprinting) isCrouching = false;
        if (isCrouching) isSprinting = false;

        float sprintSpeed = defaultSprintSpeed;
        if (currentStamina <= 0.25f) sprintSpeed = walkSpeed;

        // Current speed is dependent on whether the player is sprinting/crouching (speed is then multiplied by input)
        float currentSpeedX = canMove ? (isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : walkSpeed) 
                                            * Input.GetAxis("Vertical") : 0;
        float currentSpeedZ = canMove ? (isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : walkSpeed)
                                            * Input.GetAxis("Horizontal") : 0;
        float movementDirectionY = moveDirection.y;
        moveDirection = (forward * currentSpeedX) + (right * currentSpeedZ);

        isMoving = currentSpeedX != 0 || currentSpeedZ != 0;

        #endregion

        #region Walk SFX
        
        if (isMoving && controller.isGrounded)
        {
            walkSfxTimer -= Time.deltaTime;
            if (walkSfxTimer <= 0)
            {
                // Changes audio depending on whether the player is sprinting, crouching, or walking
                if (isSprinting) 
                    walkSourceSfx.clip = sprintSfxs[Random.Range(0, sprintSfxs.Length)];
                else 
                    walkSourceSfx.clip = walkSfxs[Random.Range(0, walkSfxs.Length)];
                if (isCrouching)
                    walkSourceSfx.volume = 0.1f;
                else 
                    walkSourceSfx.volume = 0.3f;
    
                walkSfxTimer = walkSourceSfx.clip.length;
                walkSourceSfx.Play();
            }
        }
        else
        {
            walkSfxTimer = 0f;
            walkSourceSfx.Stop();
        }
        #endregion

        HandleCrouch();

        #region Handles Sprinting

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        // Decreases stamina while sprinting
        if (isSprinting && currentStamina > 0f)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;

            sprintTime += Time.deltaTime;

            if (isMoving) 
                UpdateAnimator(false, false, true); // Sets anim to running
            else UpdateAnimator(true, false, false); // Sets anim to idle
        } 
        else
        {
            sprintTime = 0f;
            if (isMoving) 
                UpdateAnimator(false, true, false); // Sets anim to walking
            else UpdateAnimator(true, false, false); // Sets anim to idle

            // Regains stamina after a short delay, stops if stamina has reached max
            if (!staminaDelayActive && currentStamina < maxStamina)
                StartCoroutine(RegainStaminaDelay());

            if (canRegainStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;

                // Stops gaining stamina when current has reached max
                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                    canRegainStamina = false;
                }
            }
        }

        // Smoothly increases the stamina slider
        smoothedSprintValue = Mathf.Lerp(smoothedSprintValue, currentStamina, sliderSmoothSpeed * Time.deltaTime);
        staminaSlider.value = Mathf.Clamp(smoothedSprintValue, 0f, staminaSlider.maxValue);

        #endregion

        #region Handles Slopes

        Vector3 slideDir;
        if (controller.isGrounded && IsOnSteepSlope(out slideDir))
        {
            moveDirection += slideDir * slideStrength;
        }

        #endregion

        #region Handles Jumping
        if (Input.GetButton("Jump") && canMove && controller.isGrounded && currentStamina >= 0.5f)
        {
            isJumping = true;
            SoundManager.Instance.PlayAudio(jumpSfx, 1f, transform);
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

            currentStamina -= staminaDrainRate * jumpStaminaLossMultiplier * Time.deltaTime;

            if (jumpPower >= maxJumpTime || !Input.GetButton("Jump") || currentStamina <= 0.25f)
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
            canPlayLandSfx = true;
            fallTime += Time.deltaTime;

            if (fallTime > speedLinesTime && !isJumping) speedParticles.Play();
        } else 
        {
            isFalling = false;
        }

        if (controller.isGrounded && canPlayLandSfx)
        {
            // Volume and shake mag. of land is dependent on how long the players been falling
            float landVolume = 0f;
            float landShakeMagnitude = 0f;
            if (fallTime >= fallThresholdTime)
            {
                landVolume = Mathf.Clamp01(fallTime * 0.5f);
                landShakeMagnitude = Mathf.Clamp(fallTime, 0f, 5f);
            }
            SoundManager.Instance.PlayAudio(landSfx, landVolume, transform, 0);

            // Screen Shake
            ShakerInstance instance = CameraShakerHandler.Shake(landShake);
            instance.MultiplyMagnitude(landShakeMagnitude * 1.5f, -1);

            canPlayLandSfx = false;
            fallTime = 0f;
        }

        #endregion

        #region Speed Lines

        // Adds sprint lines either if player is sprinting or the player is falling
        if (isSprinting && currentStamina > 0f || !controller.isGrounded && !isJumping)
        {  
            // Only plays if player has been sprinting/falling for a certain amount of time
            if (fallTime > speedLinesTime || sprintTime > speedLinesTime)
                speedParticles.Play();
        } else speedParticles.Stop();

        #endregion

        controller.Move(moveDirection * Time.deltaTime);
    }

    void HandleCrouch()
    {
        // Gets target height/camera y pos depending on whether crouching or not
        float targetHeight = isCrouching ? crouchHeights[1] : crouchHeights[0];
        float targetCameraY = isCrouching ? crouchCameraY[1] : crouchCameraY[0];

        // Smoothly updates controller height
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 10f);

        // Smoothly moves the camera
        Vector3 camLocal = camHolder.localPosition;
        camLocal.y = Mathf.Lerp(camLocal.y, targetCameraY, Time.deltaTime * 10f);
        camHolder.localPosition = camLocal;
    }

    IEnumerator RegainStaminaDelay()
    {
        float elapsedTime = 0f;
        staminaDelayActive = true;

        while (elapsedTime <= regainStaminaDelay)
        {
            if (isSprinting)
            {
                staminaDelayActive = false;
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        staminaDelayActive = false;
        canRegainStamina = true;
    }

    /// Checks if the player is on a steep slope
    /// If so the player will slide down it (depending on direction)
    bool IsOnSteepSlope(out Vector3 slideDirection)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2f))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            
            if (angle > controller.slopeLimit)
            {
                slideDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal);
                return true;
            }
        }

        slideDirection = Vector3.zero;
        return false;
    }

    void UpdateAnimator(bool isIdle, bool isWalking, bool isRunning)
    {
        anim.SetBool("isIdle", isIdle);
        anim.SetBool("isWalking", isWalking);
        anim.SetBool("isRunning", isRunning);
    }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.transform.root == transform.root) return;

        if (obj.CompareTag("Weapon") || obj.CompareTag("Fall Zone"))
            GameManager.Instance.ChangeState(gameOverState, 0);
        if (obj.CompareTag("Bush")) isHidden = true;
    }

    void OnTriggerExit(Collider obj)
    {
        if (obj.CompareTag("Bush")) isHidden = false;
    }
}
