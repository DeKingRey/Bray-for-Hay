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

        if (isKnocked || inProximity) return;
        if (isAlert)
        {
            isAlert = false;
            StartCoroutine(OnAlert());
        }

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
    
        RaycastHit hit;
        // Returns if player is not in line of sight/an obstacle is in the way
        if (!Physics.Raycast(transform.position, playerDir, out hit, detectionRadius)) return;
        if (!hit.transform.CompareTag("Player")) return;

        float playerDistance = Vector3.Distance(transform.position, player.position);

        // Changes enemy state
        if (inAttackRange) state = EnemyState.Attacking;
        else state = EnemyState.Investigating;
    }

    private IEnumerator OnAlert()
    {
        float defaultVisionAngle = visionAngle;

        visionAngle *= alertMultiplier;

        yield return new WaitForSeconds(alertDuration);
    }
}
