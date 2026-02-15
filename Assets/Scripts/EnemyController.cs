using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour
{
    private enum PathType
    {
        Loop,
        ReverseWhenComplete
    }

    [Header("Pathfinding")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private PathType pathType;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private int direction = 1;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        
    }

    private int GetNextWaypointIndex()
    {
        currentWaypointIndex += direction;

        if (pathType == PathType.Loop) index %= waypoints.Length;
    }
}
