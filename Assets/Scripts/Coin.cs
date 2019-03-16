using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Magnetic {

    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;
    private const float drag = 1.25f;
    private const float stuckThreshold = 1000f; // Square magnitude of normal force necessary for friction
    private const float dotThreshold = -.3f; // friction threshold for dot product between force and normal vector
    private const float equalMagnitudeConstant = .01f;

    // Used for pseudo-parenting Coin when stuck to object it collides with
    private FixedJoint collisionJoint;
    private Transform collisionCollider;
    private Vector3 collisionNormal;

    private bool isStuck;

    public override bool IsPerfectlyAnchored {
        get {
            return Mathf.Abs((LastPosition - transform.position).sqrMagnitude) < equalMagnitudeConstant;
        }
    }

    public new void Clear() {
        base.Clear();
        UnStick();
    }

    private void OnCollisionEnter(Collision collision) {
        OnCollisionStay(collision);
    }

    /*
     * Simulates friction and makes the target stuck to the colliding surface
     * 
     * If stuck, the coin is jointed to the object it collided with.
     * collisionNormal needs to be constantly updated for each OnCollisionStay.
     */
    private void OnCollisionStay(Collision collision) {
        if (!collision.collider.CompareTag("Player")) {
            // Could be colliding with multiple objects.
            // Only care about the oldest one, for now.
            if (collisionCollider == collision.transform || collisionCollider == null) {
                collisionCollider = collision.transform;
                collisionNormal = collision.contacts[0].normal;
                if(isBeingPushPulled) {
                    if (!isStuck) { // Only updates on first frame of being stuck
                        isStuck = IsStuckByFriction(collision.impulse / Time.deltaTime, LastNetForceOnTarget);
                        if (isStuck) {
                            CreateJoint(collision.rigidbody);
                        }
                    }
                }
            } else {
                // A second, simultaneous collision has occured. Disregard.
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (collisionCollider == collision.transform) {
            UnStick();
        } else {
            // A different collision has ended. Disregard.
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("PlayerBody") &&
                    Keybinds.IronPulling() && !(Player.PlayerIronSteel.HasPullTarget &&  Player.PlayerIronSteel.PushTargets.IsTarget(this))) {
            //(Player.PlayerIronSteel.PullingOnPullTargets && Player.PlayerIronSteel.PullTargets.IsTarget(this) ||
            // Player.PlayerIronSteel.PullingOnPushTargets && Player.PlayerIronSteel.PushTargets.IsTarget(this) || 
            // !Player.PlayerIronSteel.HasIron && Keybinds.IronPulling())) {
            BeCaughtByAllomancer(other.transform.parent.GetComponent<Player>());
        }
    }

    public override void AddForce(Vector3 netForce, Vector3 allomanticForce) {
        // Calculate drag from its new velocity
        // Effectively caps the max velocity of the coin without affecting the ANF.
        //Vector3 newNetForce = netForce;
        Vector3 newNetForce = Vector3.ClampMagnitude(
            -(Vector3.Project(Rb.velocity, netForce.normalized) + (netForce / NetMass * Time.fixedDeltaTime)) * drag, netForce.magnitude
        ) + netForce;
        if (collisionCollider) { // If in a collision..
            if (isStuck) { // and is stuck...
                if (!IsStuckByFriction(netForce) && !IsStuckByFriction(allomanticForce)) { // ... but friction is too weak to keep the coin stuck in the target.
                                                                                           // Check both netForce and allomanticForce because the APB may decrease
                                                                                           // the netForce, even when the allomanticForce would be good enough for friction
                    UnStick();
                }
            } else { // and is not yet stuck from the previous pushes...
                isStuck = IsStuckByFriction(netForce) || IsStuckByFriction(allomanticForce);
                if (isStuck) { // but this push would stick the coin.
                    CreateJoint(collisionCollider.GetComponent<Rigidbody>());
                }
            }
        }

        LastExpectedAcceleration = newNetForce / NetMass; // LastPosition, LastVelocity are updated
        Rb.AddForce(newNetForce);
    }

    public override void StopBeingPullPushed() {
        base.StopBeingPullPushed();
        UnStick();
    }

    /*
     * Creates a joint between the Coin and anchor.
     */
    private void CreateJoint(Rigidbody anchor) {
        if (anchor) { // collided-with object has a rigidbody
            collisionJoint = gameObject.AddComponent<FixedJoint>();
            collisionJoint.connectedBody = anchor;
        } else { // collided-with object does not have a rigidbody, like a wall or ceiling
                 // simply freeze the coin where it is
            Rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    /*
     * Returns true if allomanticForce provides a strong enough friction against the collisionNormal
     */
    private bool IsStuckByFriction(Vector3 allomanticForce) {
        return IsStuckByFriction(allomanticForce, allomanticForce);
    }
    private bool IsStuckByFriction(Vector3 allomanticForce, Vector3 direction) {
        return Vector3.Dot(direction.normalized, collisionNormal) < dotThreshold && Vector3.Project(allomanticForce, collisionNormal).sqrMagnitude > stuckThreshold;
    }

    private void UnStick() {
        isStuck = false;
        collisionCollider = null;
        Destroy(collisionJoint);
        collisionNormal = Vector3.zero;
        Rb.constraints = RigidbodyConstraints.None;
    }

    private void BeCaughtByAllomancer(Player player) {
        player.CoinHand.CatchCoin(this);
        HUD.TargetOverlayController.HardRefresh();
    }
}
