using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEnemy : EnemyController
{
    [SerializeField] private float maxHearingDistance = 18f;

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

    public void Investigate()
    {
        Vector3 playerDir = (player.position - transform.position).normalized;
        float playerDistance = Vector3.Distance(transform.position, player.position);
        float radius = maxHearingDistance / playerDistance;

        RaycastHit hit;
        if (Physics.SphereCast(transform.position, radius, playerDir, out hit, maxHearingDistance, playerLayer))
        {
            
        }
    }
}
