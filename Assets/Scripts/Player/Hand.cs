using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls how and where coins are thrown from the player.
 */
public class Hand : MonoBehaviour {

    private const float baseSteepAngle = 1f / 2f;
    private const float keyboardSteepAngle = 2f / 3f;

    private Camera firstPersonCamera;
    private Transform centerOfMass;
    private PlayerMovementController movementController;

    // Use this for initialization
    void Start() {
        firstPersonCamera = transform.parent.parent.parent.GetComponentInChildren<Camera>();
        centerOfMass = transform.parent;
        movementController = GetComponentInParent<PlayerMovementController>();
    }

    // If the player is in the air, the hand follows the left joystick.
    // If the player is grounded or is in the air but not touching the joystick, the hand follows the camera.
    void Update() {

        if (movementController.IsGrounded) {
            centerOfMass.localEulerAngles = new Vector3(firstPersonCamera.transform.eulerAngles.x, 0, 0);
        } else {
            float vertical = -Keybinds.Vertical() * baseSteepAngle;
            float horizontal = -Keybinds.Horizontal() * baseSteepAngle;

            if (!GamepadController.UsingGamepad) {
                // If using keyboard, throw coins at a steeper angle
                vertical *= keyboardSteepAngle;
                horizontal *= keyboardSteepAngle;
            }

            Quaternion newRotation = new Quaternion();
            newRotation.SetLookRotation(new Vector3(horizontal, (-1 + Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1).magnitude), vertical), Vector3.up);
            centerOfMass.localRotation = newRotation;
        }
    }
}
