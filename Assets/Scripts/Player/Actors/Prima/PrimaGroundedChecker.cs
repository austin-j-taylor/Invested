using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Every frame, checks if the player is touching a collider (i.e. is able to jump, either from the ground or off of a wall).
/// </summary>
public class PrimaGroundedChecker : MonoBehaviour {

    private const float feetRange = 0.10f;
    private readonly Vector3 feetOffset = new Vector3(0, -.25f, 0);

    public bool IsGrounded { get { return StandingOnCollider != null; } }

    public Collider StandingOnCollider { get; private set; } = null;
    public Vector3 Point { get; private set; }
    public Vector3 Normal { get; private set; }

    private void OnCollisionEnter(Collision collision) {
        if (!collision.collider.isTrigger) {
            OnCollisionStay(collision);
        }
    }

    /*
     * As long as the player is touching a surface, the player will be able to jump off of that surface.
     */
    private void OnCollisionStay(Collision collision) {
        if (StandingOnCollider == null)
            StandingOnCollider = collision.collider;
        if (collision.collider == StandingOnCollider) // Only check first collision
            if (!collision.collider.isTrigger) {
                Point = collision.GetContact(0).point;
                Normal = collision.GetContact(0).normal;
            }
    }

    /*
     * If the player's body stops touching a surface, they can still jump off of it for a brief time...
     * ...Until the player gets slightly farther away from the surface.
     * This facilitates wall-jumping, because the player may not be perfectly touching the wall when they try to jump.
     */
    private void OnTriggerExit(Collider other) {
        StandingOnCollider = null;
    }

    /// <summary>
    /// Raycast downward; if there's a ground very close to your feet, always try to jump from that.
    /// </summary>
    public void UpdateStanding() {
        if (Physics.Raycast(transform.position + feetOffset, Vector3.down, out RaycastHit hit, feetRange, GameManager.Layer_IgnorePlayer)) {
            if (StandingOnCollider != hit.collider) {
                StandingOnCollider = hit.collider;
                Normal = hit.normal;
                Point = hit.point;
            }
        }
    }
}
