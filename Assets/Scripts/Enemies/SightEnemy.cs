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

        if (isKnocked) return;

        // Checks range from player
        bool inPlayerRange = Physics.CheckSphere(transform.position, detectionRadius, playerLayer);
        bool inAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        if (!inPlayerRange) state = EnemyState.Patrolling;

        // Gets angle of direction to player to forward position
        Vector3 playerDir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, playerDir);

        // Checks if player is in vision cone
        if (angle > visionAngle * 0.5f)
        {
            state = EnemyState.Patrolling;
            return;
        }

        // Changes enemy state
        if (inAttackRange) state = EnemyState.Attacking;
        else state = EnemyState.Investigating;
    }
}
