using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Magnetic {

    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;
    private const float equalInMagnitudeConstant = .01f;
    private const float drag = 1.25f;
    private const float stuckThreshold = 1f;
    
    private bool inContactWithPlayer = false;
    private bool isStuck = false;

    public override bool IsPerfectlyAnchored { // Only matters for Coins, which have so low masses that Unity thinks they have high velocities when pushed, even when anchored
        get {
            return Mathf.Abs(LastPosition.sqrMagnitude - transform.position.sqrMagnitude) < equalInMagnitudeConstant;
        }
    }

    private bool IsStuck {
        get {
            return isStuck;
        }
        set {
            isStuck = value;
            Rb.isKinematic = value;
            Rb.velocity = Vector3.zero;
        }
    }

    public new void Clear() {
        base.Clear();
        IsStuck = false;
    }

    // Makes coins sticky upon colliding with something
    private void OnCollisionEnter(Collision collision) {
        if (!collision.collider.CompareTag("Player")) {
            if(Allomancer && Allomancer.SteelPushing && !LastWasPulled)
                AddFriction(collision);
        }
    }

    // Only called when not kinematic i.e. when sliding along the ground
    private void OnCollisionStay(Collision collision) {
        if (!collision.collider.CompareTag("Player")) {
            if (Allomancer && Allomancer.SteelPushing && !LastWasPulled) {
                if (!IsStuck) {
                    AddFriction(collision);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            inContactWithPlayer = true;
        }
    }

    private void OnTriggerStay(Collider other) {
        if (inContactWithPlayer && Keybinds.IronPulling() && other.CompareTag("Player")) {
            BeCaughtByAllomancer(other.GetComponent<Player>());
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other.CompareTag("Player")) {
            inContactWithPlayer = false;
        }
    }

    // Effectively caps the max velocity of the coin without affecting the ANF
    public override void AddForce(Vector3 netForce) {
        // Calculate drag from its new velocity
        Vector3 newNetForce = Vector3.ClampMagnitude(
            -(Vector3.Project(Rb.velocity, netForce.normalized) + (netForce / NetMass * Time.fixedDeltaTime)) * drag, netForce.magnitude
        ) + netForce;

        if (IsStuck) {
            if (Allomancer.SteelPushing) {
                if (newNetForce.sqrMagnitude < stuckThreshold) {
                    IsStuck = false;
                }
            }
        }

        LastExpectedAcceleration = newNetForce / NetMass;
        Rb.AddForce(newNetForce, ForceMode.Force);
    }

    private void AddFriction(Collision collision) {
        IsStuck = collision.impulse.sqrMagnitude > stuckThreshold;
        if (IsPerfectlyAnchored || IsStuck) {
            Rb.velocity = Vector3.zero;
        }
    }

    private void BeCaughtByAllomancer(Player player) {
        player.CoinHand.CatchCoin(this);
        HUD.TargetOverlayController.HardRefresh();
    }

    public override void StopBeingPullPushed() {
        if(IsStuck)
            IsStuck = false;
    }
}
