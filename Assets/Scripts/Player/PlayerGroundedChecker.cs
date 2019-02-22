using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Every frame, checks if the player is in contact with a collider (i.e. is able to jump, either from the ground or off of a wall).
 * "Pewter jumps:" While holding a movement key while jumping, you will jump further in that direction horizontally.
 *      If you are against a wall and try to move into the wall while jumping, you'll instead jump more vertically
 */
public class PlayerGroundedChecker : MonoBehaviour {


    public bool IsGrounded { get { return standingOnCollider != null; } }
    
    private Collider standingOnCollider = null;
    private Vector3 point;
    private Vector3 normal;
    
    private void OnCollisionEnter(Collision collision) {
        if (!collision.collider.isTrigger) {
            OnCollisionStay(collision);
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
                point = collision.GetContact(0).point;
                normal = collision.GetContact(0).normal;
            }
    }

    /*
     * If the player's body stops touching a surface, they can still jump off of it for a brief time...
     * ...Until the player gets slightly farther away from the surface.
     * This facilitates wall-jumping, because the player may not be perfectly touching the wall when they try to jump.
     */
    private void OnTriggerExit(Collider other) {
        if (other == standingOnCollider) {
            standingOnCollider = null;
        }
    }

    public void Jump(Vector3 movement) {
        Rigidbody targetRb = standingOnCollider.attachedRigidbody;
        Vector3 force = Player.PlayerPewter.Jump(movement, normal);
        
        Player.PlayerInstance.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
        // Apply force and torque to target
        if (targetRb) {
            Vector3 radius = point - targetRb.worldCenterOfMass;

            targetRb.AddForce(-force, ForceMode.Impulse);
            targetRb.AddTorque(Vector3.Cross(radius, -force));
        }
    }
}
