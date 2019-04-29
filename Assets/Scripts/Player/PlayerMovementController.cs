using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls player Movement, Jumping, Gravity, and Air resistance.
 * 
 * Inherits AllomanticPewter properties.
 * 
 */

public class PlayerMovementController : AllomanticPewter {

    private const float radius = .26f;
    // Rolling
    public const float rollingAcceleration = 5f;
    public const float maxRollingSpeed = 7.5f / radius;
    private const float maxRollingAngularSpeed = Mathf.Infinity; // maxRollingSpeed;
    // Pewter
    private const float pewterAcceleration = 5.5f;
    private const float maxSprintingSpeed = 12.5f / radius;
    private const float pewterSpeedFactor = maxSprintingSpeed / maxRollingSpeed;
    private const float maxSprintingAngularVelocity = Mathf.Infinity;
    // Pewter burning
    protected const float gramsPewterPerJump = 1f;
    protected const float timePewterPerJump = 1.5f;
    // Jumping
    private const float jumpHeight = 300;
    private const float jumpDirectionModifier = 400;
    private const float jumpPewterMagnitude = 500;

    // Factors
    private const float movementFactor = .15f;
    private const float airControlFactor = .3f;
    private const float dotFactor = 10;
    // Air resistance
    private const float dragAirborneLinear = .2f;
    private const float dragGroundedLinear = 3f;
    [SerializeField]
    private  float dragAirborneAngular = 1.5f;
    [SerializeField]
    private  float dragGroundedAngular = 5f;
    private const float dragNoControl = 10f;
    // Misc
    private const float momentOfInertiaMagnitude = 5;
    private const float momentOfInertiaMagnitudeWalking = 50;
    private readonly Vector3 particleSystemPosition = new Vector3(0, -.2f, 0);
    private readonly Vector3 inertiaTensorRunning = new Vector3(momentOfInertiaMagnitude, momentOfInertiaMagnitude, momentOfInertiaMagnitude);
    private readonly Vector3 inertiaTensorWalking = new Vector3(momentOfInertiaMagnitudeWalking, momentOfInertiaMagnitudeWalking, momentOfInertiaMagnitudeWalking);

    private PlayerGroundedChecker groundedChecker;
    private PIDController_Vector3 pidSpeed;

    public bool IsGrounded {
        get {
            return groundedChecker.IsGrounded;
        }
    }

    private bool jumpQueued;
    private bool lastWasSprintingOnGround;

    protected override void Awake() {
        base.Awake();
        groundedChecker = transform.GetComponentInChildren<PlayerGroundedChecker>();
        pidSpeed = GetComponent<PIDController_Vector3>();
        rb.maxAngularVelocity = maxRollingAngularSpeed;
}

private void Update() {
        // Check if jumping
        if (IsGrounded && Keybinds.JumpDown()) {
            // Queue a jump for the next FixedUpdate
            // Actual jumping done in FixedUpdate to stay in sync with PlayerGroundedChecker
            jumpQueued = true;
        } else {
            jumpQueued = false;
        }
        // Check if sprinting
        if (Keybinds.Sprint() && PewterReserve.HasMass) {
            IsSprinting = true;
        } else {
            IsSprinting = false;
        }
    }

    protected override void FixedUpdate() {
        base.FixedUpdate();

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
                rb.inertiaTensor = inertiaTensorWalking;
            } else {
                rb.inertiaTensor = inertiaTensorRunning;
            }

            if (IsGrounded) {
                // if a Jump is queued
                if (jumpQueued) {
                    jumpQueued = false;

                    Rigidbody targetRb = groundedChecker.StandingOnCollider.attachedRigidbody;

                    Vector3 jumpForce;

                    // Apply Pewter Jump, if sprinting
                    if (IsSprinting) {
                        Drain(gramsPewterPerJump, timePewterPerJump);

                        particleSystem.transform.rotation = particleDirection;
                        particleSystem.transform.position = Player.PlayerInstance.transform.position + particleSystemPosition;

                        if (movement.sqrMagnitude <= .01f) { // Vertical jump
                            movement = Vector3.up;
                            // If movement is going INTO the wall, we must be jumping up it
                        } else if (Vector3.Dot(groundedChecker.Normal, movement) < -0.01f) {
                            float angle = Vector3.Angle(movement, groundedChecker.Normal) - 90;
                            movement = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, groundedChecker.Normal)) * movement;
                            if (movement.y < 0)
                                movement.y = -movement.y;
                        }
                        jumpForce = Vector3.ClampMagnitude(groundedChecker.Normal * jumpHeight + movement * jumpDirectionModifier, jumpPewterMagnitude);

                        particleDirection = Quaternion.LookRotation(-jumpForce);
                        particleSystem.transform.rotation = particleDirection;
                        particleSystem.Play();
                    } else {
                        jumpForce = groundedChecker.Normal * jumpHeight;
                    }

                    rb.AddForce(jumpForce, ForceMode.Impulse);
                    // Apply force and torque to target
                    if (targetRb) {
                        Vector3 radius = groundedChecker.Point - targetRb.worldCenterOfMass;

                        targetRb.AddForce(-jumpForce, ForceMode.Impulse);
                        targetRb.AddTorque(Vector3.Cross(radius, -jumpForce));
                    }
                }

                // If moving or pushing or pulling, apply weaker drag
                if (movement.sqrMagnitude > 0 || Player.PlayerIronSteel.IronPulling || Player.PlayerIronSteel.SteelPushing) {
                    // You: "why use ints to represent binary values that should be represented by booleans"
                    // Me, an intellectual:
                    rb.drag = SettingsMenu.settingsData.playerAirResistance * dragAirborneLinear;
                } else {
                    // If not moving and not pushing or pulling, apply stronger drag and pull player to a stop
                    rb.drag = SettingsMenu.settingsData.playerAirResistance * dragGroundedLinear;
                    rb.angularDrag = dragGroundedAngular;
                }
            } else { // If airborne, reduce movement magnitude

                rb.drag = SettingsMenu.settingsData.playerAirResistance * dragAirborneLinear;
                rb.angularDrag = dragAirborneAngular;
                //movement *= airControlFactor;
            }

            if (movement.sqrMagnitude > 0) { // If moving at all
                // Apply Pewter Sprint, if possible

                if(IsSprinting) {
                    // if sprinting
                    // Play particles if just sprinting on the ground for the first time
                    if (!lastWasSprintingOnGround && IsGrounded) {
                        particleDirection = Quaternion.LookRotation(-movement);
                        particleSystem.transform.rotation = particleDirection;
                        particleSystem.Play();
                    }
                    //movement *= pewterAcceleration * Mathf.Max(maxSprintingSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
                    rb.maxAngularVelocity = maxSprintingAngularVelocity;
                    lastWasSprintingOnGround = IsGrounded; // only show particles after hitting the ground
                } else {
                    // not Sprinting, move normally.
                    //movement = MovementMagnitude(movement);
                    rb.maxAngularVelocity = maxRollingAngularSpeed;
                    lastWasSprintingOnGround = false;
                }

                // Convert movement to torque
                //Vector3 torque = Vector3.Cross(IsGrounded ? groundedChecker.Normal : Vector3.up, movement) * torqueFactor;
                Vector3 torque = Vector3.Cross(Vector3.up, movement);

                Vector3 feedback = Vector3.Project(rb.angularVelocity, torque.normalized);
                Vector3 target = torque * maxRollingSpeed;
                if (IsSprinting)
                    target *= pewterSpeedFactor;

                torque = pidSpeed.Step(feedback, target);
                //Debug.Log("speed    : " + rb.velocity.magnitude);

                //float dot = Vector3.Dot(torque, rb.angularVelocity);
                //if(dot < 0) {
                //    torque *= 2 - Mathf.Exp(dot / dotFactor);
                //}
                // Apply torque to player
                rb.AddTorque(torque, ForceMode.Acceleration);
                // Apply a small amount of the movement force to player for tighter controls & air movement
                rb.AddForce(movement * airControlFactor * rollingAcceleration, ForceMode.Acceleration);

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
        //Vector3 feedback = Vector3.Project(rb.velocity, movement.normalized);
        //Vector3 target = movement * maxRollingSpeed;
        //Vector3 command = pidSpeed.Step(feedback, target);

        //return command;
        return movement;
        //return movement * rollingAcceleration * Mathf.Max(maxRollingSpeed - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
    }
    

    public override void Clear() {
        PewterReserve.SetMass(100);
        jumpQueued = false;
        lastWasSprintingOnGround = false;
        rb.inertiaTensor = inertiaTensorRunning;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = SettingsMenu.settingsData.playerGravity == 1;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        base.Clear();
    }

    public void EnableGravity() {
        rb.useGravity = true;
    }
    public void DisableGravity() {
        rb.useGravity = false;
    }
}
