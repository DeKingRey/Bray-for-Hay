using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [Header("Pathfinding")]
    [SerializeField] private float waitTime;
    [SerializeField] private float knockbackRecoveryThreshold;

    [Header("Detection")]
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected float detectionRadius;

    [Header("Combat")]
    [SerializeField] private float attackDistance;
    [SerializeField] private float attackCooldown;
    [SerializeField] private GameObject bulletObject;

    private NavMeshAgent agent;
    private EnemyPath path;
    private Rigidbody rb;
    private float groundY = -3.437806f;

    private float elapsedTime = 0f;
    [HideInInspector] public bool isKnocked = false;
    protected bool isChasing = false;

    protected Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        path = GetComponent<EnemyPath>();
        rb = GetComponent<Rigidbody>();

        agent.destination = path.GetCurrentWaypoint();
    }

    void Update()
    {
        if (isKnocked)
        {
            Debug.Log($"{gameObject.name}: {rb.velocity.magnitude}");
            if (rb.velocity.magnitude <= knockbackRecoveryThreshold) RecoverFromKnockback();
            else return;
        }

        if (isChasing)
        {
            ChasePlayer();
            return;
        }

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

    public void ApplyKnockback()
    {
        isKnocked = true;
        agent.enabled = false;
    }

    private void RecoverFromKnockback()
    {
        isKnocked = false;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        agent.enabled = true;

        Vector3 agentPosition = new Vector3(transform.position.x, groundY, transform.position.z);
        agent.Warp(agentPosition); // Tells agent new position
    }

    void ChasePlayer()
    {
        isChasing = true;
        agent.destination = player.position;

        if (agent.remainingDistance >= detectionRadius) isChasing = false;

        if (agent.remainingDistance <= attackDistance)
        {
            StartCoroutine(Attack());
        }
    }

    private IEnumerator Attack()
    {
        Instantiate(bulletObject);

        yield return new WaitForSeconds(attackCooldown);
    }
}
