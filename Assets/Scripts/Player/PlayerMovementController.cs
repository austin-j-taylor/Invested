using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls player movement and certain first-person camera control.
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
    //private AllomanticIronSteel playerIronSteel;
    private PlayerGroundedChecker groundedChecker;

    public bool IsGrounded {
        get {
            return groundedChecker.IsGrounded;
        }
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        //playerIronSteel = GetComponent<AllomanticIronSteel>();
        groundedChecker = GetComponentInChildren<PlayerGroundedChecker>();
    }
    void FixedUpdate() {
        // Horizontal movement
        float horiz = Keybinds.Horizontal();
        float verti = Keybinds.Vertical();
        Vector3 movement = new Vector3(horiz, 0f, verti);
        //if (!GamepadController.UsingGamepad)
        movement = Vector3.ClampMagnitude(movement, 1);
        movement = transform.TransformDirection(movement);
        Vector3 velocityInDirectionOfMovement = Vector3.Project(rb.velocity, movement.normalized);
        if (IsGrounded) {
            //// If player is trying to move in the same direction of their push, it is like they are trying to walk with their push -> use weaker drag
            //// IF the player is not trying to move in the same direction of their push, it is like they are trying to resist their puish -> use stronger drag
            //if (movement.magnitude > 0 && (Vector3.Dot(movement, playerIronSteel.LastNetForceOnAllomancer.normalized) > 0 && (playerIronSteel.IronPulling || playerIronSteel.SteelPushing))) {
            //    rb.drag = airDrag;
            //} else {
            //    rb.drag = groundedDrag;
            //}
            if (movement.magnitude > 0) {
                rb.drag = airDrag;
            } else {
                rb.drag = groundedDrag;
            }
        } else { // is airborne
            rb.drag = airDrag;
            movement *= airControlFactor;
        }
        if (movement.magnitude > 0) {
            movement *= acceleration * Mathf.Max(maxRunningSpeed - velocityInDirectionOfMovement.magnitude, 0);
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
}
