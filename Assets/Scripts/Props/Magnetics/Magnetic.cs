//using cakeslice;
using UnityEngine;
using VolumetricLines;

/// <summary>
/// An object can be Pushed or Pulled.
/// </summary>
public class Magnetic : MonoBehaviour {

    #region constants
    private const float metalLinesLerpConstant = .30f;
    public const float lowLineColor = .3f;
    public const float highLineColor = .85f;
    private readonly Color brightBlue = new Color(0, .3f, highLineColor);
    #endregion

    #region serializedFields
    [SerializeField]
    public Prop_SO prop_SO = null;
    [SerializeField]
    protected float netMass = 0;
    [SerializeField]
    private float magneticMass = 0;
    // This only matters if this Magnetic is static (no rigidbody).
    // If true, on startup, find the center of mass by the collider's positions.
    // If false, find the center of pass by the gameobject's position.
    [SerializeField]
    private bool calculateCOMFromColliders = false;
    #endregion

    #region fields
    protected bool thisFrameIsBeingPushPulled = false;
    private Collider[] colliders;
    private Color[] defaultEmissionColor;
    private VolumetricLineBehavior blueLine;
    private float lightSaberFactor;
    private Vector3 cachedCenterOfMass;
    private bool customCenterOfMass = false;
    #endregion

    #region properties
    public Rigidbody Rb { get; protected set; }
    public bool IsStatic { get; protected set; }
    public Collider MainCollider => (HasColliders ? colliders[0] : null);
    public bool HasColliders { get; private set; }
    public bool HasRenderer { get; private set; }
    public virtual bool IsBeingPushPulled { get; protected set; } = false;
    public bool LastWasPulled { get; set; } = false;
    public Vector3 Velocity => IsStatic ? Vector3.zero : Rb.velocity;
    public Vector3 LastPosition { get; private set; }
    public Vector3 LastVelocity { get; set; }
    public Vector3 LastExpectedAcceleration { get; protected set; }
    // These keep track of each Magnetic's participation to the net force on the Allomancer
    public Vector3 LastAllomanticForce { get; set; }
    public Vector3 LastAnchoredPushBoostFromAllomancer { get; set; }
    public Vector3 LastAnchoredPushBoostFromTarget { get; set; }
    // The allomantic force, excluding the burn rate.
    public Vector3 LastMaxPossibleAllomanticForce { get; set; }
    public Vector3 LastNetForceOnAllomancer => LastAllomanticForce + LastAnchoredPushBoostFromTarget;
    public Vector3 LastNetForceOnTarget => -LastAllomanticForce + LastAnchoredPushBoostFromAllomancer;
    public Vector3 LastAllomanticForceOnAllomancer => LastAllomanticForce;
    public Vector3 LastAllomanticForceOnTarget => -LastAllomanticForce;
    public float ColliderBodyBoundsSizeY => HasColliders ? colliders[0].bounds.size.y : HasRenderer ? GetComponent<Renderer>().bounds.size.y : 0;

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
    /// <summary>
    /// NetMass and MagneticMass:  A person may weigh 60kg (RigidBody mass, NetMass) but only have 3kg worth of metal on them (MagneticMass)
    /// If the magneticMass field is left to be 0, then the object is considered to be wholly magnetic (i.e. a RigidBody mass of 60kg with 60kg worth of metal) and uses the RigidBody mass for the magnetic mass.
    /// If the object does not have a RigidBody attached, then fall back to the netMass field for the net mass of the object.
    /// </summary>
    public float NetMass => netMass; // The total mass of this object (RigidBody mass)
    public float MagneticMass => magneticMass; // The magnetic mass of this object
    // If the object has a Rigidbody, this is the real centerOfMass. Otherwise, it is just the transform position.
    //// if the object is made of multiple colliders, find the center of volume of all of those colliders.
    // If the object has only one collider, the local center of mass is calculated at startup.
    private Vector3 centerOfMass;
    public Vector3 CenterOfMass {
        get {
            if (transform.hasChanged || (Rb != null && !Rb.IsSleeping())) {
                UpdateCenterOfMass();
                transform.hasChanged = false;
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
            return cachedCenterOfMass;
        }
        set {
            // Sets the local center of mass
            customCenterOfMass = true;
            centerOfMass = value;
        }
    }

    public virtual bool IsPerfectlyAnchored => false; // Overriden by subclasses, for low-mass targets
    #endregion

    #region clearing
    protected virtual void Awake() {
        //if (childMagnetics == null || childMagnetics.Length == 0) {
        //    // If not assigned in the editor, assume that all children should glow
        //    childMagnetics = GetComponentsInChildren<Renderer>();
        //}
        //defaultEmissionColor = new Color[childMagnetics.Length];
        if (!IsStatic) { // assigned by MagneticDense
            Rb = GetComponentInParent<Rigidbody>();
            IsStatic = (Rb == null);
        }
        if (GameManager.MetalLinesTransform != null) {
            blueLine = Instantiate(GameManager.MetalLineTemplate, GameManager.MetalLinesTransform);
            blueLine.gameObject.SetActive(false);
        }
        colliders = GetComponentsInChildren<Collider>();
        lightSaberFactor = 1;
        isHighlighted = false;
        HasColliders = colliders.Length > 0;
        HasRenderer = GetComponent<Renderer>() != null;

        if (IsStatic) { // No RigidBody attached
            if (netMass == 0) {
                Debug.LogError("Magnetic's netMass is 0 on an object without a RigidBody " + name + " at " + transform.position, gameObject);
            }
            if (magneticMass == 0) {
                magneticMass = netMass;
            }
            if (HasColliders) {
                if (calculateCOMFromColliders) {
                    Vector3 centers = colliders[0].bounds.center;
                    int triggerCount = 0;
                    for (int i = 1; i < colliders.Length; i++) {
                        if (!colliders[i].isTrigger)
                            centers += colliders[i].bounds.center;
                        else
                            triggerCount++;
                    }
                    centerOfMass = transform.InverseTransformPoint(centers / (colliders.Length - triggerCount));
                    centerOfMass = transform.InverseTransformPoint(GetComponentInChildren<Renderer>().bounds.center);
                } else {
                    centerOfMass = Vector3.zero;
                }
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

        UpdateCenterOfMass();
        Charge = Mathf.Pow(magneticMass, AllomanticIronSteel.chargePower);
        LastVelocity = Vector3.zero;
        LastPosition = transform.position;
        LastExpectedAcceleration = Vector3.zero;
        //LastExpectedVelocityChange = Vector3.zero;
        //LastExpectedEnergyUsed = 0;
        LastAllomanticForce = Vector3.zero;
        LastMaxPossibleAllomanticForce = Vector3.zero;
        LastAnchoredPushBoostFromAllomancer = Vector3.zero;
        LastAnchoredPushBoostFromTarget = Vector3.zero;
    }

    protected virtual void Start() {
        if (blueLine)
            blueLine.gameObject.SetActive(false);
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

    public void OnDisable() {
        DisableBlueLine();
        GameManager.RemoveMagnetic(this);
    }
    public void OnEnable() {
        if (gameObject.layer != LayerMask.NameToLayer("Undetectable Magnetic"))
            GameManager.AddMagnetic(this);
    }
    #endregion

    #region physics
    private void FixedUpdate() {
        if (!IsStatic) {
            LastVelocity = Velocity;
            LastPosition = transform.position;
        }
        IsBeingPushPulled = thisFrameIsBeingPushPulled;
        thisFrameIsBeingPushPulled = false;
    }

    /// <summary>
    /// Adds a force from a Push or Pull to this object.
    /// </summary>
    /// <param name="netForce">the Net Allomantic Force</param>
    /// <param name="allomanticForce">just the Allomantic Force component</param>
    public virtual void AddForce(Vector3 netForce, Vector3 allomanticForce /* unused for the Magnetic base class */) {
        if (!IsStatic) {
            LastExpectedAcceleration = netForce / netMass;
            Rb.AddForce(netForce, ForceMode.Force);
        }
        thisFrameIsBeingPushPulled = true;
    }

    /// <summary>
    /// Checks if the Allomancer would be able to sense this Magnetic with ironsight
    /// </summary>
    /// <param name="allomancer">the Allomancer to check against</param>
    /// <param name="burnRate">the burn % of the Allomancer</param>
    /// <returns></returns>
    public bool IsInRange(AllomanticIronSteel allomancer, float burnRate) {
        return allomancer.CalculateAllomanticForce(this).magnitude * burnRate > SettingsMenu.settingsAllomancy.metalDetectionThreshold;
    }

    private void UpdateCenterOfMass() {
        if (IsStatic || customCenterOfMass) {
            if (HasColliders || customCenterOfMass) {
                cachedCenterOfMass = transform.TransformPoint(centerOfMass);
            } else {
                // no collider or rigidbody, so center of mass is set to transform.position as a default
                cachedCenterOfMass = transform.position;
            }
        } else {
            if (HasColliders) {
                // Not static, has colliders
                cachedCenterOfMass = transform.TransformPoint(centerOfMass);
            } else {
                // no collider or rigidbody, so center of mass is set to transform.position as a default
                cachedCenterOfMass = transform.position;
            }
        }
    }
    #endregion

    #region blueLines
    // 
    /// <summary>
    /// Set properties of the blue line pointing to this metal
    /// </summary>
    /// <param name="endPos">the other end of the line</param>
    /// <param name="width">the width of the line</param>
    /// <param name="lsf">the "light saber factor", or the white core of the line</param>
    /// <param name="color">the line's color</param>
    public void SetBlueLine(Vector3 endPos, float width, float lsf, Color color) {
        if (blueLine && enabled) {
            blueLine.gameObject.SetActive(true);
            blueLine.SetStartAndEndAndWidth(CenterOfMass, endPos, width);
            if (lightSaberFactor != 0 || lsf != 0) {
                lightSaberFactor = Mathf.Lerp(lightSaberFactor, lsf, metalLinesLerpConstant);
                blueLine.LightSaberFactor = lightSaberFactor;
            }
            blueLine.LineColor = color;
        }
    }

    public void SetBlueLine(Vector3 endPos, float width, float lsf, float brightness) {
        SetBlueLine(endPos, width, lsf, new Color(0, brightness * lowLineColor, brightness * highLineColor, 1));
    }

    // Brighten this particular metal's blue line
    public void BrightenLine() {
        //blueLine.LineColor = brightBlue;
        blueLine.LineColor = blueLine.LineColor * 2;
    }

    public void DisableBlueLine() {
        if (blueLine && blueLine.gameObject.activeSelf)
            blueLine.gameObject.SetActive(false);
    }
    #endregion
}