﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KogMovementController : MonoBehaviour {

    private enum WalkingState { Idle, Walking, Sprinting, Anchored };

    #region constants
    [SerializeField]
    private float topSpeed = 5;
    [SerializeField]
    private float topSpeedSprinting = 12.5f;
    [SerializeField]
    private float topSpeedAnchored = 1.5f;
    [SerializeField]
    private float acceleration = 2.5f;
    [SerializeField]
    private float accelerationSprinting = 5f;

    [SerializeField]
    private float rotationalTopSpeed = 5;
    [SerializeField]
    private float rotationalAcceleration = 2.5f;

    [SerializeField]
    private float jumpHeight = 900;

    [SerializeField]
    private float speed_P = 10, speed_mD = 50;
    #endregion

    #region properties
    private WalkingState state;
    public bool IsGrounded {
        get => groundedChecker.IsGrounded;
    }
    #endregion

    private PrimaGroundedChecker groundedChecker;
    private Rigidbody rb;
    private Vector3 lastInput; // The last horizontal/vertical movement command sent to the player
    private PIDController_Vector3 pidSpeed;
    private bool jumpQueued;
    private float lastJumpTime;

    #region clearing
    protected void Awake() {
        rb = GetComponent<Rigidbody>();
        pidSpeed = gameObject.AddComponent<PIDController_Vector3>();
        pidSpeed.SetParams(speed_P, 0, 0, speed_mD);
        groundedChecker = transform.GetComponentInChildren<PrimaGroundedChecker>();
    }

    private void Start() {
        Clear();
    }

    public void Clear() {
        state = WalkingState.Idle;
        lastInput = Vector3.zero;
        jumpQueued = false;
        lastJumpTime = -1;
    }

    #endregion

    #region updates
    private void Update() {
        if (!GameManager.MenusController.pauseMenu.IsOpen) {
            if (Player.CanControl && Player.CanControlMovement) {
                if (IsGrounded && Keybinds.JumpDown()) {
                    // Queue a jump for the next FixedUpdate
                    // Actual jumping done in FixedUpdate
                    jumpQueued = true;
                }
            }
        }
    }
    private void FixedUpdate() {

        if (Player.CanControl && Player.CanControlMovement) {

            if (IsGrounded) {
                // if a Jump is queued
                if (jumpQueued) {
                    jumpQueued = false;
                    lastJumpTime = Time.unscaledTime;

                    groundedChecker.UpdateStanding();
                    Rigidbody targetRb = groundedChecker.StandingOnCollider.attachedRigidbody;

                    Vector3 jumpForce = groundedChecker.Normal * jumpHeight;

                    rb.AddForce(jumpForce, ForceMode.Impulse);
                    Debug.Log(jumpForce);
                    // Apply force and torque to target
                    if (targetRb) {
                        Vector3 radius = groundedChecker.Point - targetRb.worldCenterOfMass;

                        targetRb.AddForce(-jumpForce, ForceMode.Impulse);
                        targetRb.AddTorque(Vector3.Cross(radius, -jumpForce));
                    }
                }

            } // Not grounded
              // "Short hopping": if you tap the jump button instead of holding it, do a short hop.
              // Effectively, if you release the jump button within a short window of jumping,
              //  apply a small negative impulse to reduce the jump height.
            if (!Keybinds.Jump() && Time.unscaledTime - lastJumpTime < PrimaMovementController.shortHopThreshold) {
                rb.AddForce(-groundedChecker.Normal * jumpHeight / 2, ForceMode.Impulse);
                lastJumpTime = -1;
            }


            // Convert user input to movement vector
            // If the control wheel is open, use the last input for movement

            Vector3 movement;
            if (HUD.ControlWheelController.IsOpen) {
                movement = lastInput;
            } else {
                movement = new Vector3(Keybinds.Horizontal(), 0f, Keybinds.Vertical());
                lastInput = movement;
            }

            // If is unclamped and upside-down, keep movement in an intuitive direction for the player
            // Rotate movement to be in direction of camera and clamp magnitude
            if (SettingsMenu.settingsGameplay.cameraClamping == 0 && CameraController.UpsideDown) {
                movement.x = -movement.x;
                movement = CameraController.CameraDirection * Vector3.ClampMagnitude(movement, 1);
                movement = -movement;
            } else {
                movement = CameraController.CameraDirection * Vector3.ClampMagnitude(movement, 1);
            }
            bool wantToMove = movement.sqrMagnitude > 0;

            // Transitions
            switch (state) {
                case WalkingState.Idle:
                    if (wantToMove) {
                        if (Keybinds.Sprint()) {
                            state = WalkingState.Sprinting;
                        } else if (Keybinds.Walk()) {
                            state = WalkingState.Anchored;
                        } else {
                            state = WalkingState.Walking;
                        }
                    }
                    break;
                case WalkingState.Walking:
                    if (wantToMove) {
                        if (Keybinds.Sprint()) {
                            state = WalkingState.Sprinting;
                        } else if (Keybinds.Walk()) {
                            state = WalkingState.Anchored;
                        }
                    } else {
                        state = WalkingState.Idle;
                    }
                    break;
                case WalkingState.Sprinting:
                    if (wantToMove) {
                        if (Keybinds.Walk()) {
                            state = WalkingState.Anchored;
                        } else if (!Keybinds.Sprint()) {
                            state = WalkingState.Walking;
                        }
                    } else {
                        state = WalkingState.Idle;
                    }
                    break;
                case WalkingState.Anchored:
                    if (wantToMove) {
                        if (!Keybinds.Walk()) {
                            if (Keybinds.Sprint()) {
                                state = WalkingState.Sprinting;
                            } else {
                                state = WalkingState.Walking;
                            }
                        }
                    } else {
                        state = WalkingState.Idle;
                    }
                    break;
            }

            pidSpeed.SetParams(speed_P, 0, 0, speed_mD);

            Vector3 target = Vector3.zero;
            // Actions
            switch (state) {
                case WalkingState.Idle:
                    //target = Vector3.zero;
                    break;
                case WalkingState.Walking:
                    target = movement * topSpeed;
                    break;
                case WalkingState.Sprinting:
                    target = movement * topSpeedSprinting;
                    break;
                case WalkingState.Anchored:
                    target = movement * topSpeedAnchored;
                    break;
            }

            Vector3 feedback = rb.velocity;
            feedback.y = 0;
            Vector3 output = pidSpeed.Step(feedback, target);
            rb.AddForce(output, ForceMode.Acceleration);
        }
    }
    #endregion

}