using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls player Movement, Jumping, Gravity, and Air resistance.
 * 
 */

public class PlayerMovementController : MonoBehaviour {

    private const float acceleration = 5f;
    private const float maxRunningSpeed = 7.5f;
    private const float airControlFactor = .06f;
    private const float dragAirborne = .2f;
    private const float dragGrounded = 3f;
    private const float dragNoControl = 10f;
    private readonly Vector3 jumpHeight = new Vector3(0, 400f, 0);

    private Rigidbody rb;
    private PlayerGroundedChecker groundedChecker;

    public bool IsGrounded {
        get {
            return groundedChecker.IsGrounded;
        }
    }

    private bool jumpQueued = false;

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        groundedChecker = transform.GetComponentInChildren<PlayerGroundedChecker>();
    }

    private void Update() {
        if (IsGrounded && Keybinds.JumpDown()) {
            // Queue a jump for the next FixedUpdate
            // Actual jumping done in FixedUpdate to stay in synx with PlayerGroundedChecker
            jumpQueued = true;
        } else {
            jumpQueued = false;
        }
    }

    void FixedUpdate() {
        if (Player.CanControlPlayer) {
            Vector3 movement = new Vector3(Keybinds.Horizontal(), 0f, Keybinds.Vertical());
            movement = CameraController.CameraDirection * Vector3.ClampMagnitude(movement, 1);

            // If is unclamped and upside-down, keep movement in an intuitive direction for the player
            if (SettingsMenu.settingsData.cameraClamping == 0) {
                float angle = CameraController.ActiveCamera.transform.localEulerAngles.y;
                if (angle > 1) { // flips to 180 when camera is upside-down
                    movement = -movement;
                }
            }
            
            if (IsGrounded) {
                // Jump
                if (jumpQueued) {
                    jumpQueued = false;
                    rb.AddForce(jumpHeight, ForceMode.Impulse);
                    groundedChecker.AddForceToTouchingCollider(-jumpHeight);
                }
                // Apply drag
                if (movement.sqrMagnitude > 0 || Player.PlayerIronSteel.IronPulling || Player.PlayerIronSteel.SteelPushing) {
                    // You: "why use ints to represent binary values that should be represented by booleans"
                    // Me, an intellectual:
                    rb.drag = SettingsMenu.settingsData.playerAirResistance * dragAirborne;
                } else {
                    rb.drag = SettingsMenu.settingsData.playerAirResistance * dragGrounded;
                }
            } else { // is airborne
                rb.drag = SettingsMenu.settingsData.playerAirResistance * dragAirborne;
                movement *= airControlFactor;
            }
            if (movement.magnitude > 0) {
                movement *= acceleration * Mathf.Max(maxRunningSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
                rb.AddForce(movement, ForceMode.Acceleration);
            }
        } else {
            //if (IsGrounded) {
                rb.drag = SettingsMenu.settingsData.playerAirResistance * dragNoControl;
            //} else {
            //    rb.drag = SettingsMenu.settingsData.playerAirResistance * airDrag;
            //}
        }
    }

    public void Clear() {
        jumpQueued = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = SettingsMenu.settingsData.playerGravity == 1;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
    }

    public void EnableGravity() {
        rb.useGravity = true;
    }
    public void DisableGravity() {
        rb.useGravity = false;
    }
}
