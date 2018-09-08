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

    public AllomanticIronSteel Allomancer { get; set; }
    protected Rigidbody Rb { get; set; }

    // Used for calculating Allomantic Normal Force
    public Vector3 Velocity { get
            {
            if (IsStatic)
                return Vector3.zero;
            else
                return Rb.velocity;
        }
    }
    public Vector3 LastPosition { get; private set; }
    public Vector3 LastVelocity { get; private set; }
    public Vector3 LastExpectedAcceleration {
        get {
            return lastExpectedAcceleration;
        }
        protected set {
            lastExpectedAcceleration = value;
            LastPosition = transform.position;
            LastVelocity = Velocity;
        }
    }
    private Vector3 lastExpectedAcceleration;

    // These keep track of each Magnetic's participation to the net force on the Allomancer
    public Vector3 LastAllomanticForce { get; set; }
    public Vector3 LastAllomanticNormalForceFromAllomancer { get; set; }
    public Vector3 LastAllomanticNormalForceFromTarget { get; set; }
    
    public bool InRange { get; set; }
    public bool IsStatic { get; set; }

    public bool LastWasPulled {
        get {
            return lastWasPulled;
        }
        set {
            if(value != lastWasPulled) { // swapped between pulling and pushing, clear certain values
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
    public float Charge { get; private set; }
    // If this Magnetic is at the center of the screen, highlighted, ready to be targeted.
    public bool IsHighlighted { get; private set; }
    public float Mass { get { return mass; } }
    // Global center of mass
    public Vector3 CenterOfMass {
        get {
            return transform.TransformPoint(LocalCenterOfMass);
        }
    }
    public virtual bool IsPerfectlyAnchored { // Only matters for Coins, which have so low masses that Unity thinks they have high velocities when pushed, even when anchored
        get {
            return false;
        }
    }
    
    private void Awake () {
        Allomancer = null;
        highlightedTargetOutline = gameObject.AddComponent<Outline>();
        blueLine = Instantiate(GameManager.MetalLineTemplate);
        Rb = GetComponent<Rigidbody>();
        ColliderBody = GetComponent<Collider>();
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
        LastPosition = Vector3.zero;
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastAllomanticNormalForceFromAllomancer = Vector3.zero;
        LastAllomanticNormalForceFromTarget = Vector3.zero;
        InRange = false;
    }

    private void Start() {
        GameManager.AddMagnetic(this);
    }

    // If the Magnetic is untargeted
    public void Clear() {
        StopBeingPullPushed();
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

    private void OnDestroy() {
        Destroy(blueLine);
        GameManager.MagneticsInScene.Remove(this);
    }

    public virtual void AddForce(Vector3 netForce) {
        if (!IsStatic) {
            LastExpectedAcceleration = netForce / Mass;
            Rb.AddForce(netForce, ForceMode.Force);
        }
    }
    public virtual void StopBeingPullPushed() { }

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
}
