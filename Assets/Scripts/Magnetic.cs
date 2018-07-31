using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnetic : MonoBehaviour {

    private const float equalMagnitudeConstant = .01f;

    private bool lastWasPulled;
    [SerializeField]
    private float mass;
    public AllomanticIronSteel Allomancer {
        get;
        set;
    }

    // Used for calculating Allomantic Normal Force
    public Vector3 Velocity { get
            {
            if (IsStatic)
                return Vector3.zero;
            else
                return Rb.velocity;
        }
    }
    public Vector3 LastPosition { get; set; }
    public Vector3 LastVelocity { get; set; }
    public virtual Vector3 LastExpectedAcceleration { get; set; }

    public Vector3 LastAllomanticForce { get; set; }
    public Vector3 LastMaximumAllomanticForce { get; set; }
    public Vector3 LastAllomanticNormalForceFromAllomancer { get; set; }
    public Vector3 LastAllomanticNormalForceFromTarget { get; set; }

    public float LightSaberFactor { get; set; }

    public bool LastWasPulled {
        get {
            return lastWasPulled;
        }
        set {
            if(value != lastWasPulled) { // swapped between pulling and pushing, clear certain values
                SoftClear();
                lastWasPulled = value;
            }
        }
    }

    public Vector3 LastNetAllomanticForceOnAllomancer {
        get {
            //if (LastWasPulled) {
            return LastAllomanticForce + LastAllomanticNormalForceFromTarget;
            //} else {
            //    return -LastAllomanticForce + LastAllomanticNormalForceFromTarget;
            //}
        }
    }

    public Vector3 LastNetAllomanticForceOnTarget {
        get {
            //if (LastWasPulled) {
            return -LastAllomanticForce + LastAllomanticNormalForceFromAllomancer;
            //} else {
            //    return LastAllomanticForce + LastAllomanticNormalForceFromAllomancer;
            //}
        }
    }

    // If the object has a Rigidbody, this is the real centerOfMass. Otherwise, it is just the transform local position.
    public Vector3 LocalCenterOfMass { get; private set; }
    // Global center of mass
    public Vector3 CenterOfMass {
        get {
            return transform.TransformPoint(LocalCenterOfMass);
        }
    }
    public float Mass {
        get {
            return mass;
        }
    }
    public bool InRange {
        get;
        set;
    }
    public bool IsPerfectlyAnchored {
        get {
            return Mathf.Abs(LastPosition.magnitude - transform.position.magnitude) < equalMagnitudeConstant;
        }
    }
    public Rigidbody Rb { get; private set; }
    public bool IsStatic { get; set; }


    // Use this for initialization
    void Awake () {
        Allomancer = null;// = GameObject.FindGameObjectWithTag("Player").GetComponent<AllomanticIronSteel>();
        Rb = GetComponent<Rigidbody>();
        LastPosition = Vector3.zero;
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastMaximumAllomanticForce = Vector3.zero;
        LastAllomanticNormalForceFromAllomancer = Vector3.zero;
        LastAllomanticNormalForceFromTarget = Vector3.zero;
        LightSaberFactor = 1;
        lastWasPulled = false;
        IsStatic = Rb == null;
        if(!IsStatic) {
            mass = Rb.mass;
            LocalCenterOfMass = Rb.centerOfMass;
        } else {
            LocalCenterOfMass = Vector3.zero;
        }
        InRange = false;
    }

    public void Clear() {
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastMaximumAllomanticForce = Vector3.zero;
        LastAllomanticNormalForceFromAllomancer = Vector3.zero;
        LastAllomanticNormalForceFromTarget = Vector3.zero;
        Allomancer = null;
        LightSaberFactor = 1;
        InRange = false;
    }

    public void SoftClear() {
        LastExpectedAcceleration = Vector3.zero;
        LastAllomanticNormalForceFromAllomancer = Vector3.zero;
        LastAllomanticNormalForceFromTarget = Vector3.zero;
        LastMaximumAllomanticForce = Vector3.zero;
        //LightSaberFactor = 1;
    }

    public virtual void AddForce(Vector3 force, ForceMode forceMode) {
        if (!IsStatic) {
            LastExpectedAcceleration = force / mass;

            Rb.AddForce(force, forceMode);
        }
    }

    //private void OnCollisionStay(Collision collision) {
    //    if (allomancer != null) {
    //        Vector3 collisionWithoutGravity = (collision.impulse / Time.fixedDeltaTime + Physics.gravity * mass);
    //        Vector3 forceOnPlayer = collisionWithoutGravity;// / mass * allomancer.Mass;
    //        Debug.Log("forceOnPlayer: " + forceOnPlayer + " raw: " + collision.impulse / Time.fixedDeltaTime + " Gravity: " + Physics.gravity * mass + " delta time: " + Time.fixedDeltaTime);
    //        forceFromCollision = forceOnPlayer;
    //    }
    //}
    //private void OnCollisionExit(Collision collision) {
    //    forceFromCollision = Vector3.zero;
    //}
    //public Vector3 forceFromCollision = Vector3.zero;

    //    private void OnCollisionStay(Collision collision) {
    //        frameCounter++;
    //        if (Allomancer != null) {
    //            if (frameCounter % 2 == 0) {
    //                Vector3 collisionWithoutGravity = (collision.impulse / Time.fixedDeltaTime + Physics.gravity * mass);
    //                Vector3 forceOnPlayer = collisionWithoutGravity;// / mass * allomancer.Mass;
    //                Vector3 raw = collision.impulse / Time.fixedDeltaTime;
    //                Debug.Log("EVE forceOnPlayer: " + forceOnPlayer + " raw: " + collision.impulse / Time.fixedDeltaTime + " Gravity: " + Physics.gravity * mass + " delta time: " + Time.fixedDeltaTime);

    //                forceFromCollisionTotal = collisionWithoutGravity + forceFromCollisionOdd;

    //                Debug.Log("Total Force: " + forceFromCollisionTotal);
    //            } else {
    //                Vector3 collisionWithoutGravity = (collision.impulse / Time.fixedDeltaTime + Physics.gravity * mass);
    //                Vector3 forceOnPlayer = collisionWithoutGravity;// / mass * allomancer.Mass;
    //                Vector3 raw = collision.impulse / Time.fixedDeltaTime;
    //                Debug.Log("ODD forceOnPlayer: " + forceOnPlayer + " raw: " + collision.impulse / Time.fixedDeltaTime + " Gravity: " + Physics.gravity * mass + " delta time: " + Time.fixedDeltaTime);
    //                forceFromCollisionOdd = collisionWithoutGravity;
    //            }
    //        }
    //    }

    //    private void OnCollisionExit(Collision collision) {
    //    forceFromCollisionOdd = Vector3.zero;
    //    forceFromCollisionTotal = Vector3.zero;
    //}
    //    public Vector3 forceFromCollisionOdd = Vector3.zero;
    //    public Vector3 forceFromCollisionTotal = Vector3.zero;
    //    private int frameCounter = 0;
}
