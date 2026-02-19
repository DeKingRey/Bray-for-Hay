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

        if (!playerInRange) return;

        // Gets angle of direction to player to forward position
        Vector3 playerDir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, playerDir);

        // Checks if player is in vision cone
        if (angle > visionAngle * 0.5f) return;

        if (Physics.Raycast(transform.position, playerDir, out RaycastHit hit, detectionRadius))
        {
            if (hit.collider.CompareTag("Player"))
            {
                canSensePlayer = true;
            }
        }
    }
}
