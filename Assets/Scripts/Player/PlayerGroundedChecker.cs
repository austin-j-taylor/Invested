using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Every frame, checks if the player is just above a collider (i.e. is grounded).
 * "Pewter jumps:" While holding a movement key while jumping, you will jump further in that direction horizontally.
 *      If you are against a wall and try to move into the wall while jumping, you'll instead jump more vertically
 */
public class PlayerGroundedChecker : MonoBehaviour {

    private const float jumpHeight = 400;
    private const float jumpPewterMagnitude = 600;
    private const float jumpDirectionModifier = 350;

    public bool IsGrounded { get; private set; } = false;

    private bool isInCollider = false;
    private ParticleSystem particleSystem;
    private Collider standingOnCollider = null;
    private Vector3 point;
    private Vector3 normal;

    private void Awake() {
        particleSystem = transform.parent.GetComponentInChildren<ParticleSystem>();
    }

    private void FixedUpdate() {
        IsGrounded = isInCollider;
        isInCollider = false;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    // Debug
    Vector3 lastJump;
    Vector3 lastForce;
    Vector3 lastMovement;
    private void Update() {
        Debug.DrawLine(Player.PlayerInstance.transform.position + new Vector3(0, 1, 0), Player.PlayerInstance.transform.position + new Vector3(0, 1, 0) + lastJump, Color.blue);
        Debug.DrawLine(Player.PlayerInstance.transform.position + new Vector3(0, 1, 0), Player.PlayerInstance.transform.position + new Vector3(0, 1, 0) + lastMovement, Color.yellow);
        Debug.DrawLine(Player.PlayerInstance.transform.position + new Vector3(0, 1, 0), Player.PlayerInstance.transform.position + new Vector3(0, 1, 0) + lastForce, Color.green);

        particleSystem.transform.rotation = Quaternion.identity;
    }

    //private void OnTriggerStay(Collider other) {
    //    if (!isInCollider) // Only check first collision this frame
    //        if (!other.isTrigger) {
    //            isInCollider = true;
    //            standingOnCollider = other;

    //            if (Physics.SphereCast(transform.position, GetComponent<SphereCollider>().radius, new Vector3(.01f, .01f, .01f), out RaycastHit hit)) {
    //                Debug.Log("found" + transform.position);
    //                point = hit.point;
    //                Debug.DrawLine(Player.PlayerInstance.transform.position, hit.point, Color.red);

    //                normal = hit.normal;
    //            } else {
    //                Debug.Log("missed");
    //            }
    //        }
    //}

    private void OnCollisionStay(Collision collision) {
        if (!isInCollider) // Only check first collision this frame
            if (!collision.collider.isTrigger) {
                isInCollider = true;
                standingOnCollider = collision.collider;
                point = collision.GetContact(0).point;
                normal = collision.GetContact(0).normal;
            }
    }

    public void Jump(Vector3 movement) {
        Rigidbody targetRb = standingOnCollider.attachedRigidbody;
        Vector3 force = normal;

        // If Pewter Jumping
        if (movement.sqrMagnitude > 0f) {


            if (Vector3.Dot(force, movement) < -0.01f) {
                float angle = Vector3.Angle(movement, force);
                angle -= 90;
                movement = Quaternion.AngleAxis(angle, Vector3.Cross(Vector3.up, force)) * movement;
                if (movement.y < 0)
                    movement.y = -movement.y;
            }
            lastForce = force.normalized;

            force = force * jumpHeight + movement * jumpDirectionModifier;
            Vector3.ClampMagnitude(force, jumpPewterMagnitude);

            //particleSystem.Play();

        } else {
            force *= jumpHeight;
        }

        lastJump = force.normalized;
        lastMovement = movement.normalized;

        Player.PlayerInstance.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        // Apply force and torque to target
        if (targetRb) {
            Vector3 radius = point - targetRb.centerOfMass;

            targetRb.AddForce(-force, ForceMode.Impulse);
            targetRb.AddTorque(Vector3.Cross(radius, -force));
        }
    }
}
