using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horseshoe : MonoBehaviour
{
    void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Force Box") || obj.CompareTag("Door")) Destroy(gameObject);
    }
}
