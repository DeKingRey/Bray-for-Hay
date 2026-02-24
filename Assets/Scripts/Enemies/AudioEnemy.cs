using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEnemy : EnemyController
{
    [Header("Hearing")]
    [SerializeField] private float maxHearingDistance = 18f;
    [SerializeField] private float minShootDelayDistance;
    [SerializeField] private float minDelayDuration;
    [SerializeField] private float maxDelayDuration;

    private VolumeDetector detector;

    protected override void Start()
    {
        base.Start();

        detector = FindObjectOfType<VolumeDetector>();
    }

    protected override void Update()
    {
        base.Update();

        // Make it so it attacks even if it cant hear the player (create a suspicious function or smth)

        if (!playerInRange) return;

        float distance = Vector3.Distance(transform.position, player.position);
    }

    /// Sends out a sphere cast towards the player when a sound is heard
    /// The sphere sizes is dependent on the distance to the player (closer = larger radius)
    /// If the player is beyond the hearing distance, the enemy will just investigate
    public void Investigate()
    {
        Vector3 playerDir = (player.position - transform.position).normalized;
        float playerDistance = Vector3.Distance(transform.position, player.position);
        float radius = maxHearingDistance / playerDistance;

        canHearPlayer = 1;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, radius, playerDir, out hit, maxHearingDistance, playerLayer))
        {
            if (playerDistance < maxHearingDistance) StartCoroutine(ShootDelay(playerDistance));
            else canHearPlayer = 1;
        }
    }

    /// Starts a delay for shooting
    /// The closer the player is the sooner the enemy will shoot
    IEnumerator ShootDelay(float distance)
    {
        float duration = maxDelayDuration;
        if (distance <= minShootDelayDistance) duration = minDelayDuration;

        yield return new WaitForSeconds(duration);
        canHearPlayer = 2;
    }
}
