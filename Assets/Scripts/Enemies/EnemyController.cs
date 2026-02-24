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
    [SerializeField] private float knockbackRecoveryThreshold = 0f;
    [SerializeField] private float knockbackRecoveryTime = 3f;
    public float forceThreshold;

    [Header("Detection")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float detectionRadius;

    [Header("Combat")]
    [SerializeField] protected float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] protected GameObject projectile;

    protected NavMeshAgent agent;
    private EnemyPath path;
    private Rigidbody rb;

    private float elapsedTime = 0f;
    [HideInInspector] public bool isKnocked = false;
    protected bool playerInRange = false;
    protected bool canSensePlayer = false;
    protected bool canAttack = true;
    protected EnemyState state = EnemyState.Patrolling;
    
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

        DisableRagdoll();
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.State != playState) return;

        if (isKnocked)
        {
            if (rb.velocity.magnitude <= knockbackRecoveryThreshold) RecoverFromKnockback();
            else return;
        }

        if (state == EnemyState.Patrolling) Patrolling();
        else
        {
            playerInRange = true;

            // Only non-blind enemies will change states (blind enemies have their own methods)
            if (!inAttackRange && canSensePlayer || canHearPlayer == 1) Chasing();
            else if (inAttackRange && canSensePlayer || canHearPlayer == 2) Attacking();
            else Patrolling();
        } else playerInRange = false;
    }

    public void ApplyKnockback()
    {
        isKnocked = true;
        agent.enabled = false;
        EnableRagdoll();

        StartCoroutine(KnockedOutTimer());
    }

    private void RecoverFromKnockback()
    {
        isKnocked = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        agent.enabled = true;

        DisableRagdoll();
        agent.Warp(transform.position); // Tells agent new position
    }

    IEnumerator KnockedOutTimer()
    {
        yield return new WaitForSeconds(knockbackRecoveryTime);

        RecoverFromKnockback();
    }

    void Patrolling()
    {
        canSensePlayer = false;

        if (agent.remainingDistance <= 0.1f)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitTime)
            {
                elapsedTime = 0f;
                agent.destination = path.GetNextWaypoint();
            }
        }
    }

    void Chasing()
    {
        Debug.Log("chasing");

        // Will make the enemy stop when they are within attack range
        if (agent.remainingDistance >= attackRange) agent.destination = player.position;
        else agent.destination = transform.position;
    }

    protected void Attacking()
    {
        Debug.Log("attacking");

        agent.destination = transform.position;

        transform.LookAt(player);

        if (canAttack) Attack();
    }

    protected void Attack()
    {
        // Sends out a projectile towards the player
        Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * 32f, ForceMode.Impulse);

        Invoke(nameof(ResetAttack), attackCooldown);
        canAttack = false;

        canHearPlayer = 0;
    }

    void ResetAttack()
    {
        canAttack = true;
    }

    void EnableRagdoll()
    {
        animator.enabled = false;
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

        animator.enabled = true;

        // Resets bones to default pose
        animator.Rebind();
        animator.Update(0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}