using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEnemy : EnemyController
{
    [SerializeField] private float volumeThreshold;

    private Transform player;

    void Update()
    {
        // Checks if player is within detection radius of enemy
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
        if (hits.Length == 0) return;

        player = hits[0].transform;

        // Check if audio is detected within radius
        // Check that audio is above threshold
        // Chase
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (player != null)
            Gizmos.DrawRay(transform.position, (player.position - transform.position).normalized);
    }
}
