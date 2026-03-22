using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Patrolling,
        Investigating,
        Attacking
    }

    [SerializeField] private GameManager.GameState playState;
    public EnemyState state = EnemyState.Patrolling;
    [SerializeField] private Animator indicatorAnim;
    
    [Header("Force Settings")]
    public Rigidbody hips;

    [Tooltip("Max time enemy will be knocked out")]
    [SerializeField] private float maxKnockbackTime = 3f;

    [SerializeField] private float maxExpectedForce = 1f;

    [Tooltip("Minimum Force to Apply Knockback")]
    public float forceThreshold;

    [Space(10)]

    [Header("Pathfinding")]
    [Tooltip("How long the enemy will wait at a waypoint")]
    [SerializeField] private float waitTime;

    [Tooltip("How fast the enemy rotates - in degrees per second")]
    [SerializeField] private float rotationSpeed = 90f;

    [Space(10)]

    [Header("Detection")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float detectionRadius;

    [Tooltip("Radius for enemy to notice player regardless of sight/hearing (feeling)")]
    [SerializeField] private float proximityRadius;

    [Tooltip("Multiplies hearing/sight when on alert")]
    [SerializeField] protected float alertMultiplier = 1.5f;
    [SerializeField] protected float alertDuration = 3f;

    [Space(10)]

    [Header("Attacking")]
    [SerializeField] protected float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] protected GameObject projectile;
    [SerializeField] private Transform shotPoint;

    [Space(10)]

    [Header("Shoot Delays")]
    [SerializeField] protected float minShootDistance;
    [SerializeField] protected float minDelayDuration;
    [SerializeField] protected float maxDelayDuration;

    [Space(10)]

    [Header("Sound Effects")]
    [SerializeField] private AudioClip alertSfx;
    [SerializeField] private AudioClip suspectSfx;
    [SerializeField] private AudioClip horseshoeSfx;
    [SerializeField] private AudioClip ragdollSfx;
    [SerializeField] private AudioClip[] trotSfxs;
    [SerializeField] private AudioClip[] gallopSfxs;
    [SerializeField] private AudioSource walkSourceSfx;

    protected NavMeshAgent agent;
    private EnemyPath path;
    private Rigidbody rb;
    private Collider enemyCollider;

    private float elapsedTime = 0f;
    [HideInInspector] public bool isKnocked = false;
    protected bool canAttack = true;
    protected bool inProximity = false;
    protected bool isAlert = false;
    
    protected Transform player;
    private Animator animator;
    private float walkSoundTimer = 0f;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = GetComponent<EnemyPath>();
        rb = GetComponent<Rigidbody>();
        enemyCollider = GetComponent<Collider>();
        animator = GetComponentInChildren<Animator>();

        agent.destination = path.GetCurrentWaypoint();
        player = GameObject.FindWithTag("Player").transform;

        DisableRagdoll();
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.State != playState) return;

        if (isKnocked) return;

        // Proximity radius will be quite a small sphere, checking if the player is extremely close to the player
        // This simulates the enemy 'feeling' the player
        inProximity = Physics.CheckSphere(transform.position, proximityRadius, playerLayer);
        if (inProximity && state != EnemyState.Attacking) ChangeEnemyState(EnemyState.Attacking);

        if (state == EnemyState.Patrolling) Patrolling();
        else
        {
            // Only non-blind enemies will change states (blind enemies have their own methods)
            if (state == EnemyState.Attacking) Attacking();
            else Chasing();
        }
    }

    public void ChangeEnemyState(EnemyState newState)
    {
        if (newState == state) return;
        state = newState;

        switch (state)
        {
            case EnemyState.Patrolling:
                break;
            case EnemyState.Investigating:  
                SoundManager.Instance.PlayAudio(suspectSfx, 0.5f, transform);
                break;
            case EnemyState.Attacking:
                break;
        }
    }

    void Patrolling()
    {
        UpdateIndicators(true, false, false); // Sets indicator to idle
        UpdateAnimator(false, true, false); // Sets anim to walking

        // Once arrived at current waypoint, go to next one (after delay)
        if (agent.remainingDistance <= 0.1f)
        {
            agent.isStopped = true;
            elapsedTime += Time.deltaTime;
            
            UpdateAnimator(true, false, false); // Sets anim to idle
            walkSoundTimer = 0f;

            if (elapsedTime >= waitTime)
            {
                elapsedTime = 0f;
                agent.isStopped = false;
                animator.enabled = true;
                agent.destination = path.GetNextWaypoint();
            }
        }
        else {
            agent.isStopped = false;
            PlayWalkAudio(trotSfxs);
        }
    }

    void Chasing()
    {
        UpdateIndicators(false, true, false); // Sets indicator to suspicious

        // Will make the enemy stop when they are within attack range
        if (Vector3.Distance(transform.position, player.position) >= attackRange)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            PlayWalkAudio(gallopSfxs);
            UpdateAnimator(false, false, true); // Sets anim to running
        }
        else
        {
            agent.isStopped = true;
            walkSoundTimer = 0f;
            UpdateAnimator(true, false, false); // Sets anim to idle
        }
    }

    protected void Attacking()
    {
        agent.destination = transform.position;
        UpdateAnimator(true, false, false); // Sets anim to idle

        if (canAttack) StartCoroutine(RotateToPlayer());
    }


    /// When attacking, the enemy rotates to the player before shooting
    private IEnumerator RotateToPlayer()
    {
        canAttack = false;
        UpdateIndicators(false, true, false); // Sets indicator to suspicious
        UpdateAnimator(true, false, false); // Sets anim to idle

        SoundManager.Instance.PlayAudio(suspectSfx, 0.5f, transform);

        // Gets direction towards player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        // Calculates target rotation with the player direction
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Rotates smoothly to face the player until target rotation is more or less reached
        while (Quaternion.Angle(transform.rotation, targetRotation) > 2f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRotation;
        
        float playerDistance = Vector3.Distance(transform.position, player.position);
        StartCoroutine(ShootDelay(playerDistance));
    }

    /// Starts a delay for shooting
    /// The closer the player is the sooner the enemy will shoot
    private IEnumerator ShootDelay(float distance)
    {
        UpdateIndicators(false, false, true); // Sets indicator to alert
        SoundManager.Instance.PlayAudio(alertSfx, 1f, transform);

        float duration = Random.Range(minDelayDuration, maxDelayDuration);
        if (distance <= minShootDistance) duration = minDelayDuration;

        yield return new WaitForSeconds(duration);
        animator.SetTrigger("Attack");
    }

    public void Attack()
    {
        // Sends out a projectile towards the player
        Rigidbody rb = Instantiate(projectile, shotPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
        Vector3 playerDir = (player.position - transform.position).normalized;
        rb.velocity = playerDir * 10f;
        SoundManager.Instance.PlayAudio(horseshoeSfx, 0.75f, rb.gameObject.transform);

        isAlert = true;

        ChangeEnemyState(EnemyState.Patrolling);

        // Resets attack
        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    public void ApplyKnockback(float forceAmount)
    {
        if (isKnocked) return;

        isKnocked = true;
        
        EnableRagdoll();
        UpdateIndicators(true, false, false); // Sets indicator anim to idle

        // Scales knockout duration with forceamount
        float normalizedForce = Mathf.Clamp01(forceAmount / maxExpectedForce);
        float koDuration = maxKnockbackTime * normalizedForce;
        StartCoroutine(KnockedOutTimer(koDuration));
    }

    private void RecoverFromKnockback()
    {
        DisableRagdoll();

        isKnocked = false;
    }

    IEnumerator KnockedOutTimer(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (isKnocked) RecoverFromKnockback();
    }

    void EnableRagdoll()
    {
        // Ensures root doesn't fight with ragdoll
        agent.enabled = false;
        enemyCollider.isTrigger = true;
        rb.isKinematic = true;
        if (animator) animator.enabled = false;
        UpdateIndicators(true, false, false); // Sets indicator to idle

        SoundManager.Instance.PlayAudio(ragdollSfx, 1f, transform);

        // Enables all rbs in ragdoll
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            if (rb.gameObject != gameObject)
            {
                rb.isKinematic = false;
            }
        }

        // Enables all ragdoll colliders
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.gameObject != gameObject)
            {
                c.isTrigger = false;
            }
        }
    }

    void DisableRagdoll()
    {
        Vector3 ragdollPosition = hips.position;

        // Disables rbs in ragdoll
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            if (rb.gameObject != gameObject)
            {
                rb.isKinematic = true;
            }
        }

        // Disbales colliders in ragdoll
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.gameObject != gameObject)
            {
                c.isTrigger = true;
            }
        }

        // Reforms position and components of root
        transform.position = ragdollPosition;

        rb.isKinematic = true;
        enemyCollider.isTrigger = false;

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        // Resets bones to default pose
        if (animator) 
        {
            animator.enabled = true;
            animator.Rebind();
            animator.Update(0f);
        }

        agent.enabled = true;
        agent.Warp(transform.position);
    }

    void UpdateIndicators(bool isIdle, bool isSuspicious, bool isAlert)
    {
        indicatorAnim.SetBool("isIdle", isIdle);
        indicatorAnim.SetBool("isSuspicious", isSuspicious);
        indicatorAnim.SetBool("isAlert", isAlert);
    }

    void UpdateAnimator(bool isIdle, bool isWalking, bool isRunning)
    {
        animator.SetBool("isIdle", isIdle);
        animator.SetBool("isWalking", isWalking);
        animator.SetBool("isRunning", isRunning);
    }

    void PlayWalkAudio(AudioClip[] walkSfxs)
    {
        walkSoundTimer -= Time.deltaTime;
        if (walkSoundTimer <= 0)
        {
            walkSourceSfx.clip = walkSfxs[Random.Range(0, walkSfxs.Length)];
            walkSoundTimer = walkSourceSfx.clip.length;
            walkSourceSfx.Play();
        }
    }

    void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Fall Zone"))
        {
            maxKnockbackTime = Mathf.Infinity;
            ApplyKnockback(Mathf.Infinity);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);
    }
}