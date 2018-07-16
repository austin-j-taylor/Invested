using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Controls player movement and certain first-person camera control.
 * 
 */

public class PlayerMovementController : MonoBehaviour {

    private const float speed = 10f;
    private const float maxRunningSpeed = 5f;
    private const float airControlFactor = .2f;
    private const float movingDrag = .2f;
    private const float groundedDrag = movingDrag;//5f;
    private readonly Vector3 jumpHeight = new Vector3(0, 5f, 0);


    private Rigidbody rb;
    private PlayerJumpChecker jumpChecker;

    public bool IsGrounded {
        get {
            return jumpChecker.IsGrounded;
        }
    }

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        jumpChecker = GetComponentInChildren<PlayerJumpChecker>();
    }
    public float vel;
    void FixedUpdate() {

        // Horizontal movement
        float horiz = Keybinds.Horizontal();
        float verti = Keybinds.Vertical();
        Vector3 movement = new Vector3(horiz, 0f, verti);
        //if (!GamepadController.UsingGamepad)
        movement = Vector3.ClampMagnitude(movement, 1);
        movement = transform.TransformDirection(movement);

        Vector3 velocityInDirectionOfMovement = Vector3.Project(rb.velocity, movement.normalized);
        vel = velocityInDirectionOfMovement.magnitude;
        if (IsGrounded) {
            
            if (movement.magnitude > 0) {
                rb.drag = movingDrag;
                
                if (velocityInDirectionOfMovement.magnitude < maxRunningSpeed) {
                    //rb.MovePosition(transform.position + transform.rotation * (movement) * speed * Time.deltaTime);
                    //rb.AddForce(transform.TransformDirection(movement) * speed, ForceMode.Acceleration);
                    rb.AddForce(movement * speed, ForceMode.Acceleration);
                }
            } else { // is not moving
                rb.drag = groundedDrag;
            }

        } else { // is airborne
            rb.drag = movingDrag;
            if (velocityInDirectionOfMovement.magnitude < maxRunningSpeed) {
                rb.AddForce(movement * speed * airControlFactor, ForceMode.Acceleration);
            }
        }
    }

    private void Update() {
        if(IsGrounded) {
            // Jump
            if (Keybinds.JumpDown()) {
                rb.AddForce(jumpHeight, ForceMode.VelocityChange);
            }
        }
    }
}
