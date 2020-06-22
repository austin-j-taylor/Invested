using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Coins are metals with small enough masses to warrent special considerations for the physics engine.
/// In general, coins may get Stuck to other object when Pushed against them, as long as the simulated friction is strong enough.
/// </summary>
public class Coin : Magnetic {

    #region constants
    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;
    private const float drag = 1.3f;//1.25f;
    private const float highSpeedThreshold = 1; // what constitutes "high speed": meters per frame
    private const float stuckThresholdSqr = 1000f; // Square magnitude of normal force necessary for friction
    private const float dotThreshold = -.3f; // friction threshold for dot product between force and normal vector
    private const float equalMagnitudeConstant = .01f;
    #endregion

    // Used for pseudo-parenting Coin when stuck to object it collides with
    private FixedJoint collisionJoint;
    private Transform collisionCollider;
    private Vector3 collisionNormal;

    private bool isStuck;

    public override bool IsPerfectlyAnchored => Mathf.Abs((LastPosition - transform.position).sqrMagnitude) < equalMagnitudeConstant;

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

    #region collision
    /// <summary>
    /// When first colliding with a Wall at high speed, consider sticking to it.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision) {
        Vector3 velocityThisFrame = LastVelocity * Time.fixedDeltaTime;
        //Debug.DrawLine(LastPosition, LastPosition + (LastVelocity * Time.fixedDeltaTime), Color.green);
        //Debug.DrawLine(LastPosition - (velocityThisFrame), LastPosition, Color.green);
        if (velocityThisFrame.magnitude > highSpeedThreshold) {
            // Need to "jump back a frame" for the raycast
            if (Physics.Raycast(LastPosition - (velocityThisFrame), LastVelocity, out RaycastHit hit, velocityThisFrame.magnitude)) {
                //Debug.Log("High speed collision" + velocityThisFrame.magnitude);
                //Debug.DrawLine(LastPosition - (velocityThisFrame), hit.point, Color.red);
                transform.position = hit.point;
            }
        }
        //    } else {
        //        Debug.LogError("Did not find coin collision" + LastVelocity.magnitude);
        //    }
        //} else {
        //    Debug.Log("Slow: " + LastVelocity.magnitude);
        //}

        OnCollisionStay(collision);
    }

    /// <summary>
    /// Simulates friction and makes the target stuck to the colliding surface.
    /// If stuck, the coin is jointed to the object it collided with.
    /// collisionNormal needs to be constantly updated for each OnCollisionStay.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay(Collision collision) {
        if (!collision.collider.CompareTag("Player")) {
            // Could be colliding with multiple objects.
            // Only care about the oldest one, for now.
            if (collisionCollider == null) {
                collisionCollider = collision.transform;
            }
            if (collisionCollider == collision.transform) {
                collisionNormal = collision.contacts[0].normal;
                if (IsBeingPushPulled) {
                    if (!isStuck) { // Only updates on first frame of being stuck. Stops being stuck when force becomes weak or stops being pushed.
                        isStuck = IsStuckByFriction(collision.impulse / Time.deltaTime, LastNetForceOnTarget);
                        //Debug.Log("Now stuck");
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

    /// <summary>
    /// This coin should be caught by the Player if they are Pulling close enough to it.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("PlayerBody") &&
                    //Keybinds.IronPulling() && (!Player.PlayerIronSteel.HasPullTarget || Player.PlayerIronSteel.PullTargets.IsTarget(this))) {
                    (Keybinds.IronPulling() || Player.PlayerIronSteel.BubbleIsOpen && Player.PlayerIronSteel.BubbleMetalStatus == AllomanticIronSteel.iron)) {
            // Make sure there's not a wall between the coin and the player
            Vector3 direction = other.transform.position - transform.position;
            if (!Physics.Raycast(transform.position, direction, direction.magnitude, GameManager.Layer_IgnoreCamera))
                BeCaughtByAllomancer(other.transform.parent.GetComponent<Player>());
        }
    }
    #endregion

    /// <summary>
    /// While something is Pushing on the coin, check if that Push provides enough friction to Stick it.
    /// </summary>
    /// <param name="netForce">the Net Allomantic Force</param>
    /// <param name="allomanticForce">just the Allomantic Force component</param>
    public override void AddForce(Vector3 netForce, Vector3 allomanticForce) {
        //Debug.DrawLine(LastPosition, LastPosition + (LastVelocity * Time.fixedDeltaTime), Color.blue);
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
                    // (for example, EwV will make the force -> 0 when the coin is speedy)
                    UnStick();
                }
            } else { // and is not yet stuck from the previous pushes...
                isStuck = IsStuckByFriction(netForce) || IsStuckByFriction(allomanticForce);
                if (isStuck) { // but this push would stick the coin.
                    CreateJoint(collisionCollider.GetComponent<Rigidbody>());
                }
            }
        }
        base.AddForce(newNetForce, allomanticForce);
        //LastExpectedAcceleration = newNetForce / NetMass; // LastPosition, LastVelocity are updated
        //Rb.AddForce(newNetForce * (1 / Time.timeScale));

        //thisFrameIsBeingPushPulled = true;
    }

    #region joints
    /// <summary>
    /// Creates a joint between the Coin and another object.
    /// </summary>
    /// <param name="anchor">the object to Stick the coin to</param>
    private void CreateJoint(Rigidbody anchor) {
        if (anchor) { // collided-with object has a rigidbody
            collisionJoint = gameObject.AddComponent<FixedJoint>();
            collisionJoint.connectedBody = anchor;
        } else { // collided-with object does not have a rigidbody, like a wall or ceiling
                 // simply freeze the coin where it is
            Rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    /// <summary>
    /// Check if if allomanticForce provides a strong enough friction against the collisionNormal
    /// </summary>
    /// <param name="allomanticForce">the acting force</param>
    /// <returns>the friction is strong enough to keep the coin Stuck</returns>
    private bool IsStuckByFriction(Vector3 allomanticForce) => IsStuckByFriction(allomanticForce, allomanticForce);
    /// <summary>
    /// Check if if allomanticForce provides a strong enough friction against the collisionNormal
    /// </summary>
    /// <param name="allomanticForce">the acting force</param>
    /// <param name="direction">the direction of the acting force, different than the direction of the first parameter</param>
    /// <returns></returns>
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
    #endregion

    private void BeCaughtByAllomancer(Player player) {
        player.CoinHand.CatchCoin(this);
        HUD.TargetOverlayController.HardRefresh();
    }
}
