using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public GameObject CameraFollowObj;
    public GameObject PlayerObj;

    public float RotationSmooth = 1f;
    public float MouseSensitivity = 50f;
    public float ControllerSensitivity = 5f;

    float CameraVerticalRotation = 0f;
    float CameraHorizontalRotation = 0f;
    float MouseX;
    float MouseY;

    PlayerMovement MovementScript;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        MovementScript = PlayerObj.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float mx = MouseX * Time.deltaTime * MouseSensitivity;
        float my = MouseY * Time.deltaTime * MouseSensitivity;

        CameraVerticalRotation -= my;
        CameraHorizontalRotation += mx;
        CameraVerticalRotation = Mathf.Clamp(CameraVerticalRotation, -70f, 90f);
        Quaternion rotation = Quaternion.Euler(CameraVerticalRotation, CameraHorizontalRotation, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, RotationSmooth * Time.deltaTime);
        if (MovementScript)
        {
            if (MovementScript.CameraMode == 2)
            {
                PlayerObj.transform.rotation = Quaternion.Lerp(PlayerObj.transform.rotation, Quaternion.Euler(0f, CameraHorizontalRotation, 0f), RotationSmooth);
            }
        }
       
    }

    private void LateUpdate()
    {
        CameraUpdater();
    }

    void CameraUpdater()
    {
        Transform target = CameraFollowObj.transform;
        transform.position = target.position;
    }

    public void UpdateMouseX(InputAction.CallbackContext context)
    {
        MouseX = context.ReadValue<float>();
    }

    public void UpdateMouseY(InputAction.CallbackContext context)
    {
        MouseY = context.ReadValue<float>();
    }

    public void UpdateControllerCamera(InputAction.CallbackContext context)
    {
        Vector2 dir = context.ReadValue<Vector2>();
        MouseX = dir.x * ControllerSensitivity;
        MouseY = dir.y * ControllerSensitivity;
    }
}

