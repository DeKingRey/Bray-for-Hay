using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEnemy : EnemyController
{
    [Header("Hearing")]
    [SerializeField] private float maxHearingDistance = 18f;
    [SerializeField] private float minShootDelayDistance;

    private VolumeDetector detector;

    protected override void Start()
    {
        base.Start();

        detector = FindObjectOfType<VolumeDetector>();
    }

    /// Sends out a sphere cast towards the player when a sound is heard
    /// The sphere sizes is dependent on the distance to the player (closer = larger radius)
    /// If the player is beyond the hearing distance, the enemy will just investigate
    public void Investigate()
    {
        if (inProximity) return;
        
        Vector3 playerDir = (player.position - transform.position).normalized;
        float playerDistance = Vector3.Distance(transform.position, player.position);

        // Sphere cast radius (larger if dist smaller)
        float radius = maxHearingDistance / Mathf.Clamp(playerDistance, 0.1f, maxHearingDistance);

        state = EnemyState.Investigating;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, radius, playerDir, out hit, maxHearingDistance, playerLayer))
        {
            // Attacks if player isn't too far
            if (playerDistance < maxHearingDistance) StartCoroutine(ShootDelay(playerDistance));
            else state = EnemyState.Investigating;
        }
    }
}
