using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightEnemy : EnemyController
{
    [Header("Sight")]
    [SerializeField] private float visionAngle;

    protected override void Update()
    {
        base.Update();

        // State manager for enemies - only applies to non-blind enemies
        bool inChaseRange = Physics.CheckSphere(transform.position, detectionRadius, playerLayer);
        bool inAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (inAttackRange) state = EnemyState.Attacking;
        else if (inChaseRange) state = EnemyState.Chasing;
        else state = EnemyState.Patrolling;

        if (!playerInRange) return;

        // Gets angle of direction to player to forward position
        Vector3 playerDir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, playerDir);

        // Checks if player is in vision cone
        if (angle > visionAngle * 0.5f) return;

        canSensePlayer = true;
    }
}
