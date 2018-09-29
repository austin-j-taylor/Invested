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

    private bool isStuck = false;
    private bool lastWasStuck = false;
    private Vector3 stuckNormal;
    private Vector3 collisionPosition;

    public override bool IsPerfectlyAnchored { // Only matters for Coins, which have so low masses that Unity thinks they have high velocities when pushed, even when anchored
        get {
            return Mathf.Abs(LastPosition.sqrMagnitude - transform.position.sqrMagnitude) < equalInMagnitudeConstant;
        }
    }

    private bool isInCollider = false;

    private bool IsStuck {
        get {
            return isStuck;
        }
        set {
            isStuck = value;
            if (value)
                Rb.velocity = Vector3.zero;
        }
    }
    
    public new void Clear() {
        base.Clear();
        isInCollider = false;
        IsStuck = false;
        lastWasStuck = false;
    }

    private void OnCollisionEnter(Collision collision) {
        OnCollisionStay(collision);
    }

    // Only called when not kinematic i.e. when sliding along the ground
    private void OnCollisionStay(Collision collision) {
        if (!collision.collider.CompareTag("Player")) {
            if (Allomancer && Allomancer.SteelPushing && !LastWasPulled) {
                CheckStuck(collision);
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        isInCollider = false;
        IsStuck = false;
    }

    //private void OnTriggerEnter(Collider other) {
    //    if (Keybinds.IronPulling() && other.CompareTag("Player")) {
    //        BeCaughtByAllomancer(other.GetComponent<Player>());
    //    }
    //}

    private void OnTriggerStay(Collider other) {
        if (Keybinds.IronPulling() && other.CompareTag("Player")) {
            BeCaughtByAllomancer(other.GetComponent<Player>());
        }
    }

    // Effectively caps the max velocity of the coin without affecting the ANF
    public override void AddForce(Vector3 netForce) {
        // Calculate drag from its new velocity
        Vector3 newNetForce = Vector3.ClampMagnitude(
            -(Vector3.Project(Rb.velocity, netForce.normalized) + (netForce / NetMass * Time.fixedDeltaTime)) * drag, netForce.magnitude
        ) + netForce;
        
        if (!isInCollider)
            IsStuck = false;
        isInCollider = false;

        if (Allomancer && Allomancer.SteelPushing) {
            IsStuck = Vector3.Project(newNetForce, stuckNormal).sqrMagnitude > stuckThreshold;
        }

        LastExpectedAcceleration = newNetForce / NetMass; // LastPosition, LastVelocity are updated
        Rb.AddForce(newNetForce, ForceMode.Force);
    }

    /*
     * Simulates friction and makes the target stuck to the colliding surface
     */
    private void CheckStuck(Collision collision) {
        Vector3 thisNormal = collision.contacts[0].normal;
        IsStuck = Vector3.Project(collision.impulse / Time.deltaTime, thisNormal).sqrMagnitude > stuckThreshold;
        isInCollider = IsStuck;
        if (IsStuck) {
            if (!lastWasStuck) {
                //    if (Allomancer) Debug.Log(isInCollider);
                collisionPosition = collision.contacts[0].point;
                stuckNormal = thisNormal;
                transform.position = collisionPosition;
            }
            if (collision.rigidbody == null) {
                transform.position = collisionPosition;
            } else {
                collisionPosition = collision.contacts[0].point;
                stuckNormal = thisNormal;
                transform.position = collisionPosition;
            }
            //Rb.velocity = Vector3.zero;
        }
        lastWasStuck = IsStuck;
    }

    private void BeCaughtByAllomancer(Player player) {
        player.CoinHand.CatchCoin(this);
        HUD.TargetOverlayController.HardRefresh();
    }

    public override void StopBeingPullPushed() {
        isInCollider = false;
        IsStuck = false;
        lastWasStuck = false;
    }
}
