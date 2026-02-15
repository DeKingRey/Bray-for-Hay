using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceForceReceiver : MonoBehaviour
{
    private Rigidbody rb;
    private VolumeDetector volumeDetector;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        volumeDetector = FindObjectOfType<VolumeDetector>();
    }

    void OnTriggerStay(Collider other)
    {
        // Applies force to the object when within the force area
        if (other.gameObject.CompareTag("Force Area"))
        {
            Vector3 forceDirection = transform.position - other.gameObject.transform.position;
            float sourceDistance = Vector3.Distance(transform.position, other.gameObject.transform.position);

            float volumeForceMultiplier = volumeDetector.VolumeFromMicrophone() * volumeDetector.micMultiplier;
            if (volumeForceMultiplier < volumeDetector.minVolume) volumeForceMultiplier = 0;

            if (gameObject.CompareTag("Enemy"))
            {
                EnemyController enemy = GetComponent<EnemyController>();
                enemy.ApplyKnockback();
            }

            rb.AddForce(forceDirection * volumeForceMultiplier / sourceDistance, ForceMode.Impulse);
        }
    }
}
