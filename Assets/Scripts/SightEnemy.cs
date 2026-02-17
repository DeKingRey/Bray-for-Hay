using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightEnemy : EnemyController
{
    [SerializeField] private float visionAngle;
    [SerializeField] private float visionDistance;

    void Update()
    {
        // Checks if player is within detection radius of enemy
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length == 0) return;

        player = hits[0].transform;

        // Gets angle of direction to player to forward position
        Vector3 playerDir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, playerDir);

        // Checks if player is in vision cone
        if (angle > visionAngle * 0.5f) return;

        if (Physics.Raycast(transform.position, playerDir, out RaycastHit hit, visionDistance))
        {
            if (hit.collider.CompareTag("Player"))
            {
                isChasing = true;
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (player != null)
            Gizmos.DrawRay(transform.position, (player.position - transform.position).normalized);
    }
}
