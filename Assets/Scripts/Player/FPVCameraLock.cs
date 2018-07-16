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
    private bool locked;
    private const float mouseSens = 2f;
    private const float mouseSmooth = 1f;
    private readonly Vector2 gamepadSpeed = new Vector3(100, 80);
    private const float gamepadSens = 1.6f;
    private const float gamepadSmooth = 4f;

    // Use this for initialization
    void Start() {
        player = transform.parent;
        cameraPosition.x = player.eulerAngles.y;
        cameraPosition.y = player.eulerAngles.x;
        Cursor.lockState = CursorLockMode.Locked;
        locked = true;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            locked = !locked;
            if (locked) {
                Cursor.lockState = CursorLockMode.Locked;
                Time.timeScale = 1f;
            } else {
                Cursor.lockState = CursorLockMode.None;
                Time.timeScale = 0f;
            }
        }
        if (locked) {
            if (GamepadController.UsingGamepad) {
                Vector2 gamepadInput = new Vector2(Input.GetAxis("HorizontalRight"), Input.GetAxis("VerticalRight"));
                gamepadInput.x *= gamepadSpeed.x * gamepadSens;
                gamepadInput.y *= gamepadSpeed.y * gamepadSens;
                gamepadInput *= Time.deltaTime;

                cameraVelocity.x = Mathf.Lerp(cameraVelocity.x, gamepadInput.x, 1f / gamepadSmooth);
                cameraVelocity.y = Mathf.Lerp(cameraVelocity.y, gamepadInput.y, 1f / gamepadSmooth);
            } else {
                Vector2 mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                mouseInput = Vector2.Scale(mouseInput, new Vector2(mouseSens, mouseSens));

                cameraVelocity.x = Mathf.Lerp(cameraVelocity.x, mouseInput.x, 1f / mouseSmooth);
                cameraVelocity.y = Mathf.Lerp(cameraVelocity.y, mouseInput.y, 1f / mouseSmooth);
            }
            cameraPosition += cameraVelocity;
            cameraPosition.y = Mathf.Clamp(cameraPosition.y, -90f, 90f);

            transform.localRotation = Quaternion.AngleAxis(-cameraPosition.y, Vector3.right);
            player.localRotation = Quaternion.AngleAxis(cameraPosition.x, player.up);
        }
    }
}
