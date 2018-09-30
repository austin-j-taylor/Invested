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
    //private Vector3 collisionLocalPosition;
    private Transform collidedCollider = null;

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
            if (value)
                Rb.velocity = Vector3.zero;
        }
    }


    Matrix4x4 parentMatrix;
    
    Quaternion startParentRotation;

    Vector3 startChildPosition;
    Quaternion startChildRotation;
    

    private void FixedUpdate() {
        UpdatePositionToParent();
    }

    private void UpdatePositionToParent() {
        if (IsStuck) { // Coin is stuck to something; pseudo-parent it.
            parentMatrix = Matrix4x4.TRS(collidedCollider.position, collidedCollider.rotation, collidedCollider.lossyScale);
            //transform.position = startChildPosition;
            transform.position = parentMatrix.MultiplyPoint3x4(startChildPosition);
            transform.rotation = (collidedCollider.rotation * Quaternion.Inverse(startParentRotation)) * startChildRotation;
        }
    }

    public new void Clear() {
        base.Clear();
        collidedCollider = null;
        IsStuck = false;
        lastWasStuck = false;
    }
    /*
     * Entered collision. Initialize pseudo-parenting.
     */
    private void OnCollisionEnter(Collision collision) {
        OnCollisionStay(collision);
    }

    // Only called when not kinematic i.e. when sliding along the ground
    private void OnCollisionStay(Collision collision) {
        collidedCollider = collision.transform;

        if (!collision.collider.CompareTag("Player")) {
            if (Allomancer && Allomancer.SteelPushing && !LastWasPulled) {
                CheckStuck(collision);
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        //collidedCollider = null;
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
        
        if (Allomancer && Allomancer.SteelPushing) {
            IsStuck = Vector3.Project(newNetForce, stuckNormal).sqrMagnitude > stuckThreshold;
        }

        LastExpectedAcceleration = newNetForce / NetMass; // LastPosition, LastVelocity are updated
        Rb.AddForce(newNetForce, ForceMode.Force);
    }

    /*
     * Simulates friction and makes the target stuck to the colliding surface
     * 
     * If stuck, the coin is pseudo-parented to the object it collided with.
     * stuckNormal needs to be constantly updated for each OnCollisionStay
     */
    private void CheckStuck(Collision collision) {
        Vector3 thisNormal = collision.contacts[0].normal;
        IsStuck = Vector3.Project(collision.impulse / Time.deltaTime, thisNormal).sqrMagnitude > stuckThreshold;
        if (IsStuck) {
            if (!lastWasStuck) {
                // First frame of being stuck.

                startParentRotation = collidedCollider.rotation;
                startChildPosition = transform.position;
                startChildRotation = transform.rotation;

                // Keeps child position from being modified at the start by the parent's initial transform
                Vector3 num = Quaternion.Inverse(collidedCollider.rotation) * (startChildPosition - collidedCollider.position);
                Vector3 den = collidedCollider.lossyScale;
                startChildPosition = new Vector3(num.x / den.x, num.y / den.y, num.z / den.z);
            }
            stuckNormal = thisNormal;
            //transform.position = collisionLocalPosition;
            UpdatePositionToParent();
            Rb.velocity = Vector3.zero;
        } else {
            //collidedCollider = null;
        }

        lastWasStuck = IsStuck;
    }

    private void BeCaughtByAllomancer(Player player) {
        player.CoinHand.CatchCoin(this);
        HUD.TargetOverlayController.HardRefresh();
    }

    public override void StopBeingPullPushed() {
        IsStuck = false;
        lastWasStuck = false;
    }
}
