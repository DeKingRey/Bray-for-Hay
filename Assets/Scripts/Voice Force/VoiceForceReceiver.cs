using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceForceReceiver : MonoBehaviour
{
    private Rigidbody rb;
    private VolumeDetector volumeDetector;
    private EnemyController enemy;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        volumeDetector = FindObjectOfType<VolumeDetector>();

        enemy = GetComponent<EnemyController>();
    }

    void OnTriggerStay(Collider other)
    {
        // Applies force to the object when within the force area
        if (other.gameObject.CompareTag("Force Area"))
        {
            Vector3 forceDirection = transform.position - other.gameObject.transform.position;
            forceDirection.y = 0.3f;
            forceDirection.Normalize();

            float sourceDistance = Vector3.Distance(transform.position, other.gameObject.transform.position);

            float volumeForceMultiplier = volumeDetector.VolumeFromMicrophone() * volumeDetector.micMultiplier;
            if (volumeForceMultiplier < volumeDetector.minVolume) volumeForceMultiplier = 0;

            // Force amount is dependent on distance
            Vector3 forceAmount = forceDirection * volumeForceMultiplier;

            if (forceAmount.magnitude <= 0) return;

            // Disables enemy AI
            if (enemy != null)
            {
                if (!enemy.isKnocked && forceAmount.magnitude >= enemy.forceThreshold)
                {
                    rb.isKinematic = false;
                    enemy.ApplyKnockback();
                    rb.AddForce(forceAmount, ForceMode.Impulse); 
                }
            } else rb.AddForce(forceAmount, ForceMode.Impulse);      
        }
    }
}
