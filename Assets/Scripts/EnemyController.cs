using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float waitTime;
    [SerializeField] private float knockbackRecoveryThreshold;

    [SerializeField] protected LayerMask playerLayer;

    private NavMeshAgent agent;
    private EnemyPath path;
    private Rigidbody rb;

    private float elapsedTime = 0f;
    private bool isKnocked = false;

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
            if (rb.velocity.magnitude <= knockbackRecoveryThreshold) RecoverFromKnockback();
            else return;
        }

        if (agent.remainingDistance <= 0.1f)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= waitTime)
            {
                waitTime = 0f;
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
        agent.Warp(transform.position); // Tells agent new position
    }
}
