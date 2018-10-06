using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Magnetic {

    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;
    private const float equalInMagnitudeConstant = .01f;
    private const float drag = 1.25f;
    private const float stuckThreshold = 100f; // Square magnitude of normal force necessary for friction
    
    private bool lastWasStuck = false;

    // Used for pseudo-parenting Coin when stuck to object it collides with
    private Transform collidedCollider;
    private Vector3 collisionNormal;
    private Quaternion startParentRotation;
    private Vector3 startChildPosition;
    private Quaternion startChildRotation;

    public override bool IsPerfectlyAnchored { // Only matters for Coins, which have so low masses that Unity thinks they have high velocities when pushed, even when anchored
        get {
            return Mathf.Abs(LastPosition.sqrMagnitude - transform.position.sqrMagnitude) < equalInMagnitudeConstant;
        }
    }
    private bool isStuck;

    private bool IsStuck {
        get {
            return isStuck;
        }
        set {
            isStuck = value;
            if (!value)
                lastWasStuck = false;
        }
    }

    public new void Clear() {
        base.Clear();
        collidedCollider = null;
        IsStuck = false;
        lastWasStuck = false;
    }

    private void OnCollisionEnter(Collision collision) {
        OnCollisionStay(collision);
    }

    /*
     * Simulates friction and makes the target stuck to the colliding surface
     * 
     * If stuck, the coin is pseudo-parented to the object it collided with.
     * collisionNormal needs to be constantly updated for each OnCollisionStay
     */
    private void OnCollisionStay(Collision collision) {
        if (!collision.collider.CompareTag("Player")) {
            // Could be colliding with multiple objects.
            // Only care about the oldest one, for now.
            if(collidedCollider == collision.transform || collidedCollider == null) {
                collidedCollider = collision.transform;
                collisionNormal = collision.contacts[0].normal;

                if (Allomancer && (Allomancer.SteelPushing || Allomancer.IronPulling)) {

                    IsStuck = IsStuckByFriction(collision.impulse / Time.deltaTime);
                    if (IsStuck) {
                        if (!lastWasStuck) {
                            // First frame of being stuck.
                            startParentRotation = collidedCollider.rotation;
                            startChildPosition = collision.contacts[0].point; // Coin embeds itself in the target collider. Intended behavior?
                            startChildRotation = transform.rotation;

                            // Keeps child position from being modified at the start by the parent's initial transform
                            Vector3 num = Quaternion.Inverse(collidedCollider.rotation) * (startChildPosition - collidedCollider.position);
                            Vector3 den = collidedCollider.lossyScale;
                            startChildPosition = new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);
                        }
                        UpdatePositionToParent();
                        Rb.velocity = Vector3.zero;
                    }

                    lastWasStuck = IsStuck;
                }
            } else {
                // A new collision has occured
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collidedCollider == collision.transform) {
            collisionNormal = Vector3.zero;
            collidedCollider = null;
            IsStuck = false;
        } else {
            // A different collision has ended
        }
    }

    private void OnTriggerStay(Collider other) {
        if (Keybinds.IronPulling() && other.CompareTag("Player")) {
            BeCaughtByAllomancer(other.GetComponent<Player>());
        }
    }

    public override void AddForce(Vector3 netForce) {
        // Calculate drag from its new velocity
        // Effectively caps the max velocity of the coin without affecting the ANF.
        Vector3 newNetForce = Vector3.ClampMagnitude(
            -(Vector3.Project(Rb.velocity, netForce.normalized) + (netForce / NetMass * Time.fixedDeltaTime)) * drag, netForce.magnitude
        ) + netForce;

        if (Allomancer && (Allomancer.SteelPushing || Allomancer.IronPulling)) {
            IsStuck = Vector3.Dot(newNetForce, collisionNormal) < 0 && IsStuckByFriction(newNetForce);
        }
        LastExpectedAcceleration = newNetForce / NetMass; // LastPosition, LastVelocity are updated
        Rb.AddForce(newNetForce, ForceMode.Force);
    }

    private bool IsStuckByFriction(Vector3 allomanticForce) {
        return Vector3.Project(allomanticForce, collisionNormal).sqrMagnitude > stuckThreshold;
    }

    /*
     * Moves the Coin such that it appears parented to the object it has collided with
     */
    private void UpdatePositionToParent() {
        transform.position = Matrix4x4.TRS(
            collidedCollider.position, collidedCollider.rotation, collidedCollider.lossyScale
        ).MultiplyPoint3x4(startChildPosition);

        transform.rotation = (collidedCollider.rotation * Quaternion.Inverse(startParentRotation)) * startChildRotation;
    }

    private void BeCaughtByAllomancer(Player player) {
        player.CoinHand.CatchCoin(this);
        HUD.TargetOverlayController.HardRefresh();
    }

    public override void StopBeingPullPushed() {
        IsStuck = false;
    }
}
