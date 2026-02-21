using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    [SerializeField] private float duration = 5f;

    void Start()
    {
        Destroy(gameObject, duration);
    }
}
