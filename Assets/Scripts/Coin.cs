using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Magnetic {

    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;
    private const float equalInMagnitudeConstant = .01f;
    private bool inContactWithPlayer = false;

    public override bool IsPerfectlyAnchored { // Only matters for Coins, which have so low masses that Unity thinks they have high velocities when pushed, even when anchored
        get {
            return Mathf.Abs(LastPosition.magnitude - transform.position.magnitude) < equalInMagnitudeConstant;
        }
    }

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
    //private void OnCollisionEnter(Collision collision) {
    //    if (!collision.collider.CompareTag("Player")) {
    //        Rb.isKinematic = true;
    //    }
    //}

    // Effectively caps the max velocity of the coin without affecting the ANF
    public override void AddForce(Vector3 netForce, ForceMode forceMode) {
        // If the force would accelerate this Coin past its maxSpeed, don't accelerate it any more!
        if (forceMode == ForceMode.Force) {
            Vector3 newVelocity = Rb.velocity + netForce / Mass * Time.fixedDeltaTime;
            if ((newVelocity).magnitude > maxSpeed) {
                // Apply the force that would push it to the maxSpeed, but no further
                Vector3 changeInVelocityThatWillBringMeToTopSpeed = Vector3.ClampMagnitude(newVelocity, maxSpeed) - Rb.velocity;

                LastExpectedAcceleration = changeInVelocityThatWillBringMeToTopSpeed / Time.fixedDeltaTime;
                //LastExpectedAcceleration = force / Mass;
                Rb.AddForce(changeInVelocityThatWillBringMeToTopSpeed, ForceMode.VelocityChange);
            } else {
                //LastExpectedAcceleration = allomanticForce / Mass;
                Rb.AddForce(netForce, ForceMode.Force);
            }
        } else {
            Debug.Log("You shouldn't be adding a force to a Coin with anything other than ForceMode.Force");
        }
    }

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
