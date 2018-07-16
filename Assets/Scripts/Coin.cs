using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : Magnetic {

    private const float airResistanceFactor = .1f;
    private const float minSpeed = 5f;
    private const float maxSpeed = 120f;

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
    public override Vector3 LastExpectedAcceleration {
        get {
            return base.LastExpectedAcceleration;
        }
        set {
            //if (value.magnitude > maxAcceleration) {
            //    Debug.Log(" ... " + value.magnitude);
            //    //base.LastExpectedAcceleration = value.normalized * maxAcceleration;
            //    base.LastExpectedAcceleration = value;
            //    Debug.Log(" new expected acceleration: " + base.LastExpectedAcceleration.magnitude);
            //} else {
            //    Debug.Log(value.magnitude);
                base.LastExpectedAcceleration = value;
            //}
        }
    }

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
            Player player = other.GetComponent<Player>();
            //if (player.IronSteel.IronPulling)
            if (Keybinds.IronPulling())
                player.Pouch.AddCoin(this);
        }
    }
    //private void OnCollisionEnter(Collision collision) {
    //    if (!collision.collider.CompareTag("Player")) {
    //        Rb.isKinematic = true;
    //    }
    //}

    public override void AddForce(Vector3 force, ForceMode forceMode) {
        // If the force would accelerate this Coin past its maxSpeed, don't accelerate it any more!
        if (forceMode == ForceMode.Force) {
            Vector3 newVelocity = Rb.velocity + force / Mass * Time.fixedDeltaTime;
            if ((newVelocity).magnitude > maxSpeed) {
                // Apply the force that would push it to the maxSpeed, but no further
                Vector3 changeInVelocityThatWillBringMeToTopSpeed = Vector3.ClampMagnitude(newVelocity, maxSpeed) - Rb.velocity;

                LastExpectedAcceleration = changeInVelocityThatWillBringMeToTopSpeed / Time.fixedDeltaTime;
                Rb.AddForce(changeInVelocityThatWillBringMeToTopSpeed, ForceMode.VelocityChange);
            } else {
                LastExpectedAcceleration = force / Mass;
                Rb.AddForce(force, ForceMode.Force);
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
