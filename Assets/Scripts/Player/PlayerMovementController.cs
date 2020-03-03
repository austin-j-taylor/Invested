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

    public const float radius = .26f; // radius of the player sphere collider
    // Rolling
    public const float rollingAcceleration = 5f;
    public const float targetRollingSpeed = 7.5f;
    public const float targetRollingSpeedRadial = targetRollingSpeed / radius;
    //private const float maxRollingAngularSpeed = Mathf.Infinity; // targetRollingSpeedRadial;
    // Pewter
    private const float targetSprintingSpeed = 20f;
    private const float targetSprintingSpeedRadial = targetSprintingSpeed / radius;
    //private const float maxSprintingAngularVelocity = Mathf.Infinity;
    // Pewter burning
    protected const double gramsPewterPerJump = .25f;
    protected const float timePewterPerJump = .5f;
    // Jumping
    private const float jumpHeight = 300;
    private const float jumpDirectionModifier = 400;
    private const float jumpPewterMagnitude = 350;

    // Factors
    private const float walkingFactor = .15f;
    private const float airControlFactor = .3f;
    private const float dotFactor = 10;
    // Air resistance
    [SerializeField]
    private float dragAirborneLinear = .2f;
    [SerializeField]
    private float dragGroundedLinear = 3f;
    [SerializeField]
    private float dragAirborneAngular = 1.5f;
    [SerializeField]
    private float dragGroundedAngular = 3f;
    [SerializeField]
    private const float dragNoControl = 10f;

    private readonly Vector3 particleSystemPosition = new Vector3(0, -.2f, 0);

    // Friction
    [SerializeField]
    private float frictionDynamicRolling = 6;
    [SerializeField]
    private float frictionDynamicSprinting = 10f;
    [SerializeField]
    private float frictionDynamicWalking = 15f;
    // Misc
    [SerializeField]
    private float momentOfInertiaMagnitude = 5;
    [SerializeField]
    private float momentOfInertiaMagnitudeSprinting = 25;
    [SerializeField]
    private float momentOfInertiaMagnitudeWalking = 50;
    //private  Vector3 inertiaTensorRunning = new Vector3(momentOfInertiaMagnitude, momentOfInertiaMagnitude, momentOfInertiaMagnitude);
    //private  Vector3 inertiaTensorWalking = new Vector3(momentOfInertiaMagnitudeWalking, momentOfInertiaMagnitudeWalking, momentOfInertiaMagnitudeWalking);
    // For "Spider-Man Swinging" effect
    [SerializeField]
    private float swinging_gain_P = 0.05f;
    private Vector3 lastFrom = Vector3.zero;
    private float lastDeltaW = 0;

    private PlayerGroundedChecker groundedChecker;
    private PIDController_Vector3 pidSpeed;
    private PhysicMaterial physicsMaterial;

    public bool IsGrounded {
        get {
            return groundedChecker.IsGrounded;
        }
    }
    public bool IsWalking { get; private set; }
    private bool jumpQueued;
    private bool lastWasSprintingOnGround;
    private bool invertGravity = false;

    protected override void Awake() {
        base.Awake();
        groundedChecker = transform.GetComponentInChildren<PlayerGroundedChecker>();
        pidSpeed = GetComponent<PIDController_Vector3>();
        //rb.maxAngularVelocity = maxRollingAngularSpeed;
        rb.maxAngularVelocity = Mathf.Infinity;
        physicsMaterial = GetComponent<Collider>().material;
    }

    private void Update() {
        if (Player.CanControl && Player.CanControlMovement) {
            // walking/rolling/sprinting state machine
            if (IsWalking) {
                // was walking
                if (Keybinds.Sprint() && PewterReserve.HasMass) {
                    // start sprinting
                    IsSprinting = true;
                    IsWalking = false;
                    rb.inertiaTensor = new Vector3(momentOfInertiaMagnitude, momentOfInertiaMagnitude, momentOfInertiaMagnitude);
                    Player.PlayerFlywheelController.Retract();
                } else if (!Keybinds.Walk()) {
                    // stop rolling
                    IsWalking = false;
                    rb.inertiaTensor = new Vector3(momentOfInertiaMagnitude, momentOfInertiaMagnitude, momentOfInertiaMagnitude);
                    Player.PlayerFlywheelController.Retract();
                } // continue walking
            } else if (IsSprinting) {
                // was sprinting
                if (!Keybinds.Sprint() || !PewterReserve.HasMass) {
                    // start rolling
                    IsSprinting = false;
                } // continue sprinting
            } else {
                // was rolling
                if (Keybinds.Sprint() && PewterReserve.HasMass) {
                    // start sprinting
                    IsSprinting = true;

                } else if (Keybinds.Walk()) {
                    // start walking
                    rb.inertiaTensor = new Vector3(momentOfInertiaMagnitudeWalking, momentOfInertiaMagnitudeWalking, momentOfInertiaMagnitudeWalking);
                    Player.PlayerFlywheelController.Extend();
                    IsWalking = true;
                } // continue rolling
            }

            // Check if jumping
            if (IsGrounded && Keybinds.JumpDown()) {
                // Queue a jump for the next FixedUpdate
                // Actual jumping done in FixedUpdate to stay in sync with PlayerGroundedChecker
                jumpQueued = true;
            } else {
                jumpQueued = false;
            }
        } else {
            jumpQueued = false;
            IsSprinting = false;
            IsWalking = false;
        }
    }
    protected override void FixedUpdate() {
        base.FixedUpdate();

        // "Spider-Man Swinging"
        // When the sphere is swinging through the air, rotate the player body to give the effect
        // that the player is physically swinging through the air, connected to the target by something like a rope.
        // Uses a simple Proportional error controller on the angle between the player and the target to get the effect.
        if (Player.PlayerIronSteel.PushingOrPulling) {
            // Get the angle between the player and the target
            Vector3 targetPosition = transform.position + Player.PlayerIronSteel.LastNetForceOnAllomancer;
            Vector3 to = transform.position - targetPosition;
            Vector3 from;
            Debug.DrawRay(transform.position, to, Color.yellow);
            if (lastFrom == Vector3.zero) { // if this is the first frame of spinning
                from = to;
            } else {
                from = lastFrom;
            }
            //Debug.DrawRay(transform.position, from, Color.red);

            // Make the player rotate based on that angle
            float expectedDeltaW = Vector3.Angle(from, to);
            float deltaW = swinging_gain_P * (expectedDeltaW - lastDeltaW);
            //Debug.Log(deltaW + " = " + P + " * (" + expectedDeltaW + " - " + lastDeltaW + ")");

            Vector3 torqueDirection = Vector3.Cross(from, to).normalized;
            rb.AddTorque(torqueDirection * deltaW, ForceMode.VelocityChange);

            lastDeltaW = deltaW;
            lastFrom = to;
        } else {
            lastFrom = Vector3.zero;
            lastDeltaW = 0;
        }

        // There's an option in the settings to invert gravity. Aplpy that here.
        if (invertGravity) {
            rb.AddForce(-Physics.gravity, ForceMode.Acceleration);
        }

        // Handle all player movement control
        if (Player.CanControl && Player.CanControlMovement) {
            // Convert user input to movement vector
            Vector3 movement = new Vector3(Keybinds.Horizontal(), 0f, Keybinds.Vertical());

            // If is unclamped and upside-down, keep movement in an intuitive direction for the player
            // Rotate movement to be in direction of camera and clamp magnitude
            if (SettingsMenu.settingsData.cameraClamping == 0 && CameraController.UpsideDown) {
                movement.x = -movement.x;
                movement = CameraController.CameraDirection * Vector3.ClampMagnitude(movement, 1);
                movement = -movement;
            } else {
                movement = CameraController.CameraDirection * Vector3.ClampMagnitude(movement, 1);
            }

            if (IsGrounded) {
                // if a Jump is queued
                if (jumpQueued) {
                    jumpQueued = false;

                    groundedChecker.UpdateStanding();
                    Rigidbody targetRb = groundedChecker.StandingOnCollider.attachedRigidbody;

                    Vector3 jumpForce;

                    // Apply Pewter Jump, if sprinting
                    if (IsSprinting && PewterReserve.HasMass) {
                        Vector3 movementForPewter = CameraController.UpsideDown ? -movement : movement;

                        if (movementForPewter.sqrMagnitude <= .01f) { // Vertical jump. Jump straight up.
                            movementForPewter = groundedChecker.Normal;
                            //movement = Vector3.up;
                        } else if (Vector3.Dot(groundedChecker.Normal, movementForPewter) < -0.01f) { // Wall jump. Kick up off of wall.
                            float angle = Vector3.Angle(movementForPewter, groundedChecker.Normal) - 90;
                            movementForPewter = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, groundedChecker.Normal)) * movementForPewter;
                            if (CameraController.UpsideDown) {
                                if (movementForPewter.y > 0)
                                    movementForPewter.y = -movementForPewter.y;
                            } else {
                                if (movementForPewter.y < 0)
                                    movementForPewter.y = -movementForPewter.y;
                            }
                        } // Either jumping in a direction or kicking off of a wall. Either way, do nothing special.
                        jumpForce = Vector3.ClampMagnitude(groundedChecker.Normal * jumpHeight + movementForPewter * jumpDirectionModifier, jumpPewterMagnitude);

                        HitSurface(-jumpForce);
                        Drain(-jumpForce, gramsPewterPerJump, timePewterPerJump);
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
                    rb.angularDrag = dragAirborneAngular;
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

            //float sprintingAcceleration = 0;
            if (movement.sqrMagnitude > 0) { // If moving at all
                // Apply Pewter Sprint, if possible

                if (IsSprinting) {
                    // if sprinting
                    // Play particles if just sprinting on the ground for the first time
                    if (!lastWasSprintingOnGround && IsGrounded) {
                        HitSurface(-groundedChecker.Normal - movement.normalized);
                    }
                    lastWasSprintingOnGround = IsGrounded; // only show particles after hitting the ground

                    //// Pewter acceleration: The faster you are going, the more you can keep going.
                    //// 1x movement @ movement = half of sprinting speed
                    //// i.e. the movement starts to increase when you are rolling above half the speed you can normally roll
                    //// This factor is applied to the target speed for the the PID loop
                    //float ratio = rb.angularVelocity.magnitude / targetRollingSpeedRadial * 4;
                    //sprintingAcceleration = Mathf.Log(1 + ratio) / 4;
                    //Debug.Log("Ratio: " + ratio + " ... " + sprintingAcceleration);

                    //if (sprintingAcceleration < 0)
                    //    sprintingAcceleration = 0;

                    //rb.maxAngularVelocity = maxSprintingAngularVelocity;
                    physicsMaterial.dynamicFriction = frictionDynamicSprinting;
                    rb.inertiaTensor = new Vector3(momentOfInertiaMagnitudeSprinting, momentOfInertiaMagnitudeSprinting, momentOfInertiaMagnitudeSprinting);
                } else {
                    // not Sprinting, move normally.
                    //movement = MovementMagnitude(movement);
                    //rb.maxAngularVelocity = maxRollingAngularSpeed;
                    lastWasSprintingOnGround = false;

                    // If Walking, reduce movment
                    if (IsWalking) {
                        movement *= walkingFactor;
                        physicsMaterial.dynamicFriction = frictionDynamicWalking;
                    } else {
                        physicsMaterial.dynamicFriction = frictionDynamicRolling;
                    }
                }

                // Convert movement to torque
                Vector3 torque = Vector3.Cross(Vector3.up, movement);

                Vector3 feedback = rb.angularVelocity;
                // Also: add Force acting on the player from its Pushes/Pulls as desired movement
                Vector3 target = torque * (IsSprinting ? targetSprintingSpeedRadial : targetRollingSpeedRadial)/* + Vector3.Cross(Vector3.up, Player.PlayerIronSteel.LastNetForceOnAllomancer)*/;

                torque = pidSpeed.Step(feedback, target);
                //Debug.Log("speed    : " + rb.velocity.magnitude);

                //float dot = Vector3.Dot(torque, rb.angularVelocity);
                //if(dot < 0) {
                //    torque *= 2 - Mathf.Exp(dot / dotFactor);
                //}
                // Apply torque to player
                rb.AddTorque(torque, ForceMode.Acceleration);
                // Spin flywheels to match movement
                Player.PlayerFlywheelController.SpinToTorque(torque);
                // Apply a small amount of the movement force to player for tighter controls & air movement
                rb.AddForce((CameraController.UpsideDown ? -movement : movement) * airControlFactor * rollingAcceleration, ForceMode.Acceleration);
                 
                // Debug
                //Debug.DrawRay(transform.position, rb.angularVelocity, Color.red);
                //Debug.DrawRay(transform.position, torque, Color.white);
                //Debug.DrawRay(transform.position, movement, Color.blue);
            }
        } else {
            rb.drag = SettingsMenu.settingsData.playerAirResistance * dragNoControl;
        }

    }

    // Convert a movement vector into real player movement based on current velocity
    private Vector3 MovementMagnitude(Vector3 movement) {
        //Vector3 feedback = Vector3.Project(rb.velocity, movement.normalized);
        //Vector3 target = movement * targetRollingSpeedRadial;
        //Vector3 command = pidSpeed.Step(feedback, target);

        //return command;
        return movement;
        //return movement * rollingAcceleration * Mathf.Max(targetRollingSpeedRadial - Vector3.Project(rb.velocity, movement.normalized).magnitude, 0);
    }


    public override void Clear() {
        jumpQueued = false;
        lastWasSprintingOnGround = false;
        //rb.inertiaTensor = inertiaTensorRunning;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = SettingsMenu.settingsData.playerGravity == 1;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints.None;
        base.Clear();
    }

    public void EnableGravity() {
        rb.useGravity = true;
        invertGravity = false;
    }
    public void DisableGravity() {
        rb.useGravity = false;
        invertGravity = false;
    }
    public void InvertGravity() {
        rb.useGravity = false;
        invertGravity = true;
    }
}
