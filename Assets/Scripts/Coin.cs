using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Magnetic {

    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;
    private const float equalInMagnitudeConstant = .01f;
    private const float drag = 1.25f;

    //private float freeStaticFriction;
    //private float freeDynamicFriction;
    //private float pushFriction = 10f;
    //private PhysicMaterial material;
    private bool inContactWithPlayer = false;

    public override bool IsPerfectlyAnchored { // Only matters for Coins, which have so low masses that Unity thinks they have high velocities when pushed, even when anchored
        get {
            return Mathf.Abs(LastPosition.magnitude - transform.position.magnitude) < equalInMagnitudeConstant;
        }
    }

    private new void Awake() {
        base.Awake();
        //material = GetComponent<Collider>().material;
        //freeStaticFriction = material.staticFriction;
        //freeDynamicFriction = material.dynamicFriction;
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

    private void BeCaughtByAllomancer(Player player) {
        player.CoinHand.CatchCoin(this);
        HUD.TargetOverlayController.HardRefresh();
    }

    // Effectively caps the max velocity of the coin without affecting the ANF
    public override void AddForce(Vector3 netForce) {
        // Calculate drag from its new velocity
        Vector3 newNetForce = Vector3.ClampMagnitude(
            -(Vector3.Project(Rb.velocity, netForce.normalized) + (netForce / Mass * Time.fixedDeltaTime)) * drag, netForce.magnitude
        ) + netForce;

        LastExpectedAcceleration = newNetForce / Time.fixedDeltaTime;
        Rb.AddForce(newNetForce, ForceMode.Force);
    }

    //public override void StartBeingPullPushed(bool pulling) {
    //    if (!pulling) {
    //        material.staticFriction = pushFriction;
    //        material.dynamicFriction = pushFriction;
    //    }
    //}
    //public override void StopBeingPullPushed() {
    //    material.staticFriction = freeStaticFriction;
    //    material.dynamicFriction = freeDynamicFriction;
    //}

    // Makes coins sticky after colliding with something
    //private void OnCollisionEnter(Collision collision) {
    //    if (!collision.collider.CompareTag("Player")) {
    //        Rb.isKinematic = true;
    //    }
    //}
    //private void OnCollisionStay(Collision collision) {
    //    if(Allomancer.SteelPushing) {
    //        material.staticFriction = pushFriction;
    //        material.dynamicFriction = pushFriction;
    //    } else {

    //    }
    //}

    //public new Vector3 LastVelocity {
    //    get {
    //        return base.LastVelocity;
    //    }
    //    set {
    //        if (value.magnitude > maxSpeed)
    //            base.LastVelocity = value.normalized * maxSpeed;
    //        else
    //            base.LastVelocity = value;
    //    }
    //}

    /*
     * Prevents coins from flying away at extremely high, supersonic speeds. Unrealistic, but more like what appears in the books.
     */
    //private void FixedUpdate() {
    //    if(!Rb.IsSleeping() && Rb.velocity.magnitude > minSpeed) {

    //        //if (Rb.velocity.magnitude > maxSpeed) {
    //        //    Debug.Log(Rb.velocity.magnitude + " " + this.name + " This coin is travelling at supersonic speeds. Please stop doing that. " + Rb.velocity);
    //        //    Vector3 oldVelocity = Rb.velocity;
    //        //    Rb.velocity = Rb.velocity.normalized * maxSpeed;
    //        //    //Vector3 accelerationFromThis = Rb.velocity - oldVelocity; // CHANGE SIGN?
    //        //    //LastExpectedAcceleration += accelerationFromThis;
    //        //}

    //        //Debug.Log(Rb.velocity.magnitude + " " + this.name + " " + Rb.velocity);
    //        //Rb.AddForce(Rb.velocity * (maxSpeed - Rb.velocity.magnitude), ForceMode.Acceleration);

    //        //Vector3 airResistance = ((-Rb.velocity.normalized) * Mathf.Pow(Rb.velocity.magnitude, 2) * airResistanceFactor);
    //        //Debug.Log("Speed before: " + Rb.velocity.magnitude + " Air resistance: " + airResistance.magnitude);
    //        //Rb.AddForce(airResistance, ForceMode.Acceleration);
    //        //Debug.Log("Speed after: " + Rb.velocity.magnitude);

    //        //Rb.velocity = Vector3.ClampMagnitude(Rb.velocity, maxSpeed);
    //        //Debug.Log();
    //        //LastVelocity = Rb.velocity;
    //        //LastExpectedAcceleration += airResistance;
    //        //lastExpectedAcceleration = Vector3.zero;// + Physics.gravity;
    //    }
    //}

    //public override void AddForce(Vector3 force, ForceMode forceMode) {
    //    LastVelocity = Rb.velocity;
    //    // If the force would accelerate this Coin past its maxSpeed, don't accelerate it any more!
    //    if (forceMode == ForceMode.Force) {
    //        Vector3 newVelocity = Rb.velocity + force / Mass * Time.fixedDeltaTime;
    //        if ((newVelocity).magnitude > maxSpeed) {
    //            // Apply the force that would push it to the maxSpeed, but no further
    //            Vector3 newComponent = Vector3.ClampMagnitude(newVelocity, maxSpeed) - Rb.velocity;
    //            LastExpectedAcceleration = force / Mass;
    //            Rb.AddForce(newComponent, ForceMode.VelocityChange);
    //        } else {
    //            LastExpectedAcceleration = force / Mass;
    //            Rb.AddForce(force, ForceMode.Force);
    //        }
    //    } else {
    //        Debug.Log("hopefully you shouldn't be doing that");
    //        LastExpectedAcceleration = force;
    //        Rb.AddForce(force, forceMode);
    //    }
    //    Debug.Log("SPEED:" + Rb.velocity.magnitude);
    //}
}
