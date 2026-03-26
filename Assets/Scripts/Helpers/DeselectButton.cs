using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeselectButton : MonoBehaviour
{
    void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Deselect);
    }

    void Deselect()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
}
