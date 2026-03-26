using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    [SerializeField] private GameManager.LevelType levelType;

    void Start()
    {
        GameManager.Instance.Level = levelType;
    }
}
