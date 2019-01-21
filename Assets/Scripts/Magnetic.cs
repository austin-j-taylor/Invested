using cakeslice;
using UnityEngine;
using VolumetricLines;

/*
 * Signifies that this object is magnetic. This object can be Pushed or Pulled.
 * The RigidBody mass of this object signifies the total mass of this object, and the "magneticMass" field indicates
 *      the mass of this object that is actually magnetic (i.e. A person may weigh 60kg (RigidBody mass) but only have 3kg worth of metal on them, implying 57kg of non-metal.)
 * If the magneticMass field is left to be 0, then the object is considered to be wholly magnetic (i.e. a RigidBody mass of 60kg with 60kg worth of metal) and uses the RigidBody mass for the magnetic mass.
 * If the object does not have a RigidBody attached, then fall back to the netMass field for the net mass of the object.
 *      If the netMass field is left to be 0, an error is thrown. 
 *      If magneticMass is left to be 0, then the object is considered to be wholly magnetic and uses netMass for the magnetic mass.
 */
public class Magnetic : MonoBehaviour {
    private const float metalLinesLerpConstant = .30f;

    [SerializeField]
    private float netMass = 0;
    [SerializeField]
    private float magneticMass = 0;

    private bool lastWasPulled;
    protected bool isBeingPushPulled;
    private float lightSaberFactor;
    private Outline highlightedTargetOutline;
    private VolumetricLineBehavior blueLine;
    private Collider[] colliders;

    protected Rigidbody Rb { get; set; }

    public Vector3 Velocity {
        get {
            if (IsStatic)
                return Vector3.zero;
            else
                return Rb.velocity;
        }
    }
    public Vector3 LastPosition { get; private set; }
    public Vector3 LastVelocity { get; set; }
    public float LastExpectedVelocityChange { get; set; }
    public float LastExpectedEnergyUsed { get; set; }
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
    public Vector3 LastAnchoredPushBoostFromAllomancer { get; set; }
    public Vector3 LastAnchoredPushBoostFromTarget { get; set; }
    // The allomantic force, excluding the burn rate.
    public Vector3 LastMaxPossibleAllomanticForce { get; set; }

    public bool IsStatic { get; private set; }
    public bool HasColliders { get; private set; }

    public bool LastWasPulled {
        get {
            return lastWasPulled;
        }
        set {
            if (value != lastWasPulled) { // swapped between pulling and pushing, clear certain values
                lastWasPulled = value;
            }
        }
    }

    public Vector3 LastNetForceOnAllomancer {
        get {
            return LastAllomanticForce + LastAnchoredPushBoostFromTarget;
        }
    }

    public Vector3 LastNetForceOnTarget {
        get {
            return -LastAllomanticForce + LastAnchoredPushBoostFromAllomancer;
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

    public float ColliderBodyBoundsSizeY {
        get {
            if (HasColliders)
                return colliders[0].bounds.size.y;
            else
                return GetComponent<Renderer>().bounds.size.y;
        }
    }
    public float Charge { get; private set; }
    // If this Magnetic is at the center of the screen, highlighted, ready to be targeted.
    public bool IsHighlighted { get; private set; }
    // The total mass of this object (RigidBody mass)
    public float NetMass { get { return netMass; } }
    // The magnetic mass of this object
    public float MagneticMass { get { return magneticMass; } }
    // If the object has a Rigidbody, this is the real centerOfMass. Otherwise, it is just the transform position.
    // if the object is made of multiple colliders, find the center of volume of all of those colliders.
    // If the object has only one collider, the local center of mass is calculated at startup.
    private Vector3 centerOfMass;
    public Vector3 CenterOfMass {
        get {
            if (HasColliders && !IsStatic) {
                Vector3 centers = colliders[0].bounds.center;
                int triggerCount = 0;
                for (int i = 1; i < colliders.Length; i++) {
                    if (!colliders[i].isTrigger)
                        centers += colliders[i].bounds.center;
                    else
                        triggerCount++;
                }
                return centers / (colliders.Length - triggerCount);
            } else if (IsStatic) {
                // no collider or rigidbody, so center of mass is set to transform.position as a default
                return transform.position;
            } else {
                return transform.TransformPoint(centerOfMass);
            }
        }
    }
    public virtual bool IsPerfectlyAnchored { // Only relevant for low-mass targets
        get {
            return false;
        }
    }

    protected void Awake() {
        if (GetComponent<Renderer>())
            highlightedTargetOutline = gameObject.AddComponent<Outline>();
        blueLine = Instantiate(GameManager.MetalLineTemplate);
        Rb = GetComponentInParent<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        if (gameObject.layer != LayerMask.NameToLayer("Undetectable Magnetic"))
            GameManager.AddMagnetic(this);
        lightSaberFactor = 1;
        lastWasPulled = false;
        isBeingPushPulled = false;
        IsHighlighted = false;
        IsStatic = Rb == null;
        HasColliders = colliders.Length > 0;

        if (IsStatic) { // No RigidBody attached
            if (netMass == 0) {
                Debug.LogError("Magnetic's netMass is 0 on an object without a RigidBody " + name + " at " + transform.position);
            }
            if (magneticMass == 0) {
                magneticMass = netMass;
            }
        } else { // RigidBody attached, which has its own mass, which replaces netMass
            netMass = Rb.mass;
            if (magneticMass == 0) {
                magneticMass = netMass;
            }
            if (colliders.Length == 1) {
                centerOfMass = Rb.centerOfMass;
            }
        }

        Charge = Mathf.Pow(magneticMass, AllomanticIronSteel.chargePower);
        LastPosition = transform.position;
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastExpectedVelocityChange = 0;
        LastExpectedEnergyUsed = 0;
        LastAllomanticForce = Vector3.zero;
        LastMaxPossibleAllomanticForce = Vector3.zero;
        LastAnchoredPushBoostFromAllomancer = Vector3.zero;
        LastAnchoredPushBoostFromTarget = Vector3.zero;
    }

    // If the Magnetic is untargeted
    public void Clear() {
        StopBeingPullPushed();
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastExpectedVelocityChange = 0;
        LastExpectedEnergyUsed = 0;
        LastAllomanticForce = Vector3.zero;
        LastMaxPossibleAllomanticForce = Vector3.zero;
        LastAnchoredPushBoostFromAllomancer = Vector3.zero;
        LastAnchoredPushBoostFromTarget = Vector3.zero;
        RemoveTargetGlow();
    }

    private void OnDestroy() {
        Destroy(blueLine);
        GameManager.RemoveMagnetic(this);
    }

    private void OnDisable() {
        OnDestroy();
    }

    public virtual void AddForce(Vector3 netForce) {
        if (!IsStatic) {
            LastExpectedAcceleration = netForce / netMass;
            Rb.AddForce(netForce, ForceMode.Force);
        }
    }
    public virtual void StartBeingPullPushed() {
        isBeingPushPulled = true;
    }
    public virtual void StopBeingPullPushed() {
        isBeingPushPulled = false;
    }

    public void AddTargetGlow() {
        if (SettingsMenu.settingsData.highlightedTargetOutline == 1 && highlightedTargetOutline)
            highlightedTargetOutline.Enable();
    }

    public void RemoveTargetGlow() {
        if (highlightedTargetOutline)
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