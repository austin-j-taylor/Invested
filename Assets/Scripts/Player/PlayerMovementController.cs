using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls player Movement, Jumping, Gravity, and Air resistance.
 * 
 */

public class PlayerMovementController : MonoBehaviour {

    private const float acceleration = 5f;
    private const float maxRunningSpeed = 5f;
    private const float airControlFactor = .05f;
    private const float airDrag = .2f;
    private const float groundedDrag = 3f;
    private readonly Vector3 jumpHeight = new Vector3(0, 420f, 0);
    
    private Rigidbody rb;
    private PlayerGroundedChecker groundedChecker;

    public bool IsGrounded {
        get {
            return groundedChecker.IsGrounded;
        }
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        groundedChecker = GetComponentInChildren<PlayerGroundedChecker>();
    }
    void FixedUpdate() {
        Vector3 movement = new Vector3(Keybinds.Horizontal(), 0f, Keybinds.Vertical());
        movement = transform.TransformDirection(Vector3.ClampMagnitude(movement, 1));
        if (IsGrounded) {
            if (movement.magnitude > 0) {
                // You: "why use ints to represent binary values that should be represented by booleans"
                // Me, an intellectual:
                rb.drag = SettingsMenu.settingsData.playerAirResistance * airDrag;
            } else {
                rb.drag = SettingsMenu.settingsData.playerAirResistance * groundedDrag;
            }
        } else { // is airborne
            rb.drag = SettingsMenu.settingsData.playerAirResistance * airDrag;
            movement *= airControlFactor;
        }
        if (movement.magnitude > 0) {
            movement *= acceleration * Mathf.Max(maxRunningSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
            rb.AddForce(movement, ForceMode.Acceleration);
        }
    }

    private void Update() {
        if(IsGrounded) {
            // Jump
            if (Keybinds.JumpDown()) {
                rb.AddForce(jumpHeight, ForceMode.Impulse);
                groundedChecker.AddForceToTouchingCollider(-jumpHeight);
            }
        }
    }

    public void Clear() {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void EnableGravity() {
        rb.useGravity = true;
    }
    public void DisableGravity() {
        rb.useGravity = false;
    }

}
