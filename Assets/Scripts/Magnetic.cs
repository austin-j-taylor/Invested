using cakeslice;
using UnityEngine;
using VolumetricLines;
/*
 * Signifies that this object is magnetic. It can be Pushed or Pulled.
 * The RigidBody mass of this object signifies the total mass of this object, and the "magneticMass" field indicates
 *      the mass of this object that is actually magnetic (i.e. A person may weigh 60kg (RigidBody mass) but only have 3kg worth of metal on them, implying 57kg of non-metal.)
 * If the magneticMass field is left to be 0, then the object is considered to be wholly magnetic (i.e. a RigidBody mass of 60kg with 60kg worth of metal) and uses the RigidBody mass
 * If the object does not have a RigidBody attached, then fall back to the netMass field for the netMass.
 *      If the netMass field is left to be 0, an error is thrown. 
 *      If magneticMass is left to be 0, then the object is considered to be wholly magnetic and simply uses netMass.
 */
public class Magnetic : MonoBehaviour {

    private const float metalLinesLerpConstant = .30f;

    [SerializeField]
    private float netMass = 0;
    [SerializeField]
    private float magneticMass = 0;

    private bool lastWasPulled;
    private Outline highlightedTargetOutline;
    private VolumetricLineBehavior blueLine;
    private float lightSaberFactor;

    public AllomanticIronSteel Allomancer { get; set; }
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
    public Vector3 LastAnchoredPushBoostFromAllomancer { get; set; }
    public Vector3 LastAnchoredPushBoostFromTarget { get; set; }
    // The allomantic force, excluding the burn rate.
    public Vector3 LastMaxPossibleAllomanticForce { get; set; }

    public bool IsStatic { get; set; }

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

    public Vector3 LastNetAllomanticForceOnAllomancer {
        get {
            return LastAllomanticForce + LastAnchoredPushBoostFromTarget;
        }
    }

    public Vector3 LastNetAllomanticForceOnTarget {
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

    // If the object has a Rigidbody, this is the real centerOfMass. Otherwise, it is just the transform local position.
    public Vector3 LocalCenterOfMass { get; private set; }
    public Collider ColliderBody { get; private set; }
    public float Charge { get; private set; }
    // If this Magnetic is at the center of the screen, highlighted, ready to be targeted.
    public bool IsHighlighted { get; private set; }
    // The total mass of this object (RigidBody mass)
    public float NetMass { get { return netMass; } }
    // The magnetic mass of this object
    public float MagneticMass { get { return magneticMass; } }
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

    private void Awake() {
        Allomancer = null;
        highlightedTargetOutline = gameObject.AddComponent<Outline>();
        blueLine = Instantiate(GameManager.MetalLineTemplate);
        Rb = GetComponent<Rigidbody>();
        ColliderBody = GetComponent<Collider>();
        lightSaberFactor = 1;
        lastWasPulled = false;
        IsHighlighted = false;
        IsStatic = Rb == null;

        if (IsStatic) { // No RigidBody attached
            if (netMass == 0) {
                Debug.LogError("Magnetic's netMass is 0 on an object without a RigidBody " + name + " at " + transform.position);
            }
            if (magneticMass == 0) {
                magneticMass = netMass;
            }
            LocalCenterOfMass = Vector3.zero;
        } else { // RigidBody attached, which has its own mass, which replaces netMass
            netMass = Rb.mass;
            if (magneticMass == 0) {
                magneticMass = netMass;
            }
            LocalCenterOfMass = Rb.centerOfMass;
        }

        Charge = Mathf.Pow(magneticMass, AllomanticIronSteel.chargePower);
        LastPosition = transform.position;
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastMaxPossibleAllomanticForce = Vector3.zero;
        LastAnchoredPushBoostFromAllomancer = Vector3.zero;
        LastAnchoredPushBoostFromTarget = Vector3.zero;
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
        LastMaxPossibleAllomanticForce = Vector3.zero;
        LastAnchoredPushBoostFromAllomancer = Vector3.zero;
        LastAnchoredPushBoostFromTarget = Vector3.zero;
        Allomancer = null;
        lightSaberFactor = 1;
        blueLine.GetComponent<MeshRenderer>().enabled = false;
        RemoveTargetGlow();
    }

    private void OnDestroy() {
        Destroy(blueLine);
        GameManager.MagneticsInScene.Remove(this);
    }

    public virtual void AddForce(Vector3 netForce) {
        if (!IsStatic) {
            LastExpectedAcceleration = netForce / netMass;
            Rb.AddForce(netForce, ForceMode.Force);
        }
    }
    public virtual void StartBeingPullPushed() { }
    public virtual void StopBeingPullPushed() { }

    public void AddTargetGlow() {
        if(SettingsMenu.settingsData.highlightedTargetOutline == 1)
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