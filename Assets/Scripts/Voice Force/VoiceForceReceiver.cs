using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceForceReceiver : MonoBehaviour
{
    private Rigidbody rb;
    private Rigidbody hipsRb;
    private VolumeDetector detector;
    private EnemyController enemy;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        detector = FindObjectOfType<VolumeDetector>();

        enemy = GetComponent<EnemyController>();
        if (enemy) hipsRb = enemy.hips;
    }

    void OnTriggerStay(Collider other)
    {
        // Applies force to the object when within the force area
        if (!other.gameObject.CompareTag("Force Area")) return;

        // Gets the basic force direction - adding upwards direction
        Vector3 forceDirection = transform.position - other.gameObject.transform.position;
        forceDirection.y = 0.3f;
        forceDirection.Normalize();

        float sourceDistance = Vector3.Distance(transform.position, other.gameObject.transform.position);
        Mathf.Clamp(sourceDistance, 1f, Mathf.Infinity);

        // Only proceeds if not too quiet
        float volumeForceMultiplier = detector.VolumeFromMicrophone() * detector.micMultiplier;
        if (volumeForceMultiplier < detector.minVolume) return;

        // Calculates base force and ensures not too small
        Vector3 baseForce = (forceDirection * volumeForceMultiplier) / sourceDistance;
        if (baseForce.magnitude <= 0) return;

        // Choose correct rb    
        Rigidbody targetRb = hipsRb != null ? hipsRb : rb;

        // Calculates final force with added upwards force
        Vector3 upwardForce = Vector3.up * baseForce.magnitude * detector.upwardMultiplier;
        Vector3 finalForce = baseForce + upwardForce;

        // Gets axis to rotate around using torque (use RHG to understand)
        // Fingers are force - they curl towards upwards direction. Thumb is the pivot point/axis
        Vector3 torqueAxis = Vector3.Cross(forceDirection, Vector3.up).normalized;

        // Handles enemy knockback
        if (enemy != null)
        {
            if (!enemy.isKnocked && baseForce.magnitude >= enemy.forceThreshold)
            {
                enemy.ApplyKnockback(finalForce.magnitude);
                Debug.Log(baseForce.magnitude);
            }
            else if (enemy.isKnocked) return;
            else return;
        }

        // Resets velocity
        //targetRb.velocity = Vector3.zero;
        //targetRb.angularVelocity = Vector3.zero;
        
        // Velocity change ignores mass
        targetRb.AddForce(finalForce, ForceMode.VelocityChange);

        // Added torque adds rotational movement - so if the enemy is pushed from behind, it falls forward
        targetRb.AddTorque(torqueAxis * baseForce.magnitude * detector.torqueMultiplier, ForceMode.VelocityChange);
    }
}
