using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls player Movement, Jumping, Gravity, and Air resistance.
 * 
 */

public class PlayerMovementController : MonoBehaviour {

    private const float defaultAcceleration = 5f;
    private const float defaultRunningSpeed = 7.5f;
    private const float movementFactor = .15f;
    private const float airControlFactor = .06f;
    private const float dragAirborne = .2f;
    private const float dragGrounded = 3f;
    private const float dragNoControl = 10f;

    private Rigidbody rb;
    private PlayerGroundedChecker groundedChecker;

    public bool IsGrounded {
        get {
            return groundedChecker.IsGrounded;
        }
    }

    private bool jumpQueued;
    private bool lastWasSprinting;
    
    private void Awake() {
        rb = GetComponent<Rigidbody>();
        groundedChecker = transform.GetComponentInChildren<PlayerGroundedChecker>();
    }

    private void Update() {
        if(!Keybinds.Sprint()) {
            lastWasSprinting = false;
        }
        if (IsGrounded && Keybinds.JumpDown()) {
            // Queue a jump for the next FixedUpdate
            // Actual jumping done in FixedUpdate to stay in sync with PlayerGroundedChecker
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

            // If Walking, reduce movment
            if(Keybinds.Walk()) {
                movement *= movementFactor;
            }

            if (IsGrounded) {
                // Jump
                if (jumpQueued) {
                    jumpQueued = false;
                    groundedChecker.Jump(movement);
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
            
            if (movement.sqrMagnitude > 0) {
                if(IsGrounded) {
                    float sprintMovement = Player.PlayerPewter.Sprint(movement, lastWasSprinting);
                    lastWasSprinting = true;
                    if (sprintMovement == 0) {
                        // if airborne or not Sprinting, move normally.
                        movement *= MovementMagnitude(movement);
                    } else {
                        // if sprinting
                        movement *= sprintMovement;
                    }
                } else {
                    // if airborne or not Sprinting, move normally.
                    movement *= MovementMagnitude(movement);
                }
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

    // Convert a movement vector into real player movement based on current velocity
    private float MovementMagnitude(Vector3 movement) {
        return defaultAcceleration * Mathf.Max(defaultRunningSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
    }

    public void Clear() {
        jumpQueued = false;
        lastWasSprinting = false;
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
