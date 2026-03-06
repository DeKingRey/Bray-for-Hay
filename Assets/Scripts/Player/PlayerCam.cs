using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private Transform player;

    [SerializeField] private float sensX;
    [SerializeField] private float sensY;

    [SerializeField] private float minRotationX = -90f;
    [SerializeField] private float maxRotationX = 90f;

    private float xRotation;
    private float yRotation;

    private bool camStart = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get rotations based on mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minRotationX, maxRotationX);

        // Sets x rotation to 0 to begin with (to avoid errors)
        if (camStart)
        {
            xRotation = 0f;
            camStart = false;
        }

        // Rotates camera/player
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.Rotate(Vector3.up * mouseX);

    }
}
