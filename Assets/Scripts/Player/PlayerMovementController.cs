﻿using System.Collections;
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
        rb.maxAngularVelocity = float.PositiveInfinity;
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
                if (IsGrounded) { // and on the ground

                    // If on a wall, rotate movement to be on that wall's plane
                    //if (Vector3.Dot(groundedChecker.Normal, movement) < -0.01f) {
                        float angle = Vector3.Angle(movement, groundedChecker.Normal) - 90;
                        movement = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, groundedChecker.Normal)) * movement;
                        if (movement.y < 0)
                            movement.y = -movement.y;
                    Debug.DrawRay(transform.position, movement, Color.red);
                    //}

                    // Apply Pewter Sprint, if possible
                    float sprintMovement = Player.PlayerPewter.Sprint(movement, lastWasSprinting);
                    if (sprintMovement == 0) {
                        // if airborne or not Sprinting, move normally.
                        lastWasSprinting = false;
                        movement *= MovementMagnitude(movement);
                    } else {
                        // if sprinting
                        lastWasSprinting = true;
                        movement *= sprintMovement;
                    }
                } else {
                    // if airborne or not Sprinting, move normally.
                    movement *= MovementMagnitude(movement);
                }

                // Apply movement to player
                Vector3 torque = Vector3.Cross((IsGrounded) ? groundedChecker.Normal : Vector3.up, movement) * 4;
                //Vector3 torque = Vector3.Cross(groundedChecker.Normal, movement) * 4;
                rb.AddTorque(torque, ForceMode.Acceleration);
                Debug.DrawRay(transform.position, torque, Color.white);
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
