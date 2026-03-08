using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEnemy : EnemyController
{
    [Header("Hearing")]
    [SerializeField] private float maxHearingDistance = 18f;
    [SerializeField] private float minShootDelayDistance;
    [SerializeField] private Transform hearingRadius;

    private VolumeDetector detector;

    protected override void Start()
    {
        base.Start();

        detector = FindObjectOfType<VolumeDetector>();
    }

    protected override void Update()
    {
        base.Update();
        if (isKnocked || inProximity) return;

        if (isAlert)
        {
            isAlert = false;
            StartCoroutine(OnAlert());
        }
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
            if (playerDistance < maxHearingDistance) state = EnemyState.Attacking;
            else state = EnemyState.Investigating;
        }
    }

    /// Temporarily increases hearing radius after investigating player
    private IEnumerator OnAlert()
    {
        Transform defaultHearingRadius = hearingRadius;
        hearingRadius.localScale *= alertMultiplier;

        yield return new WaitForSeconds(alertDuration);

        hearingRadius = defaultHearingRadius;
    }
}
