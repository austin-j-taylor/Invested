using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls the first-person camera.
 */

public class FPVCameraLock : MonoBehaviour {
    
    private Transform player;
    private Vector2 cameraPosition;
    private Vector2 cameraVelocity;
    private readonly Vector2 gamepadSpeed = new Vector3(80, 64);
    
    public static bool CameraIsLocked { get; private set; }
    public static float Sensitivity { get; set; }
    public static float Smoothing { get; set; }

    // Use this for initialization
    void Start() {
        GameObject thePlayer = GameObject.FindGameObjectWithTag("Player");
        player = thePlayer.transform;
        cameraPosition.x = player.transform.eulerAngles.y;
        cameraPosition.y = player.transform.eulerAngles.x;
        LockCamera();
        Sensitivity = 2f;
        Smoothing = 1f;
    }

    // Update is called once per frame
    void Update() {
        if (CameraIsLocked) {
            if (GamepadController.UsingGamepad) {
                Vector2 gamepadInput = new Vector2(Input.GetAxis("HorizontalRight"), Input.GetAxis("VerticalRight"));
                gamepadInput.x *= gamepadSpeed.x * Sensitivity;
                gamepadInput.y *= gamepadSpeed.y * Sensitivity;
                gamepadInput *= Time.deltaTime;

                cameraVelocity.x = Mathf.Lerp(cameraVelocity.x, gamepadInput.x, 1f / Smoothing);
                cameraVelocity.y = Mathf.Lerp(cameraVelocity.y, gamepadInput.y, 1f / Smoothing);
            } else {
                Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                mouseInput = Vector2.Scale(mouseInput, new Vector2(Sensitivity, Sensitivity));

                cameraVelocity.x = Mathf.Lerp(cameraVelocity.x, mouseInput.x, 1f / Smoothing);
                cameraVelocity.y = Mathf.Lerp(cameraVelocity.y, mouseInput.y, 1f / Smoothing);
            }
            cameraPosition += cameraVelocity;
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, -90f, 90f);

            transform.localRotation = Quaternion.AngleAxis(-cameraPosition.y, Vector3.right);
            player.transform.localRotation = Quaternion.AngleAxis(cameraPosition.x, player.transform.up);
        }
    }

    public static void LockCamera() {
        Cursor.lockState = CursorLockMode.Locked;
        CameraIsLocked = true;
        Cursor.visible = false;
    }

    public static void UnlockCamera() {
        Cursor.lockState = CursorLockMode.None;
        CameraIsLocked = false;
        Cursor.visible = true;
    }

}
