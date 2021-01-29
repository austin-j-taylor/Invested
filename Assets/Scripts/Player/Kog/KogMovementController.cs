using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KogMovementController : MonoBehaviour {

    public enum WalkingState { Idle, Walking, Sprinting, Anchored };

    #region constants
    public float topSpeed = 3.5f;
    public float topSpeedSprinting = 12.5f;
    public float topSpeedAnchored = 2;
    //[SerializeField]
    //private float acceleration = 2.5f;
    //[SerializeField]
    //private float accelerationSprinting = 5f;

    //[SerializeField]
    //private float rotationalTopSpeed = 5;
    //[SerializeField]
    //private float rotationalAcceleration = 2.5f;

    [SerializeField]
    private float jumpHeight = 900;

    [SerializeField]
    private float speed_P = 10, speed_mD = 50, orientation_P = 10;
    #endregion

    #region properties
    public WalkingState State { get; private set; }
    public bool IsGrounded => Kog.KogAnimationController.IsGrounded;
    public Vector3 Movement { get; private set; } = Vector3.zero;
    #endregion

    private Rigidbody rb;
    private Vector3 lastInput; // The last horizontal/vertical movement command sent to the player
    private Vector3 bodyLookAtDirection;
    private PIDController_Vector3 pidSpeed;
    private PIDController_float pidOrientation;
    private Vector3 lastMoveDirection;
    private bool jumpQueued;
    private float lastJumpTime;

    #region clearing
    protected void Awake() {
        rb = GetComponent<Rigidbody>();
        pidSpeed = gameObject.AddComponent<PIDController_Vector3>();
        pidSpeed.SetParams(speed_P, 0, 0, speed_mD);
        pidOrientation = gameObject.AddComponent<PIDController_float>();
        pidOrientation.SetParams(orientation_P, 0, 0);
    }

    private void Start() {
        Clear();
    }

    public void Clear() {
        State = WalkingState.Idle;
        lastInput = Vector3.zero;
        bodyLookAtDirection = transform.forward;
        lastMoveDirection = transform.forward;
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

                    Rigidbody targetRb = Kog.KogAnimationController.StandingOnRigidbody;

                    Vector3 jumpForce = Vector3.up * jumpHeight;

                    rb.AddForce(jumpForce, ForceMode.Impulse);
                    // Apply force and torque to target
                    if (targetRb) {
                        Vector3 radius = Kog.KogAnimationController.StandingOnPoint - targetRb.worldCenterOfMass;

                        targetRb.AddForce(-jumpForce, ForceMode.Impulse);
                        targetRb.AddTorque(Vector3.Cross(radius, -jumpForce));
                    }
                }

            } // Not grounded
              // "Short hopping": if you tap the jump button instead of holding it, do a short hop.
              // Effectively, if you release the jump button within a short window of jumping,
              //  apply a small negative impulse to reduce the jump height.
            if (!Keybinds.Jump() && Time.unscaledTime - lastJumpTime < PrimaMovementController.shortHopThreshold) {
                rb.AddForce(-Kog.KogAnimationController.StandingOnNormal * jumpHeight / 2, ForceMode.Impulse);
                lastJumpTime = -1;
            }


            // Convert user input to movement vector
            // If the control wheel is open, use the last input for movement
            
            if (HUD.ControlWheelController.IsOpen) {
                Movement = lastInput;
            } else {
                Movement = new Vector3(Keybinds.Horizontal(), 0f, Keybinds.Vertical());
                lastInput = Movement;
            }

            // If is unclamped and upside-down, keep movement in an intuitive direction for the player
            // Rotate movement to be in direction of camera and clamp magnitude
            if (SettingsMenu.settingsGameplay.cameraClamping == 0 && CameraController.UpsideDown) {
                Vector3 mov = Movement;
                mov.x = -Movement.x;
                Movement = -(CameraController.CameraDirection * Vector3.ClampMagnitude(mov, 1));
            } else {
                Movement = CameraController.CameraDirection * Vector3.ClampMagnitude(Movement, 1);
            }
            bool wantToMove = Movement.sqrMagnitude > 0;

            // Transitions
            switch (State) {
                case WalkingState.Idle:
                    if (wantToMove) {
                        if (Keybinds.Sprint()) {
                            State = WalkingState.Sprinting;
                        } else if (Keybinds.Walk()) {
                            State = WalkingState.Anchored;
                        } else {
                            State = WalkingState.Walking;
                        }
                    } else {
                        if (Keybinds.Walk()) {
                            State = WalkingState.Anchored;
                        }
                    }
                    break;
                case WalkingState.Walking:
                    if (wantToMove) {
                        if (Keybinds.Sprint()) {
                            State = WalkingState.Sprinting;
                        } else if (Keybinds.Walk()) {
                            State = WalkingState.Anchored;
                        }
                    } else {
                        State = WalkingState.Idle;
                    }
                    break;
                case WalkingState.Sprinting:
                    if (wantToMove) {
                        if (Keybinds.Walk()) {
                            State = WalkingState.Anchored;
                        } else if (!Keybinds.Sprint()) {
                            State = WalkingState.Walking;
                        }
                    } else {
                        State = WalkingState.Idle;
                    }
                    break;
                case WalkingState.Anchored:
                    if (wantToMove) {
                        if (!Keybinds.Walk()) {
                            if (Keybinds.Sprint()) {
                                State = WalkingState.Sprinting;
                            } else {
                                State = WalkingState.Walking;
                            }
                        }
                    } else {
                        if (!Keybinds.Walk()) {
                            State = WalkingState.Idle;
                        }
                    }
                    break;
            }

            if(wantToMove)
                lastMoveDirection = Movement;

            pidSpeed.SetParams(speed_P, 0, 0, speed_mD);
            pidOrientation.SetParams(orientation_P, 0, 0);

            // PID control for speed 
            Vector3 target = Vector3.zero;
            // Scale target speed with sprinting, etc.
            switch (State) {
                case WalkingState.Idle:
                    //target = Vector3.zero;
                    break;
                case WalkingState.Walking:
                    target = Movement * topSpeed;
                    break;
                case WalkingState.Sprinting:
                    target = Movement * topSpeedSprinting;
                    break;
                case WalkingState.Anchored:
                    target = Movement * topSpeedAnchored;
                    break;
            }

            Vector3 feedback = rb.velocity;
            feedback.y = 0;
            Vector3 output = pidSpeed.Step(feedback, target);
            rb.AddForce(output, ForceMode.Acceleration);

            Kog.KogAnimationController.SetLegTarget(target, feedback.magnitude);

            // PID control for orientation
            float feedback_O = IMath.AngleBetween_Signed(transform.forward, bodyLookAtDirection, Vector3.up, true);
            float output_O = pidOrientation.Step(feedback_O, 0);
            //Debug.Log(" movement: " + Movement.normalized + " feedback: " + feedback_O + " error: " + (feedback_O - target_O) + " output: " + output_O);
            rb.AddTorque(Vector3.up * output_O, ForceMode.Acceleration);
            transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
        }
    }
    #endregion

    /// <summary>
    /// Sets the target position for the body to face.
    /// </summary>
    /// <param name="target">the target position</param>
    public void SetBodyLookAtTarget(Vector3 target) {
        bodyLookAtDirection = target;
    }
}
