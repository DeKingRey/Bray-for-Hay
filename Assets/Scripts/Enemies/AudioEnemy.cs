using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEnemy : EnemyController
{
    [SerializeField] private float hearingStrength = 1f;
    private bool isSuspicious = false;

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

        if (!playerInRange || isSuspicious) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= detector.currentNoiseRadius * hearingStrength)
        {
            canSensePlayer = true;
            StartCoroutine(SuspicionDuration(detector.currentNoiseRadius * hearingStrength / 4));
        }
    }

    IEnumerator SuspicionDuration(float duration)
    {
        isSuspicious = true;
        yield return new WaitForSeconds(duration);
        isSuspicious = false;
        canSensePlayer = false;
    }
}
