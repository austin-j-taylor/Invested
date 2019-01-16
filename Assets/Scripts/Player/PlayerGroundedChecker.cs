using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Every frame, checks if the player is in contact with a collider (i.e. is able to jump, either from the ground or off of a wall).
 * "Pewter jumps:" While holding a movement key while jumping, you will jump further in that direction horizontally.
 *      If you are against a wall and try to move into the wall while jumping, you'll instead jump more vertically
 */
public class PlayerGroundedChecker : MonoBehaviour {

    private const float fallDamageSpeedThreshold = 7; // any fall speed above 7 m/s -> painful

    private const float jumpHeight = 400;
    private const float jumpPewterMagnitude = 600;
    private const float jumpDirectionModifier = 350;

    public bool IsGrounded { get; private set; } = false;

    private bool isInCollider = false;
    private ParticleSystem particleSystem;
    private Collider standingOnCollider = null;
    private Vector3 point;
    private Vector3 normal;
    private Quaternion particleDirection;

    private void Awake() {
        particleSystem = transform.parent.GetComponentInChildren<ParticleSystem>();
    }

    private void FixedUpdate() {
        IsGrounded = isInCollider;
    }

    // Debug
    Vector3 lastJump;
    Vector3 lastForce;
    Vector3 lastMovement;
    private void Update() {
        Debug.DrawLine(Player.PlayerInstance.transform.position + new Vector3(0, 1, 0), Player.PlayerInstance.transform.position + new Vector3(0, 1, 0) + lastJump, Color.blue);
        Debug.DrawLine(Player.PlayerInstance.transform.position + new Vector3(0, 1, 0), Player.PlayerInstance.transform.position + new Vector3(0, 1, 0) + lastMovement, Color.yellow);
        Debug.DrawLine(Player.PlayerInstance.transform.position + new Vector3(0, 1, 0), Player.PlayerInstance.transform.position + new Vector3(0, 1, 0) + lastForce, Color.green);

        if (normal != null)
            particleSystem.transform.rotation = particleDirection;
    }

    /*
     * If the player enters a collision with a high velocity, they should take damage (eventually. Now, just show some particle effects.)
     */
    private void OnCollisionEnter(Collision collision) {
        if (!collision.collider.isTrigger) {


            OnCollisionStay(collision);

            // If this was a hard fall, show a particle effect.
            Vector3 vel = Player.PlayerInstance.GetComponent<Rigidbody>().velocity;
            if (Vector3.Project(vel, normal).magnitude > fallDamageSpeedThreshold) {
                particleDirection = Quaternion.LookRotation(-normal);
                particleSystem.transform.rotation = particleDirection;
                particleSystem.Play();
            }
        }
    }

    /*
     * As long as the player is touching a surface, the player will be able to jump off of that surface.
     */
    private void OnCollisionStay(Collision collision) {
        if (standingOnCollider == null)
            standingOnCollider = collision.collider;

        if (collision.collider == standingOnCollider) // Only check first collision
            if (!collision.collider.isTrigger) {
                isInCollider = true;
                //Debug.Log(point);
                point = collision.GetContact(0).point;
                normal = collision.GetContact(0).normal;
            }
    }

    /*
     * If the player's body stops touching a surface, they can still jump off of it for a brief time...
     */
    //private void OnCollisionExit(Collision collision) {
    //    if (collision.collider == standingOnCollider) {

    //    }
    //}

    /*
     * ...Until the player gets slightly farther away from the surface.
     * This facilitates wall-jumping, because the player may not be perfectly touching the wall when they try to jump.
     */
    private void OnTriggerExit(Collider other) {
        if (other == standingOnCollider) {
            isInCollider = false;
            standingOnCollider = null;
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

            particleDirection = Quaternion.LookRotation(-force);
            particleSystem.transform.rotation = particleDirection;
            particleSystem.Play();

        } else {
            force *= jumpHeight;
        }

        lastJump = force.normalized;
        lastMovement = movement.normalized;

        Player.PlayerInstance.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        // Apply force and torque to target
        if (targetRb) {
            Vector3 radius = point - targetRb.worldCenterOfMass;

            targetRb.AddForce(-force, ForceMode.Impulse);
            targetRb.AddTorque(Vector3.Cross(radius, -force));
        }
    }
}
