using cakeslice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

public class Magnetic : MonoBehaviour {

    private const float metalLinesLerpConstant = .30f;

    [SerializeField]
    private float mass;
    
    private bool lastWasPulled;
    private Outline highlightedTargetOutline;
    private VolumetricLineBehavior blueLine;
    private float lightSaberFactor;

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
    public Vector3 LastExpectedAcceleration { get; set; }

    // These keep track of each Magnetic's participation to the net force on the Allomancer
    public Vector3 LastAllomanticForce { get; set; }
    public Vector3 LastAllomanticNormalForceFromAllomancer { get; set; }
    public Vector3 LastAllomanticNormalForceFromTarget { get; set; }


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
            return LastAllomanticForce + LastAllomanticNormalForceFromTarget;
        }
    }

    public Vector3 LastNetAllomanticForceOnTarget {
        get {
            return -LastAllomanticForce + LastAllomanticNormalForceFromAllomancer;
        }
    }

    public Vector3 LastAllomanticForceOnAllomancer {
        get {
            return LastAllomanticForce;
        }
    }

    public Vector3 LastAllomanticForceOnTarget {
        get {
            return -LastAllomanticForce;
        }
    }

    // If the object has a Rigidbody, this is the real centerOfMass. Otherwise, it is just the transform local position.
    public Vector3 LocalCenterOfMass { get; private set; }
    public Collider ColliderBody { get; private set; }
    // If this Magnetic is at the center of the screen, highlighted, ready to be targeted.
    public bool IsHighlighted { get; private set; }
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
    public virtual bool IsPerfectlyAnchored { // Only matters for Coins, which have so low masses that Unity thinks they have high velocities when pushed, even when anchored
        get {
            //return Mathf.Abs(LastPosition.magnitude - transform.position.magnitude) < equalMagnitudeConstant;
            return false;
        }
    }
    public Rigidbody Rb { get; private set; }
    public float Charge;// { get; private set; }
    public bool IsStatic { get; set; }


    // Use this for initialization
    void Awake () {
        Allomancer = null;
        highlightedTargetOutline = gameObject.AddComponent<Outline>();
        blueLine = Instantiate(GameManager.MetalLineTemplate);
        Rb = GetComponent<Rigidbody>();
        ColliderBody = GetComponent<Collider>();
        LastPosition = Vector3.zero;
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastAllomanticNormalForceFromAllomancer = Vector3.zero;
        LastAllomanticNormalForceFromTarget = Vector3.zero;
        lightSaberFactor = 1;
        lastWasPulled = false;
        IsHighlighted = false;
        IsStatic = Rb == null;
        if(!IsStatic) {
            mass = Rb.mass;
            LocalCenterOfMass = Rb.centerOfMass;
        } else {
            LocalCenterOfMass = Vector3.zero;
        }
        Charge = Mathf.Pow(mass, AllomanticIronSteel.chargePower);
        InRange = false;
    }
    private void Start() {
        GameManager.AddMagnetic(this);
    }
    // If the Magnetic is untargeted
    public void Clear() {
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastAllomanticNormalForceFromAllomancer = Vector3.zero;
        LastAllomanticNormalForceFromTarget = Vector3.zero;
        Allomancer = null;
        lightSaberFactor = 1;
        InRange = false;
        blueLine.GetComponent<MeshRenderer>().enabled = false;
        RemoveTargetGlow();
    }

    public void SoftClear() {
        //LastExpectedAcceleration = Vector3.zero;
        //LastAllomanticForce = Vector3.zero;
        //LastAllomanticNormalForceFromAllomancer = Vector3.zero;
        //LastAllomanticNormalForceFromTarget = Vector3.zero;
        //LastVelocity = Vector3.zero;
        //LightSaberFactor = 1;
    }

    private void OnDestroy() {
        GameManager.MagneticsInScene.Remove(this);
    }

    public virtual void AddForce(Vector3 netForce, ForceMode forceMode) {
        if (!IsStatic) {
            Rb.AddForce(netForce, forceMode);
        }
    }

    public void AddTargetGlow() {
        highlightedTargetOutline.Enable();
    }

    public void RemoveTargetGlow() {
        highlightedTargetOutline.Disable();
    }

    public void SetBlueLine(Vector3 endPos, float width, float lsf, Color color) {
        blueLine.GetComponent<MeshRenderer>().enabled = true;
        blueLine.StartPos = CenterOfMass;
        blueLine.EndPos = endPos;
        blueLine.LineWidth = width;
        lightSaberFactor = Mathf.Lerp(lightSaberFactor, lsf, metalLinesLerpConstant);
        blueLine.LightSaberFactor = lightSaberFactor;
        blueLine.LineColor = color;
    }

    public void DisableBlueLine() {
        blueLine.GetComponent<MeshRenderer>().enabled = false;
    }

    //public void AddTargetGlow() {
    //    Renderer targetRenderer;
    //    Material[] mats;
    //    Material[] temp;
    //    // add glowing of new pullTarget
    //    targetRenderer = GetComponent<Renderer>();
    //    temp = targetRenderer.materials;
    //    mats = new Material[temp.Length + 1];
    //    for (int i = 0; i < temp.Length; i++) {
    //        mats[i] = temp[i];
    //    }

    //    mats[mats.Length - 1] = GameManager.Instance.TargetHighlightMaterial;
    //    targetRenderer.materials = mats;
    //    IsHighlighted = true;
    //}

    //public void RemoveTargetGlow() {
    //    if (IsHighlighted) {
    //        Renderer targetRenderer;
    //        Material[] mats;
    //        Material[] temp;

    //        // remove glowing of old target
    //        targetRenderer = GetComponent<Renderer>();
    //        temp = targetRenderer.materials;
    //        if (temp.Length > 1) {
    //            mats = new Material[temp.Length - 1];
    //            mats[0] = temp[0];
    //            for (int i = 1; i < mats.Length; i++) {
    //                if (temp[i].name == "targetHighlightMaterial (Instance)") {
    //                    for (int j = i; j < mats.Length; j++) {
    //                        mats[j] = temp[j + 1];
    //                    }
    //                    break;
    //                } else {
    //                    mats[i] = temp[i];
    //                }
    //            }
    //            targetRenderer.materials = mats;
    //            IsHighlighted = false;
    //            HUD.TargetOverlayController.RemoveHighlightedTargetLabel();

    //        }
    //    }
    //}

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
