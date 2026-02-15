using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPath : MonoBehaviour
{
    public enum PathType
    {
        Loop,
        ReverseWhenComplete
    }

    [Header("Pathfinding")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private PathType pathType;

    private int currentIndex = 0;
    private int direction = 1;

    public Vector3 GetCurrentWaypoint()
    {
        return waypoints[currentIndex].position;
    }

    public Vector3 GetNextWaypoint()
    {
        if (waypoints.Length == 0) return transform.position;

        int index = GetNextWaypointIndex();
        Vector3 nextWaypoint = waypoints[index].position;

        return nextWaypoint;
    }

    private int GetNextWaypointIndex()
    {
        currentIndex += direction;

        // Loops around waypoints
        if (pathType == PathType.Loop) currentIndex %= waypoints.Length;
        else if (pathType == PathType.ReverseWhenComplete)
        {
            // Reverses direction when gone through all waypoints
            if (currentIndex >= waypoints.Length || currentIndex < 0)
            {
                direction *= -1;
                currentIndex += direction * 2; // Reverts previous movement (and moves in new dir)
            }
        }

        return currentIndex;
    }
}
