using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEnemy : EnemyController
{
    [SerializeField] private float hearingStrength = 1f;

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

        if (distance <= detector.currentNoiseRadius * hearingStrength)
        {
            canSensePlayer = true;    
        }
    }
}
