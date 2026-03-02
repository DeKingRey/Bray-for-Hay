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

    [Header("Pathfinding")]
    [SerializeField] private float waitTime;
    [SerializeField] private float knockbackRecoveryThreshold = 0.1f;
    [SerializeField] private float knockbackRecoveryTime = 3f;
    public float forceThreshold;

    [Header("Detection")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float detectionRadius;
    [SerializeField] private float proximityRadius;

    [Header("Attacking")]
    [SerializeField] protected float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] protected GameObject projectile;

    [Header("Shoot Delays")]
    [SerializeField] protected float minShootDistance;
    [SerializeField] protected float minDelayDuration;
    [SerializeField] protected float maxDelayDuration;

    protected NavMeshAgent agent;
    private EnemyPath path;
    private Rigidbody rb;

    private float elapsedTime = 0f;
    [HideInInspector] public bool isKnocked = false;
    protected bool canAttack = true;
    protected bool inProximity = false;
    public EnemyState state = EnemyState.Patrolling;
    
    protected Transform player;
    private Animator animator;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = GetComponent<EnemyPath>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        agent.destination = path.GetCurrentWaypoint();
        player = GameObject.FindWithTag("Player").transform;

        //rb.isKinematic = true;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.State != playState) return;

        if (isKnocked)
        {
            if (rb.velocity.magnitude <= knockbackRecoveryThreshold) RecoverFromKnockback();
            else return;
        }

        // Proximity radius will be quite a small sphere, checking if the player is extremely close to the player
        // This simulates the enemy 'feeling' the player
        inProximity = Physics.CheckSphere(transform.position, proximityRadius, playerLayer);
        if (inProximity) StartCoroutine(ShootDelay(minShootDistance));

        if (state == EnemyState.Patrolling) Patrolling();
        else
        {
            // Only non-blind enemies will change states (blind enemies have their own methods)
            if (state == EnemyState.Attacking) Attacking();
            else Chasing();
        }
    }

    void Patrolling()
    {
        // Once arrived at current waypoint, go to next one (after delay)
        if (agent.remainingDistance <= 0.1f)
        {
            agent.isStopped = true;
            animator.enabled = false;
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitTime)
            {
                elapsedTime = 0f;
                agent.isStopped = false;
                animator.enabled = true;
                agent.destination = path.GetNextWaypoint();
            }
        }
    }

    void Chasing()
    {
        // Will make the enemy stop when they are within attack range
        if (agent.remainingDistance >= attackRange) agent.destination = player.position;
        else agent.destination = transform.position;
    }

    /// Starts a delay for shooting
    /// The closer the player is the sooner the enemy will shoot
    protected IEnumerator ShootDelay(float distance)
    {
        float duration = Random.Range(minDelayDuration, maxDelayDuration);
        if (distance <= minShootDistance) duration = minDelayDuration;

        yield return new WaitForSeconds(duration);
        state = EnemyState.Attacking;
    }

    protected void Attacking()
    {
        agent.destination = transform.position;
        transform.LookAt(player);

        if (canAttack) Attack();
    }

    void Attack()
    {
        // Sends out a projectile towards the player
        Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * 32f, ForceMode.Impulse);

        // Resets attack
        Invoke(nameof(ResetAttack), attackCooldown);
        canAttack = false;
        state = EnemyState.Patrolling;
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    public void ApplyKnockback()
    {
        isKnocked = true;
        agent.enabled = false;

        rb.drag = 2f;
        rb.angularDrag = 2f;

        if (animator) animator.enabled = false;

        StartCoroutine(KnockedOutTimer());
    }

    private void RecoverFromKnockback()
    {
        agent.enabled = true;
        isKnocked = false;

        if (animator) animator.enabled = true;

        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        //rb.isKinematic = true;

        agent.Warp(transform.position); // Tells agent new position
    }

    IEnumerator KnockedOutTimer()
    {
        yield return new WaitForSeconds(knockbackRecoveryTime);

        if (isKnocked) RecoverFromKnockback();
    }

    void EnableRagdoll()
    {
        if (animator) animator.enabled = false;
        Collider[] colliders = this.gameObject.GetComponentsInChildren<Collider>();

        foreach (Collider c in colliders)
        {
            if (c.gameObject != this.gameObject)
            {
                c.isTrigger = false;
            }
        }

        Rigidbody[] rbs = this.gameObject.GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody rb in rbs)
        {
            if (rb.gameObject != this.gameObject)
            {
                rb.isKinematic = false;
            }
        }
    }

    void DisableRagdoll()
    {
        foreach (Rigidbody rb in GetComponentsInChildren<Rigidbody>())
        {
            if (rb.gameObject != this.gameObject)
            {
                rb.isKinematic = true;
            }
        }

        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.gameObject != this.gameObject)
            {
                c.isTrigger = true;
            }
        }

        transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);

        if (animator) animator.enabled = true;

        // Resets bones to default pose
        if (animator) animator.Rebind();
        if (animator) animator.Update(0f);
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