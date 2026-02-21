using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private GameManager.GameState playState;

    [Header("Pathfinding")]
    [SerializeField] private float waitTime;
    [SerializeField] private float knockbackRecoveryThreshold;
    [SerializeField] private float knockbackRecoveryTime = 3f;

    [Header("Detection")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float detectionRadius;

    [Header("Combat")]
    [SerializeField] private float attackRange;
    [SerializeField] private float attackCooldown;
    [SerializeField] private GameObject projectile;

    private NavMeshAgent agent;
    private EnemyPath path;
    private Rigidbody rb;

    private float elapsedTime = 0f;
    [HideInInspector] public bool isKnocked = false;
    protected bool playerInRange = false;
    protected bool canSensePlayer = false;
    private bool hasAttacked = false;

    protected Transform player;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = GetComponent<EnemyPath>();
        rb = GetComponent<Rigidbody>();

        agent.destination = path.GetCurrentWaypoint();
        player = GameObject.FindWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.State != playState) return;

        if (isKnocked)
        {
            if (rb.velocity.magnitude <= knockbackRecoveryThreshold) RecoverFromKnockback();
            else return;
        }

        bool inChaseRange = Physics.CheckSphere(transform.position, detectionRadius, playerLayer);
        bool inAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (!inChaseRange && !inAttackRange) Patrolling();
        else if (inChaseRange)
        {
            playerInRange = true;
            if (!inAttackRange && canSensePlayer) Chasing();
            else if (inAttackRange && canSensePlayer) Attacking();
            else Patrolling();
        } else playerInRange = false;
    }

    public void ApplyKnockback()
    {
        isKnocked = true;
        agent.enabled = false;

        StartCoroutine(KnockedOutTimer());
    }

    private void RecoverFromKnockback()
    {
        isKnocked = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        agent.enabled = true;

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
        agent.destination = player.position;
    }

    void Attacking()
    {
        agent.destination = transform.position;

        transform.LookAt(player);

        if (!hasAttacked)
        {
            Rigidbody rb = Instantiate(projectile, transform.position, Quaternion.identity).GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * 32f, ForceMode.Impulse);
            hasAttacked = true;
            Invoke(nameof(ResetAttack), attackCooldown);
        }
    }

    void ResetAttack()
    {
        hasAttacked = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}