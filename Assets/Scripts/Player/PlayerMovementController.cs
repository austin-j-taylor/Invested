using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls player Movement, Jumping, Gravity, and Air resistance.
 * 
 */

public class PlayerMovementController : MonoBehaviour {

    private const float rollingAcceleration = 5f;
    private const float maxRollingSpeed = 7.5f;
    private const float maxRollingAngularSpeed = 20;
    private const float maxSprintingAngularVelocity = 30;
    private const float torqueFactor = 8;
    private const float movementFactor = .15f;
    private const float airControlFactor = .06f;
    private const float dotFactor = 10;
    private const float dragAirborne = .2f;
    private const float dragGrounded = 3f;
    private const float dragNoControl = 10f;
    private const float momentOfInertiaMagnitude = 5;

    private Rigidbody rb;
    private PlayerGroundedChecker groundedChecker;
    private PIDController pid;

    public bool IsGrounded {
        get {
            return groundedChecker.IsGrounded;
        }
    }

    private bool jumpQueued;
    private bool lastWasSprintingOnGround;
    
    private void Awake() {
        rb = GetComponent<Rigidbody>();
        groundedChecker = transform.GetComponentInChildren<PlayerGroundedChecker>();
        pid = GetComponent<PIDController>();
        rb.maxAngularVelocity = maxRollingAngularSpeed;
        // Makes the ball "hit the ground running" - if it's spinning and it hits the ground, a high MOI will m
        rb.inertiaTensor = new Vector3(momentOfInertiaMagnitude, momentOfInertiaMagnitude, momentOfInertiaMagnitude);
    }

    private void Update() {
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
            // Convert user input to movement vector
            Vector3 movement = new Vector3(Keybinds.Horizontal(), 0f, Keybinds.Vertical());
            // Rotate movement to be in direction of camera and clamp magnitude
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
                // if a Jump is queued
                if (jumpQueued) {
                    jumpQueued = false;

                    Rigidbody targetRb = groundedChecker.StandingOnCollider.attachedRigidbody;

                    // Apply Pewter Jump, if possible
                    Vector3 force = Player.PlayerPewter.Jump(movement, groundedChecker.Normal);

                    rb.AddForce(force, ForceMode.Impulse);
                    // Apply force and torque to target
                    if (targetRb) {
                        Vector3 radius = groundedChecker.Point - targetRb.worldCenterOfMass;

                        targetRb.AddForce(-force, ForceMode.Impulse);
                        targetRb.AddTorque(Vector3.Cross(radius, -force));
                    }
                }

                // Apply drag
                if (movement.sqrMagnitude > 0 || Player.PlayerIronSteel.IronPulling || Player.PlayerIronSteel.SteelPushing) {
                    // You: "why use ints to represent binary values that should be represented by booleans"
                    // Me, an intellectual:
                    rb.drag = SettingsMenu.settingsData.playerAirResistance * dragAirborne;
                } else {
                    rb.drag = SettingsMenu.settingsData.playerAirResistance * dragGrounded;
                }
            } else { // If airborne, reduce movement magnitude

                rb.drag = SettingsMenu.settingsData.playerAirResistance * dragAirborne;
                //movement *= airControlFactor;
            }

            if (movement.sqrMagnitude > 0) { // If moving at all
                // Apply Pewter Sprint, if possible

                if(Player.PlayerPewter.IsSprinting) {
                    // if sprinting
                    movement *= Player.PlayerPewter.Sprint(movement, !lastWasSprintingOnGround && IsGrounded);
                    rb.maxAngularVelocity = maxSprintingAngularVelocity;
                    lastWasSprintingOnGround = IsGrounded; // only show particles if we've hit the ground
                } else {
                    // not Sprinting, move normally.
                    movement = MovementMagnitude(movement);
                    rb.maxAngularVelocity = maxRollingAngularSpeed;
                    lastWasSprintingOnGround = false;
                }

                // Convert movement to torque
                Vector3 torque = Vector3.Cross(IsGrounded ? groundedChecker.Normal : Vector3.up, movement) * torqueFactor;

                float dot = Vector3.Dot(torque, rb.angularVelocity);
                if(dot < 0) {
                    torque *= 2 - Mathf.Exp(dot / dotFactor);
                }

                // Apply torque to player
                rb.AddTorque(torque, ForceMode.Acceleration);
                // Apply a small amount of the movement force to player for tighter controls & air movement
                rb.AddForce(movement * airControlFactor, ForceMode.Acceleration);

                // Debug
                Debug.DrawRay(transform.position, rb.angularVelocity, Color.red);
                Debug.DrawRay(transform.position, torque, Color.white);
                Debug.DrawRay(transform.position, movement, Color.blue);
            }
        } else {
            rb.drag = SettingsMenu.settingsData.playerAirResistance * dragNoControl;
        }
    }

    // Convert a movement vector into real player movement based on current velocity
    private Vector3 MovementMagnitude(Vector3 movement) {
        return movement * rollingAcceleration * Mathf.Max(maxRollingSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
    }

    public void Clear() {
        jumpQueued = false;
        lastWasSprintingOnGround = false;
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
