using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Coins are metals with small enough masses to warrent special considerations for the physics engine.
 */
public class Coin : Magnetic {

    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;
    private const float drag = 1.25f;
    private const float highSpeedThresholdSqr = 100f; // what constitutes "high speed"
    private const float stuckThresholdSqr = 1000f; // Square magnitude of normal force necessary for friction
    private const float dotThreshold = -.3f; // friction threshold for dot product between force and normal vector
    private const float equalMagnitudeConstant = .01f;

    // Used for pseudo-parenting Coin when stuck to object it collides with
    private FixedJoint collisionJoint;
    private Transform collisionCollider;
    private Vector3 collisionNormal;

    /*
     * Stuck
     * 
     * A coin is stuck when it is Pushed into a collider.
     * Due to the shoddiness of Unity physics at high speeds, we need to override some things:
     *   When a coin Collides with a Wall:
     *      Raycast from LastPosition to current position. 
     *      Set the coin's position to hit.point.
     *      
     */
    private bool isStuck;

    public override bool IsPerfectlyAnchored {
        get {
            return Mathf.Abs((LastPosition - transform.position).sqrMagnitude) < equalMagnitudeConstant;
        }
    }

    public override bool IsBeingPushPulled {
        protected set {
            base.IsBeingPushPulled = value;
            if (!value)
                UnStick();
        }
    }

    public new void Clear() {
        base.Clear();
        UnStick();
    }

    /*
     * When first colliding with a Wall at high speed,
     */
    private void OnCollisionEnter(Collision collision) {
        if(Rb.velocity.sqrMagnitude < highSpeedThresholdSqr) {
            Debug.Log("High speed collision");
            if(Physics.Raycast(LastPosition, LastVelocity, out RaycastHit hit, LastVelocity.magnitude)) {

            } else {
                Debug.LogError("Somehow did not find coin collision");
            }
        }

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
            if(collisionCollider == null) {
                collisionCollider = collision.transform;
            }
            if (collisionCollider == collision.transform) {
                collisionNormal = collision.contacts[0].normal;
                if(IsBeingPushPulled) {
                    if (!isStuck) { // Only updates on first frame of being stuck. Stops being stuck when force becomes weak or stops being pushed.
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
    // Be caught by player if
    // - colliding with player
    // - player is pulling
    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("PlayerBody") &&
                    //Keybinds.IronPulling() && (!Player.PlayerIronSteel.HasPullTarget || Player.PlayerIronSteel.PullTargets.IsTarget(this))) {
                    Keybinds.IronPulling() && (LastWasPulled || !Player.PlayerIronSteel.PushTargets.IsTarget(this))) {
            BeCaughtByAllomancer(other.transform.parent.GetComponent<Player>());
        }
    }

    public override void AddForce(Vector3 netForce, Vector3 allomanticForce) {
        // Calculate drag from its new velocity
        // Effectively caps the max velocity of the coin without affecting the ANF.
        Vector3 newNetForce = netForce;
        //Vector3 newNetForce = Vector3.ClampMagnitude(
        //    -(Vector3.Project(Rb.velocity, netForce.normalized) + (netForce / NetMass * Time.fixedDeltaTime)) * drag, netForce.magnitude
        //) + netForce;
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
        Rb.AddForce(newNetForce * (1 / Time.timeScale));
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
        // true if the coin "digs" into the ground enough to stick, determined by:
        // the ANGLE of the collision is tall enough (dot product < threshold)
        // the DOWNWARD FORCE of the collision is strong enough (projection > threshold)
        return Vector3.Dot(direction.normalized, collisionNormal) < dotThreshold && Vector3.Project(allomanticForce, collisionNormal).sqrMagnitude > stuckThresholdSqr;
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
