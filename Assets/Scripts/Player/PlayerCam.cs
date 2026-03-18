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

    public float sensitivity = 0.5f;

    private float xRotation;
    private float yRotation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        xRotation = 0f;
    }

    void Update()
    {
        if (Time.frameCount == 1) return;
        if (GameManager.Instance.State != GameManager.GameState.Playing) return;

        // Get rotations based on mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * sensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minRotationX, maxRotationX);

        // Rotates camera/player
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        player.Rotate(Vector3.up * mouseX);

    }
}
