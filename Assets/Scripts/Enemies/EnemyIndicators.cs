using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIndicators : MonoBehaviour
{
    private Animator anim;
    private EnemyController enemy;

    void Start()
    {
        anim = GetComponent<Animator>();
        enemy = GetComponentInParent<EnemyController>();
    }

    void Update()
    {
        if (enemy.state == EnemyController.EnemyState.Patrolling) anim.SetTrigger("Idle");
        else if (enemy.state == EnemyController.EnemyState.Investigating) anim.SetTrigger("Suspicious");
        else anim.SetTrigger("Alert");
    }
}
