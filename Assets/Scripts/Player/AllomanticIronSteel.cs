using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumetricLines;
/*
 * Controls all facets of Ironpulling and Steelpushing. Any Ironpuller or Steelpusher would have this attached to their GameObject.
 */
public class AllomanticIronSteel : MonoBehaviour {

    // Constants
    private const float AllomanticConstant = 4000;
    private const int maxNumberOfTargets = 10;
    private const float maxRange = 50f;
    private const float closenessThreshold = 1f;
    private const float chargePower = 1f / 8f;
    private readonly Vector3 centerOfScreen = new Vector3(.5f, .5f, 0);
    // Constants for Metal Lines
    private const float startWidth = .05f;
    private const float horizontalMin = .45f;
    private const float horizontalMax = .55f;
    private const float verticalMin = .2f;
    private const float verticalMax = .8f;
    private const float gMaxLines = .15f;
    private const float bMaxLines = 1;
    private const float luminosityFactor = .4f;
    private const float MetalLinesLerpConstant = burnRateMeterLerpConstant;//.05f;
    private const float verticalImportanceFactor = 100f;
    private const float lightSaberConstant = 200f;
    // Constants for Burn Rate Meter
    private const float burnRateMeterLerpConstant = .30f;
    private const float minAngle = .12f;
    private const float maxAngle = 1f - 2 * minAngle;
    
    // Button-press time constants
    private const float timeToHoldDown = .5f;
    private const float timeDoubleTapWindow = .5f;

    // Simple metal booleans for passing to methods
    private const bool steel = false;
    private const bool iron = true;

    private LayerMask ignorePlayerLayer;
    private GamepadController gamepad;
    private PlayerMovementController movementController;
    private Rigidbody rb;
    private List<VolumetricLineBehavior> metalLines;
    private VolumetricLineBehavior metalLineTemplate;
    [SerializeField]
    private Material ironpullTargetedMaterial;
    [SerializeField]
    private Transform metalLinesAnchor;
    [SerializeField]
    private Transform centerOfMass;
    [SerializeField]
    private Text metalLineText;
    [SerializeField]
    private Text burnRateMeterPercent;
    [SerializeField]
    private Image burnRateImage;

    private bool HasPullTarget {
        get {
            return pullCount != 0; ;
        }
    }
    private bool HasPushTarget {
        get {
            return pushCount != 0;
        }
    }
    private Vector3 CenterOfMass {
        get {
            return transform.TransformPoint(rb.centerOfMass);
        }
    }
    // Button held-down times
    private float timeToStopBurning = 0f;
    private float timeToSwapBurning = 0f;

    // Currently hovered-over Magnetic
    private Magnetic lastHoveredOverTarget;

    // Magnetic variables
    private int availableNumberOfTargets;
    private int pullCount;
    private int pushCount;
    private Magnetic[] pullTargets;
    private Magnetic[] pushTargets;

    // Used for calculated the acceleration over the last frame for pushing/pulling
    private Vector3 lastAllomancerVelocity = Vector3.zero;
    private Vector3 lastExpectedAllomancerAcceleration = Vector3.zero;
    private Vector3 currentExpectedAllomancerAcceleration = Vector3.zero;

    // Used for burning metals
    private float ironBurnRate;
    private float steelBurnRate;

    public bool IronPulling { get; private set; }
    public bool SteelPushing { get; private set; }
    public bool IsBurningIronSteel { get; private set; }
    public float Mass {
        get {
            return rb.mass;
        }
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        gamepad = GetComponent<GamepadController>();
        movementController = GetComponent<PlayerMovementController>();
        metalLines = new List<VolumetricLineBehavior>();
        metalLineTemplate = GetComponentInChildren<VolumetricLineBehavior>();
        centerOfMass.localPosition = Vector3.zero;
        metalLinesAnchor.localPosition = centerOfMass.localPosition;

        ignorePlayerLayer = ~(1 << LayerMask.NameToLayer("Player"));

        availableNumberOfTargets = 1;
        pullCount = 0;
        pushCount = 0;
        pullTargets = new Magnetic[maxNumberOfTargets];
        pushTargets = new Magnetic[maxNumberOfTargets];

        IronPulling = false;
        SteelPushing = false;
        ironBurnRate = .2f;
        steelBurnRate = .2f;
        lastHoveredOverTarget = null;

        metalLineText.text = "";
        burnRateImage.color = new Color(0, .5f, 1, .75f);
        burnRateImage.fillAmount = minAngle;
        burnRateMeterPercent.text = "";
    }


    private void Update() {
        bool searchingForTarget = true;
        bool selecting;
        Magnetic target = null;

        // Start burning
        if ((Keybinds.SelectDown() || Keybinds.SelectAlternateDown()) && !Keybinds.Negate()) {
            StartBurningIronSteel();
        }

        if (IsBurningIronSteel) {
            // Check scrollwheel for changing the max number of targets and burn rate
            if (!GamepadController.UsingGamepad) {
                if (Keybinds.ScrollWheelButton()) {
                    if (Keybinds.ScrollWheelAxis() != 0) {
                        if (Keybinds.ScrollWheelAxis() > 0) {
                            IncrementNumberOfTargets();
                        } else if (Keybinds.ScrollWheelAxis() < 0) {
                            DecrementNumberOfTargets();
                        }
                    }
                } else {
                    ChangeBurnRate(Keybinds.ScrollWheelAxis());
                }
            } else {
                float scrollValue = Keybinds.ScrollWheelAxis();
                if (scrollValue > 0) {
                    IncrementNumberOfTargets();
                } if (scrollValue < 0) {
                    DecrementNumberOfTargets();
                }
            }
            IronPulling = Keybinds.IronPulling();
            SteelPushing = Keybinds.SteelPushing();

            selecting = (Keybinds.Select() || Keybinds.SelectAlternate()) && !Keybinds.Negate();
            // Assume that targets are now out of range. SearchForTargets will mark all targets within maxRange to still be in range.
            SetAllTargetsOutOfRange();
            target = SearchForMetals(searchingForTarget);
            // If any targets were not set to be in range from SearchForMetals, remove them
            RemoveAllOutOfRangeTargets();

            // highlight the potential target you would select, if you targeted it
            if (target != null && target != lastHoveredOverTarget) {
                RemoveTargetGlow(lastHoveredOverTarget);
                AddTargetGlow(target);
                lastHoveredOverTarget = target;
            } else if (target == null) {
                RemoveTargetGlow(lastHoveredOverTarget);
                lastHoveredOverTarget = null;
            }


            if (Keybinds.Select() || Keybinds.SelectAlternate()) {
                // Select or Deselect pullTarget and/or pushTarget
                if (Keybinds.Select()) {
                    if (selecting) {
                        AddTarget(target, iron);
                    } else {
                        if (Keybinds.SelectDown()) {
                            if (IsTarget(target, iron)) {
                                RemoveTarget(target, iron);
                            } else {
                                RemoveTarget(0, iron);
                            }
                        }
                    }
                }
                if (Keybinds.SelectAlternate()) {
                    if (selecting) {
                        AddTarget(target, steel);
                    } else {
                        if (Keybinds.SelectAlternateDown()) {
                            if (IsTarget(target, steel)) {
                                RemoveTarget(target, steel);
                            } else {
                                RemoveTarget(0, steel);
                            }
                        }
                    }
                }
            }
        }

        // Stop burning altogether, hide metal lines
        if (Keybinds.Negate()) {
            if (Keybinds.Select() && Keybinds.SelectAlternate() && Time.time > timeToStopBurning) {
                StopBurningIronSteel();
                timeToStopBurning = 0;
            }
        } else {
            timeToStopBurning = 0;
        }

        // Stop burning altogether, hide metal lines
        if (Keybinds.NegateDown()) {
            timeToStopBurning = Time.time + timeToHoldDown;
        }

        // Swap pull- and push- targets
        if (Keybinds.NegateDown() && timeToSwapBurning > Time.time) {
            // Double-tapped, Swap targets
            SwapPullPushTargets();
        } else {
            if (Keybinds.NegateDown()) {
                timeToSwapBurning = Time.time + timeDoubleTapWindow;
            }
        }

        // Apply pushing/pulling force
        if (IsBurningIronSteel) {
            if (GamepadController.UsingGamepad) {
                ironBurnRate = Keybinds.RightBurnRate();
                steelBurnRate = Keybinds.LeftBurnRate();
            }
            SetPullRate(ironBurnRate);
            SetPushRate(steelBurnRate);
            RefreshBurnRateMeter();
        }

        if (IsBurningIronSteel && !searchingForTarget) { // not searching for a new target but still burning passively, show lines
            SearchForMetals(searchingForTarget);
        }

    }

    private void FixedUpdate() {
        if (IsBurningIronSteel) {
            if (HasPullTarget) { // if has pull targets...
                if (IronPulling) {
                    CalculatePushPullForces(iron);
                    PullPushOnTargets(iron, iron);
                } else // cannot push and pull on the same targets at once
                if (!HasPushTarget) {
                    if (SteelPushing) {
                        CalculatePushPullForces(iron);
                        PullPushOnTargets(steel, iron);
                    } else { // neither pushing nor pulling on ironTargets
                        //ResetPullStatus(iron);
                        CalculatePushPullForces(iron, false);
                    }
                } else {
                    CalculatePushPullForces(iron, false);
                    CalculatePushPullForces(steel, false);
                }
            }
            if (HasPushTarget) {
                if (SteelPushing) {
                    CalculatePushPullForces(steel);
                    PullPushOnTargets(steel, steel);
                } else
                if (!HasPullTarget) {
                    if (IronPulling) {
                        CalculatePushPullForces(steel);
                        PullPushOnTargets(iron, steel);
                    } else {
                        //ResetPullStatus(steel);
                        CalculatePushPullForces(steel, true);
                    }
                }
            }
            lastAllomancerVelocity = rb.velocity;
            lastExpectedAllomancerAcceleration = currentExpectedAllomancerAcceleration;
            currentExpectedAllomancerAcceleration = Vector3.zero;
        }
    }

    private void CalculatePushPullForces(bool usingIronTargets, bool addNormals = true) {
        if (usingIronTargets)
            for (int i = 0; i < pullCount; i++) {
                CalculateForce(pullTargets[i], iron, addNormals);
            } else
            for (int i = 0; i < pushCount; i++) {
                CalculateForce(pushTargets[i], steel, addNormals);
            }
    }

    private void PullPushOnTargets(bool pulling, bool usingIronTargets) {
        if (usingIronTargets) {
            for (int i = 0; i < pullCount; i++) {
                AddForce(pullTargets[i], pulling);
            }
        } else {
            for (int i = 0; i < pushCount; i++) {
                AddForce(pushTargets[i], pulling);
            }
        }
    }

    //private void ResetPullStatus(bool usingIronTargets) {
    //    if (usingIronTargets) {
    //        for (int i = 0; i < pullCount; i++) {
    //            pullTargets[i].LastWasPulled = true;
    //        }
    //    } else {
    //        for (int i = 0; i < pushCount; i++) {
    //            pushTargets[i].LastWasPulled = false;
    //        }
    //    }
    //}

    // Debug
    public float charge;
    public Vector3 allomanticsForce;
    public float netAllomancersForce;
    public float netTargetsForce;
    public Vector3 resititutionFromTargetsForce;
    public Vector3 resititutionFromPlayersForce;
    public float percentOfTargetForceReturned;
    public float percentOfAllomancerForceReturned;

    /* Pushing and Pulling
     * 
     * F =  C * Burn rate * sixteenth root of (Allomancer mass * target mass) 
     *      / squared distance between the two
     *      / number of targets currently being pushed on (push on one target => 100% of the push going to them, push on three targets => 33% of each push going to each, etc. Not thought out very in-depth.)
     *  C: an Allomantic Constant. I chose a value that felt right.
     */
    private void CalculateForce(Magnetic target, bool usingIronTargets, bool addNormals = true) {
        Vector3 positionDifference = target.CenterOfMass - CenterOfMass;
        // If the target is extremely close to the player, prevent the distance from being so low that the force approaches infinity
        float distance = Mathf.Max(positionDifference.magnitude, closenessThreshold);

        Vector3 allomanticForce = AllomanticConstant * (target.LastWasPulled ? ironBurnRate : steelBurnRate) * Mathf.Pow(target.Mass * rb.mass, chargePower) * (positionDifference / distance / distance) / (usingIronTargets ? pullCount : pushCount);
        Vector3 restitutionForceFromTarget;
        Vector3 restitutionForceFromAllomancer;
        if (addNormals) {
            if (target.IsStatic) {
                // If the target has no rigidbody, let the let the restitution force equal the allomantic force. It's a perfect anchor.
                // Thus:
                // a push against a perfectly anchored metal structure is exactly twice as powerful as a push against a completely unanchored, freely-moving metal structure
                restitutionForceFromTarget =  allomanticForce;
                restitutionForceFromAllomancer = Vector3.zero; // irrelevant
            } else {
                // Calculate Allomantic Normal Forces

                if (target.IsPerfectlyAnchored) { // If target is perfectly anchored, its ANF = AF.



                    restitutionForceFromTarget = allomanticForce;
                } else { // Target is partially anchored
                         //calculate changes from the last frame
                    Vector3 newTargetVelocity = target.Rb.velocity;
                    Vector3 lastTargetAcceleration = (newTargetVelocity - target.LastVelocity) / Time.fixedDeltaTime;
                    Vector3 unaccountedForTargetAcceleration = target.LastExpectedAcceleration - lastTargetAcceleration;// + Physics.gravity;
                    restitutionForceFromTarget = Vector3.ClampMagnitude(Vector3.Project(-unaccountedForTargetAcceleration * target.Mass, positionDifference.normalized), allomanticForce.magnitude);
                }
                Vector3 newAllomancerVelocity = rb.velocity;
                Vector3 lastAllomancerAcceleration = (newAllomancerVelocity - lastAllomancerVelocity) / Time.fixedDeltaTime;
                Vector3 unaccountedForAllomancerAcceleration = lastExpectedAllomancerAcceleration - lastAllomancerAcceleration;
                //if (!movementController.IsGrounded) {
                //    unaccountedForAllomancerAcceleration += Physics.gravity;
                //}
                restitutionForceFromAllomancer = Vector3.ClampMagnitude(Vector3.Project(-unaccountedForAllomancerAcceleration * rb.mass, positionDifference.normalized), allomanticForce.magnitude);

                // using Impulse strategy
                //restitutionForceFromTarget = Vector3.ClampMagnitude(Vector3.Project(target.forceFromCollisionTotal, positionDifference.normalized), allomanticForce.magnitude);

                target.LastPosition = target.transform.position;
                target.LastVelocity = target.Rb.velocity;
            }
        } else {
            restitutionForceFromTarget = Vector3.zero;
            restitutionForceFromAllomancer = Vector3.zero;
        }
        target.LastAllomanticForce = allomanticForce;
        target.LastAllomanticNormalForceFromAllomancer = restitutionForceFromAllomancer;
        target.LastAllomanticNormalForceFromTarget = restitutionForceFromTarget;

        // Debug
        charge = Mathf.Pow(target.Mass, chargePower) * Mathf.Pow(rb.mass, chargePower);
    }

    private void AddForce(Magnetic target, bool pulling) {
        target.LastWasPulled = pulling;
        //Debug.Log(target.LastAllomanticNormalForceFromTarget);

        //Vector3 netForceOnTarget;
        //Vector3 netForceOnAllomancer;
        //// calculate net forces
        //if (pulling) {
        //    netForceOnTarget = ironBurnRate * (target.LastNetAllomanticForceOnTarget);
        //    netForceOnAllomancer = ironBurnRate * (target.LastNetAllomanticForceOnAllomancer);
        //} else { // pushing
        //    netForceOnTarget = steelBurnRate * (target.LastNetAllomanticForceOnTarget);
        //    netForceOnAllomancer = steelBurnRate * (target.LastNetAllomanticForceOnAllomancer);
        //}

        Vector3 netForceOnTarget = target.LastNetAllomanticForceOnTarget;
        Vector3 netForceOnAllomancer = target.LastNetAllomanticForceOnAllomancer;


        target.AddForce(netForceOnTarget, ForceMode.Force);

        // apply force to player
        rb.AddForce(netForceOnAllomancer, ForceMode.Force);

        //target.Rb.AddForce(targetVelocity, ForceMode.VelocityChange);

        //rb.AddForce(allomancerVelocity, ForceMode.VelocityChange);

        // set up for next frame
        //lastExpectedNormalTargetAcceleration = -restitutionForceFromAllomancer / target.Mass * Time.fixedDeltaTime;
        //lastAllomancerVelocity = rb.velocity;
        currentExpectedAllomancerAcceleration += netForceOnAllomancer / rb.mass;

        //lastExpectedNormalAllomancerAcceleration = restitutionForceFromTarget / rb.mass * Time.fixedDeltaTime;

        // Debug
        allomanticsForce = target.LastAllomanticForce;
        netAllomancersForce = netForceOnAllomancer.magnitude;
        resititutionFromTargetsForce = target.LastAllomanticNormalForceFromTarget;
        resititutionFromPlayersForce = target.LastAllomanticNormalForceFromAllomancer;
        percentOfTargetForceReturned = resititutionFromTargetsForce.magnitude / allomanticsForce.magnitude;
        percentOfAllomancerForceReturned = resititutionFromPlayersForce.magnitude / allomanticsForce.magnitude;
        netTargetsForce = netForceOnTarget.magnitude;
    }

    //private const float additive = 1f / (maxRange + 1f);

    /*
     * Searches for all Magnetics within maxRange. Shows metal lines drawing from them to the player. If searchingForTargets, returns the Magnetic "closest" to the center of the screen. Returns null otherwise.
     * 
     * Rules for the metal lines:
     *  - The WIDTH of the line is dependant on the ratio of the MASS of the pullTarget object to the player
     *  - The BRIGHTNESS of the line is dependant on the DISTANCE from the player
     *  - The LIGHT SABER FACTOR is 1 for all metals. If the player has actually targeted a metal, the light saber factor becomes lower.
     * 
     */
    private Magnetic SearchForMetals(bool searchingForTargets) {

        Camera sight = Camera.main;
        float centerestDistanceFromCenter = 1f;
        Magnetic centerestObject = null;
        int centerestObjectIndex = 0;
        Collider[] nearbyMetals = Physics.OverlapSphere(sight.transform.position, maxRange);

        int lines = 0;
        int colIndex = 0;
        while (colIndex < nearbyMetals.Length) {
            Magnetic objectToTarget = nearbyMetals[colIndex].GetComponent<Magnetic>();
            if (objectToTarget != null) { // If object is magnetic
                objectToTarget.InRange = true;
                if (searchingForTargets) {
                    // If searching for a pullTarget, calculate the object's position on screen.
                    Vector3 screenPosition = sight.WorldToViewportPoint(objectToTarget.transform.position);
                    if (screenPosition.z > 0 && screenPosition.x > horizontalMin && screenPosition.x < horizontalMax && screenPosition.y > verticalMin && screenPosition.y < verticalMax) {
                        // Test if the new object is the more ideal pullTarget than the last most ideal pullTarget
                        float distanceFromCenter = verticalImportanceFactor * Mathf.Pow(screenPosition.x - .5f, 2) + Mathf.Pow(screenPosition.y - .5f, 2);
                        if (distanceFromCenter < centerestDistanceFromCenter) {
                            centerestDistanceFromCenter = distanceFromCenter;
                            centerestObjectIndex = lines;
                            centerestObject = objectToTarget;
                        }
                    }
                }

                // Instantiate a new line, if necessary
                if (metalLines.Count <= lines) {
                    VolumetricLineBehavior newLine = Instantiate(metalLineTemplate);
                    metalLines.Add(newLine);
                }

                // Set line properties
                metalLines[lines].GetComponent<MeshRenderer>().enabled = true;
                //float closeness = Mathf.Pow(1f / (Mathf.Clamp((transform.position - objectToTarget.transform.position).magnitude, 0, 50) + 1) - additive, .5f);
                //float closeness = Mathf.Pow(((maxRange + 1) / (Mathf.Min((transform.position - objectToTarget.transform.position).magnitude, maxRange) + 1) - 1) / maxRange, luminosityFactor);
                float closeness = Mathf.Pow((maxRange + 1) / (maxRange * (Mathf.Min((transform.position - objectToTarget.transform.position).magnitude, maxRange) + 1)) - 1 / maxRange, luminosityFactor);
                //float closeness = (1 / (luminosityFactor * (transform.position - lineTarget.transform.position).magnitude + 1));

                metalLines[lines].StartPos = metalLinesAnchor.position;
                metalLines[lines].EndPos = objectToTarget.CenterOfMass;
                //if (searchingForTargets)
                //    metalLines[lines].LightSaberFactor = 1;
                //else
                //metalLines[lines].LightSaberFactor = (objectToTarget == (pullTarget) || objectToTarget == (pushTarget)) ? lightSaberTargetingFactor : 1;

                metalLines[lines].LineWidth = startWidth * Mathf.Pow(objectToTarget.Mass / rb.mass, .125f);

                if (IsTarget(objectToTarget, iron)) {
                    //if(objectToTarget.LightSaberFactor == 1) {
                    //    Debug.Log(objectToTarget.LightSaberFactor + " pre");
                    //    Debug.Log(objectToTarget.LastNetAllomanticForceOnAllomancer.magnitude + "force");
                    //    Debug.Log(Mathf.Lerp(objectToTarget.LightSaberFactor, lightSaberConstant / (lightSaberConstant + (objectToTarget.LastNetAllomanticForceOnAllomancer).magnitude), MetalLinesLerpConstant) + " new");
                    //}
                    objectToTarget.LightSaberFactor = Mathf.Lerp(objectToTarget.LightSaberFactor, lightSaberConstant / (lightSaberConstant + (objectToTarget.LastNetAllomanticForceOnAllomancer).magnitude), MetalLinesLerpConstant);
                    metalLines[lines].LightSaberFactor = objectToTarget.LightSaberFactor;
                    
                    metalLines[lines].LineColor = new Color(0, closeness * gMaxLines, 255);
                } else if(IsTarget(objectToTarget, steel)) { // if this line is being pushed on
                                                             // Metal is a steel target
                    metalLines[lines].LineColor = new Color(255, closeness * gMaxLines, 0);
                    objectToTarget.LightSaberFactor = Mathf.Lerp(objectToTarget.LightSaberFactor, lightSaberConstant / (lightSaberConstant + (objectToTarget.LastNetAllomanticForceOnAllomancer).magnitude), MetalLinesLerpConstant);
                    metalLines[lines].LightSaberFactor = objectToTarget.LightSaberFactor;
                } else {
                    // Metal is not a target
                    metalLines[lines].LineColor = new Color(0, closeness * gMaxLines, closeness * bMaxLines);
                    metalLines[lines].LightSaberFactor = 1;
                }

                lines++;
            } else {
                nearbyMetals[colIndex] = null;
            }
            colIndex++;
        }
        // disable all of the remaining lines that used to have targets but are now unused
        while (lines < metalLines.Count) {
            metalLines[lines].GetComponent<MeshRenderer>().enabled = false;
            lines++;
        }

        // Set the most ideal pullTarget to be your actual pullTarget
        //if (highlightTarget && searchingForTargets && centerestObject != null) {
        //    metalLines[centerestObjectIndex].LightSaberFactor = lightSaberTargetingFactor;
        //}
        return centerestObject;
    }

    private void AddTargetGlow(Magnetic target) {
        if (target != null) {
            Renderer targetRenderer;
            Material[] mats;
            Material[] temp;
            // add glowing of new pullTarget
            targetRenderer = target.GetComponent<Renderer>();
            temp = targetRenderer.materials;
            mats = new Material[temp.Length + 1];
            for (int i = 0; i < temp.Length; i++) {
                mats[i] = temp[i];
            }

            mats[mats.Length - 1] = ironpullTargetedMaterial;
            targetRenderer.materials = mats;
        }
    }

    private void RemoveTargetGlow(Magnetic target) {
        if (target != null) {
            Renderer targetRenderer;
            Material[] mats;
            Material[] temp;

            // remove glowing of old target
            targetRenderer = target.GetComponent<Renderer>();
            temp = targetRenderer.materials;
            if (temp.Length > 1) {
                mats = new Material[temp.Length - 1];
                mats[0] = temp[0];
                for (int i = 1; i < mats.Length; i++) {
                    if (temp[i].name == "IronpullOutline (Instance)") {
                        for (int j = i; j < mats.Length; j++) {
                            mats[j] = temp[j + 1];
                        }
                        break;
                    } else {
                        mats[i] = temp[i];
                    }
                }
                targetRenderer.materials = mats;
            }
        }
    }

    private void RemoveTarget(int index, bool ironTarget) {
        lastAllomancerVelocity = Vector3.zero;
        lastExpectedAllomancerAcceleration = Vector3.zero;

        if (ironTarget) {

            if (HasPullTarget && index < pullCount) {
                pullTargets[index].Clear();
                for (int i = index; i < pullCount - 1; i++) {
                    pullTargets[i] = pullTargets[i + 1];
                }
                pullCount--;
                pullTargets[pullCount] = null;
            }
        } else {

            if (HasPushTarget && index < pushCount) {
                pushTargets[index].Clear();
                for (int i = index; i < pushCount - 1; i++) {
                    pushTargets[i] = pushTargets[i + 1];
                }
                pushCount--;
                pushTargets[pushCount] = null;
            }
        }
    }

    public void RemoveTarget(Magnetic target, bool ironTarget, bool searchBoth = false) {
        lastAllomancerVelocity = Vector3.zero;
        lastExpectedAllomancerAcceleration = Vector3.zero;

        if (ironTarget || searchBoth) {

            if (HasPullTarget) {
                for (int i = 0; i < pullCount; i++) {
                    if (pullTargets[i] == target) { // Magnetic was found, move targets along
                        for (int j = i; j < pullCount - 1; j++) {
                            pullTargets[j] = pullTargets[j + 1];
                        }
                        pullCount--;
                        pullTargets[pullCount].Clear();
                        pullTargets[pullCount] = null;
                        break;
                    }
                }
            }
        }
        if (!ironTarget || searchBoth) {

            if (HasPushTarget) {
                for (int i = 0; i < pushCount; i++) {
                    if (pushTargets[i] == target) { // Magnetic was found, move targets along
                        for (int j = i; j < pushCount - 1; j++) {
                            pushTargets[j] = pushTargets[j + 1];
                        }
                        pushCount--;
                        pushTargets[pushCount].Clear();
                        pushTargets[pushCount] = null;
                        return;
                    }
                }
            }
        }
    }

    private void RemoveAllTargets() {
        for (int i = 0; i < pullCount; i++) {
            pullTargets[i].Clear();
        }
        for (int i = 0; i < pushCount; i++) {
            pushTargets[i].Clear();
        }
        pullTargets = new Magnetic[maxNumberOfTargets];
        pushTargets = new Magnetic[maxNumberOfTargets];
        pullCount = 0;
        pushCount = 0;
    }

    public void AddTarget(Magnetic newTarget, bool usingIron) {
        StartBurningIronSteel();
        if (newTarget != null) {
            newTarget.Allomancer = this;
            newTarget.LastWasPulled = usingIron;
            if (availableNumberOfTargets != 0) {
                if (usingIron) {
                    // Begin iterating through the array
                    // Check if target is already in the array
                    //      if so, remove old version of the target and put the new one on the end
                    // If size == length, remove oldest target, add newest target
                    for (int i = 0; i < availableNumberOfTargets; i++) {
                        if (pullTargets[i] == null) { // empty space found, add target here
                            pullTargets[i] = newTarget;
                            pullCount++;
                            return;
                        }
                        // this space in the array is taken.
                        if (pullTargets[i] == newTarget) { // newTarget is already in the array. Remove it, then continue adding this target to the end.
                            for (int j = i; j < pullCount - 1; j++) {
                                pullTargets[j] = pullTargets[j + 1];
                            }
                            pullTargets[pullCount - 1] = newTarget;
                            return;
                        }
                        // An irrelevant target was iterated through.
                    }
                    // Array was iterated through and no space was found. Remove oldest target, push targets along, and add new one.
                    pullTargets[0].Clear();
                    for (int i = 0; i < availableNumberOfTargets - 1; i++) {
                        pullTargets[i] = pullTargets[i + 1];
                    }
                    pullTargets[availableNumberOfTargets - 1] = newTarget;
                    return;

                } else {
                    // Begin iterating through the array
                    // Check if target is already in the array
                    //      if so, remove old version of the target and put the new one on the end
                    // If size == length, remove oldest target, add newest target
                    for (int i = 0; i < availableNumberOfTargets; i++) {
                        if (pushTargets[i] == null) { // empty space found, add target here
                            pushTargets[i] = newTarget;
                            pushCount++;
                            return;
                        }
                        // this space in the array is taken.
                        if (pushTargets[i] == newTarget) { // newTarget is already in the array. Remove it, then continue adding this target to the end.
                            for (int j = i; j < pushCount - 1; j++) {
                                pushTargets[j] = pushTargets[j + 1];
                            }
                            pushTargets[pushCount - 1] = newTarget;
                            return;
                        }
                        // An irrelevant target was iterated through.
                    }
                    // Code is only reachable here if Array was iterated through and no target was added. Remove oldest target, push targets along, and add new one.
                    pushTargets[0].Clear();
                    for (int i = 0; i < availableNumberOfTargets - 1; i++) {
                        pushTargets[i] = pushTargets[i + 1];
                    }
                    pushTargets[availableNumberOfTargets - 1] = newTarget;
                    return;
                }
            }
        }
    }

    private void StartBurningIronSteel() {
        if (!IsBurningIronSteel) { // just started burning metal
            gamepad.Shake(.1f, .1f, .3f);
            IsBurningIronSteel = true;
            RefreshBurnRateMeter();
            metalLineText.text = availableNumberOfTargets.ToString();
        }
    }

    private void StopBurningIronSteel() {
        //if (IsBurningIronSteel) {
        RemoveTargetGlow(lastHoveredOverTarget);
        IsBurningIronSteel = false;
        metalLineText.text = "";
        burnRateMeterPercent.text = "";
        burnRateImage.fillAmount = 0;
        RemoveAllTargets();

        gamepad.SetRumble(0, 0);

        // make blue lines disappear
        for (int i = 0; i < metalLines.Count; i++) {
            metalLines[i].GetComponent<MeshRenderer>().enabled = false;
        }
        //}
    }

    private void SetPullRate(float rate) {
        if(rate > .01f) {
            ironBurnRate = rate;
            if (HasPullTarget || HasPushTarget)
                gamepad.SetRumble(steelBurnRate, ironBurnRate);
        } else {
            //IronPulling = false;
            ironBurnRate = 0;
            gamepad.SetRumbleRight(0);
        }
    }

    private void SetPushRate(float rate) {
        if (rate > .01f) {
            steelBurnRate = rate;
            if (HasPullTarget || HasPushTarget)
                gamepad.SetRumble(steelBurnRate, ironBurnRate);
        } else {
            //SteelPushing = false;
            steelBurnRate = 0;
            gamepad.SetRumbleLeft(0);
        }
    }

    public bool IsTarget(Magnetic potentialTarget) {
        for (int i = 0; i < pullCount; i++) {
            if (potentialTarget == pullTargets[i])
                return true;
        }
        for (int i = 0; i < pushCount; i++) {
            if (potentialTarget == pushTargets[i])
                return true;
        }
        return false;
    }

    private bool IsTarget(Magnetic potentialTarget, bool ironTarget) {
        if (ironTarget)
            for (int i = 0; i < pullCount; i++) {
                if (potentialTarget == pullTargets[i])
                    return true;
            } else
            for (int i = 0; i < pushCount; i++) {
                if (potentialTarget == pushTargets[i])
                    return true;
            }
        return false;
    }

    private void SwapPullPushTargets() {
        Magnetic[] tempArray = pullTargets;
        pullTargets = pushTargets;
        pushTargets = tempArray;
        int tempSize = pullCount;
        pullCount = pushCount;
        pushCount = tempSize;

        for (int i = 0; i < pullCount; i++) {
            pullTargets[i].LastWasPulled = true;
        }
        for (int i = 0; i < pushCount; i++) {
            pushTargets[i].LastWasPulled = false;
        }
    }

    private void SetAllTargetsOutOfRange() {
        for (int i = 0; i < pullCount; i++) {
            pullTargets[i].InRange = false;
        }
        for (int i = 0; i < pushCount; i++) {
            pushTargets[i].InRange = false;
        }
    }

    private void RemoveAllOutOfRangeTargets() {
        for (int i = 0; i < pullCount; i++) {
            if (!pullTargets[i].InRange) {
                RemoveTarget(i, iron);
            }
        }
        for (int i = 0; i < pushCount; i++) {
            if (!pushTargets[i].InRange) {
                RemoveTarget(i, steel);
            }
        }
    }

    private void IncrementNumberOfTargets() {
        if (availableNumberOfTargets < maxNumberOfTargets) {
            availableNumberOfTargets++;
            metalLineText.text = availableNumberOfTargets.ToString();
        }
    }

    private void DecrementNumberOfTargets() {
        if (availableNumberOfTargets > 0) {
            availableNumberOfTargets--;
            if (pullCount > availableNumberOfTargets)
                RemoveTarget(0, iron);
            if (pushCount > availableNumberOfTargets)
                RemoveTarget(0, steel);
            // never actually have 0 available targets. Just remove targets, and stay at 1 available targets.
            if (availableNumberOfTargets == 0) {
                availableNumberOfTargets++;
            } else {
                metalLineText.text = availableNumberOfTargets.ToString();
            }

        }
    }

    private void ChangeBurnRate(float change) {
        if(change > 0) {
            change = .1f;
        } else if(change < 0) {
            change = -.1f;
        }
        ironBurnRate = Mathf.Clamp(ironBurnRate + change, 0, 1);
        steelBurnRate = Mathf.Clamp(steelBurnRate + change, 0, 1);
    }

    private void RefreshBurnRateMeter() {
        float rate = Mathf.Max(ironBurnRate, steelBurnRate);
        int percent = (int) Mathf.Round(rate * 100);
        if(percent == 0) {
            burnRateMeterPercent.text = "";
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 0, burnRateMeterLerpConstant);
        } else if(percent > 99) {
            burnRateMeterPercent.text = "MAX";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 1, burnRateMeterLerpConstant);
        } else {
            burnRateMeterPercent.text = percent + "%";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, minAngle + (rate) * (maxAngle), burnRateMeterLerpConstant); 
        }
    }
}
