//using cakeslice;
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
    public const float lowLineColor = .3f;
    public const float highLineColor = .85f;
    private readonly Color brightBlue = new Color(0, .3f, highLineColor);

    [SerializeField]
    protected float netMass = 0;
    [SerializeField]
    private float magneticMass = 0;
    //// Assigned in the editor. Marks children that should also glow when this target is highlighted.
    ////[SerializeField]
    //private Renderer[] childMagnetics;

    private bool thisFrameIsBeingPushPulled = false;
    private bool isBeingPushPulled = false;
    public virtual bool IsBeingPushPulled {
        get {
             return isBeingPushPulled;
        }
        protected set {
            isBeingPushPulled = value;
        }
    }
    private float lightSaberFactor;
    private Color[] defaultEmissionColor;
    //private Outline highlightedTargetOutline;
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
    //public Vector3 LastExpectedVelocityChange { get; set; }
    //public float LastExpectedEnergyUsed { get; set; }
    public Vector3 LastExpectedAcceleration {
        get {
            return lastExpectedAcceleration;
        }
        protected set {
            lastExpectedAcceleration = value;
            LastPosition = transform.position;
        }
    }
    private Vector3 lastExpectedAcceleration;

    // These keep track of each Magnetic's participation to the net force on the Allomancer
    public Vector3 LastAllomanticForce { get; set; }
    public Vector3 LastAnchoredPushBoostFromAllomancer { get; set; }
    public Vector3 LastAnchoredPushBoostFromTarget { get; set; }
    // The allomantic force, excluding the burn rate.
    public Vector3 LastMaxPossibleAllomanticForce { get; set; }

    public bool IsStatic { get; protected set; }
    public bool HasColliders { get; private set; }

    private bool lastWasPulled;
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
    private bool isHighlighted;
    public bool IsHighlighted {
        get {
            return isHighlighted;
        }
        set {
            if (value != isHighlighted) {
                //if (value) {
                //    if (SettingsMenu.settingsData.highlightedTargetOutline == 1) {
                //        for (int i = 0; i < childMagnetics.Length; i++) {
                //            // Assign original emmisions
                //            if (childMagnetics[i].material.IsKeywordEnabled("_EMISSION"))
                //                defaultEmissionColor[i] = childMagnetics[i].material.GetColor("_EmissionColor");

                //            childMagnetics[i].material.SetColor("_EmissionColor", new Color(0, .35f, 1f) * Mathf.LinearToGammaSpace(2));
                //            childMagnetics[i].material.EnableKeyword("_EMISSION");
                //        }
                //    }
                //} else {
                //    for (int i = 0; i < childMagnetics.Length; i++) {
                //        if (defaultEmissionColor[i] == null)
                //            childMagnetics[i].material.DisableKeyword("_EMISSION");
                //        else
                //            childMagnetics[i].material.SetColor("_EmissionColor", defaultEmissionColor[i]);
                //    }
                //}

                isHighlighted = value;
            }
        }
    }
    // The total mass of this object (RigidBody mass)
    public float NetMass { get { return netMass; } }
    // The magnetic mass of this object
    public float MagneticMass { get { return magneticMass; } }
    // If the object has a Rigidbody, this is the real centerOfMass. Otherwise, it is just the transform position.
    //// if the object is made of multiple colliders, find the center of volume of all of those colliders.
    // If the object has only one collider, the local center of mass is calculated at startup.
    private Vector3 centerOfMass;
    public Vector3 CenterOfMass {
        get {
            if (IsStatic) {
                if (HasColliders) {
                    return transform.TransformPoint(centerOfMass);
                } else {
                    // no collider or rigidbody, so center of mass is set to transform.position as a default
                    return transform.position;
                }
            } else {
                if (HasColliders) {
                    // Not static, has colliders
                    return transform.TransformPoint(centerOfMass);
                } else {
                    // no collider or rigidbody, so center of mass is set to transform.position as a default
                    return transform.position;
                }
            }


            //if (HasColliders && !IsStatic) {
            //    //Vector3 centers = colliders[0].bounds.center;
            //    //int triggerCount = 0;
            //    //for (int i = 1; i < colliders.Length; i++) {
            //    //    if (!colliders[i].isTrigger)
            //    //        centers += colliders[i].bounds.center;
            //    //    else
            //    //        triggerCount++;
            //    //}
            //    return transform.TransformPoint(centerOfMass);
            //} else if (IsStatic) {
            //    // no collider or rigidbody, so center of mass is set to transform.position as a default
            //    return transform.position;
            //} else {
            //    // Not static, has colliders
            //    return transform.TransformPoint(centerOfMass);
            //}
        }
    }
    public virtual bool IsPerfectlyAnchored { // Only relevant for low-mass targets
        get {
            return false;
        }
    }

    protected void Awake() {
        //if (childMagnetics == null || childMagnetics.Length == 0) {
        //    // If not assigned in the editor, assume that all children should glow
        //    childMagnetics = GetComponentsInChildren<Renderer>();
        //}
        //defaultEmissionColor = new Color[childMagnetics.Length];
        if (!IsStatic) { // assigned by MagneticDense
            Rb = GetComponentInParent<Rigidbody>();
            IsStatic = (Rb == null);
        }
        blueLine = Instantiate(GameManager.MetalLineTemplate);
        colliders = GetComponentsInChildren<Collider>();
        lightSaberFactor = 1;
        lastWasPulled = false;
        IsBeingPushPulled = false;
        isHighlighted = false;
        HasColliders = colliders.Length > 0;

        if (IsStatic) { // No RigidBody attached
            if (netMass == 0) {
                Debug.LogError("Magnetic's netMass is 0 on an object without a RigidBody " + name + " at " + transform.position);
            }
            if (magneticMass == 0) {
                magneticMass = netMass;
            }
            if (HasColliders) {
                //Vector3 centers = colliders[0].bounds.center;
                //int triggerCount = 0;
                //for (int i = 1; i < colliders.Length; i++) {
                //    if (!colliders[i].isTrigger)
                //        centers += colliders[i].bounds.center;
                //    else
                //        triggerCount++;
                //}
                //centerOfMass = transform.InverseTransformPoint(centers / (colliders.Length - triggerCount));
                //centerOfMass = transform.InverseTransformPoint(GetComponentInChildren<Renderer>().bounds.center);
                centerOfMass = Vector3.zero;
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
        //LastExpectedVelocityChange = Vector3.zero;
        //LastExpectedEnergyUsed = 0;
        LastAllomanticForce = Vector3.zero;
        LastMaxPossibleAllomanticForce = Vector3.zero;
        LastAnchoredPushBoostFromAllomancer = Vector3.zero;
        LastAnchoredPushBoostFromTarget = Vector3.zero;
    }

    private void FixedUpdate() {
        LastVelocity = Velocity;
        IsBeingPushPulled = thisFrameIsBeingPushPulled;
        thisFrameIsBeingPushPulled = false;
    }

    // If the Magnetic is untargeted
    public void Clear() {
        IsBeingPushPulled = false;
        LastVelocity = Vector3.zero;
        LastExpectedAcceleration = Vector3.zero;
        //LastExpectedVelocityChange = Vector3.zero;
        //LastExpectedEnergyUsed = 0;
        LastAllomanticForce = Vector3.zero;
        LastMaxPossibleAllomanticForce = Vector3.zero;
        LastAnchoredPushBoostFromAllomancer = Vector3.zero;
        LastAnchoredPushBoostFromTarget = Vector3.zero;
        IsHighlighted = false;
    }

    private void OnDestroy() {
        Destroy(blueLine);
        GameManager.RemoveMagnetic(this);
    }

    private void OnDisable() {
        DisableBlueLine();
        GameManager.RemoveMagnetic(this);
    }

    private void OnEnable() {
        if (gameObject.layer != LayerMask.NameToLayer("Undetectable Magnetic"))
            GameManager.AddMagnetic(this);
    }

    public virtual void AddForce(Vector3 netForce, Vector3 allomanticForce /* unused for the Magnetic base class */) {
        if (!IsStatic) {
            LastExpectedAcceleration = netForce / netMass;
            Rb.AddForce(netForce, ForceMode.Force);
        }
        thisFrameIsBeingPushPulled = true;
    }

    // Set properties of the blue line pointing to this metal
    public void SetBlueLine(Vector3 endPos, float width, float lsf, Color color) {
        if (blueLine) {
            blueLine.gameObject.SetActive(true);
            blueLine.StartPos = CenterOfMass;
            blueLine.EndPos = endPos;
            blueLine.LineWidth = width;
            lightSaberFactor = Mathf.Lerp(lightSaberFactor, lsf, metalLinesLerpConstant);
            blueLine.LightSaberFactor = lightSaberFactor;
            blueLine.LineColor = color;
        } else {
            Debug.Log("Null: " + gameObject);
            Debug.Log("equa: " + (gameObject == Player.PlayerInstance.gameObject));
            Debug.Log("Null: " + name);
            Debug.Log("Null: " + tag);
            Debug.Log("Null: " + blueLine);
            Debug.Log("Null: " + GameManager.MagneticsInScene.Contains(this));
        }
    }

    public void SetBlueLine(Vector3 endPos, float width, float lsf, float closeness) {
        SetBlueLine(endPos, width, lsf, new Color(0, closeness * lowLineColor, closeness * highLineColor, 1));
    }

    // Brighten this particular metal's blue line
    public void BrightenLine() {
        blueLine.LineColor = brightBlue;
    }

    public void DisableBlueLine() {
        if (blueLine)
            blueLine.gameObject.SetActive(false);
    }

    // Checks if the Allomancer would be able to sense this Magnetic with ironsight
    public bool IsInRange(AllomanticIronSteel allomancer, float burnRate) {
        return allomancer.CalculateAllomanticForce(this).magnitude * burnRate > SettingsMenu.settingsData.metalDetectionThreshold;
    }
}