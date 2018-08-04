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
    private const float closenessThreshold = .01f;
    //private readonly Vector3 centerOfScreen = new Vector3(.5f, .5f, 0);
    // Constants for Metal Lines
    private const float startWidth = .05f;
    private const float horizontalMin = .45f;
    private const float horizontalMax = .55f;
    private const float verticalMin = .2f;
    private const float verticalMax = .8f;
    private const float gMaxLines = .15f;
    private const float bMaxLines = 1;
    private const float luminosityFactor = .4f;
    private const float MetalLinesLerpConstant = .30f;
    private const float verticalImportanceFactor = 100f;
    private const float lightSaberConstant = 1000f;
    public const int maxNumberOfTargets = 10;
    public static float AllomanticConstant { get; set; } = 1200;
    public static float maxRange = 75;
    public const float chargePower = 1f / 8f;

    // Button-press time constants
    private const float timeToHoldDown = .5f;
    private const float timeDoubleTapWindow = .5f;

    // Simple metal booleans for passing to methods
    private const bool steel = false;
    private const bool iron = true;

    //private LayerMask ignorePlayerLayer;
    private Camera firstPersonView;
    private GamepadController gamepad;
    private Rigidbody rb;
    private List<VolumetricLineBehavior> metalLines;
    private VolumetricLineBehavior metalLineTemplate;
    [SerializeField]
    private Material ironpullTargetedMaterial;
    [SerializeField]
    private Transform metalLinesAnchor;
    [SerializeField]
    private Transform centerOfMass;

    private bool HasPullTarget {
        get {
            return PullCount != 0; ;
        }
    }
    private bool HasPushTarget {
        get {
            return PushCount != 0;
        }
    }
    private Vector3 CenterOfMass {
        get {
            return centerOfMass.position;
        }
    }
    // Button held-down times
    private float timeToStopBurning = 0f;
    private float timeToSwapBurning = 0f;

    // Currently hovered-over Magnetic
    private Magnetic lastHoveredOverTarget = null;

    // Checks if targets were cleared after stopping pulling/pushing
    //private bool justClearedIron = false;
    //private bool justClearedSteel = false;

    // Magnetic variables
    public int AvailableNumberOfTargets { get; private set; } = 1;
    public int PullCount { get; private set; } = 0;
    public int PushCount { get; private set; } = 0;

    private Magnetic[] pullTargets;
    private Magnetic[] pushTargets;

    // Used for calculating the acceleration over the last frame for pushing/pulling
    private Vector3 lastAllomancerVelocity = Vector3.zero;

    private Vector3 lastExpectedAllomancerAcceleration = Vector3.zero;
    private Vector3 thisFrameExpectedAllomancerAcceleration = Vector3.zero;
    //private Vector3 lastNormalForce = Vector3.zero;
    //private Vector3 thisFrameNormalForce = Vector3.zero;
    //private Vector3 lastAllomanticForce = Vector3.zero;
    //private Vector3 thisFrameAllomanticForce = Vector3.zero;

    private Vector3 thisFrameMaximumAllomanticForce = Vector3.zero;
    private Vector3 lastMaximumAllomanticForce = Vector3.zero;
    private Vector3 thisFrameMaximumNormalForce = Vector3.zero;
    private Vector3 lastMaximumNormalForce = Vector3.zero;

    // Used for burning metals
    private float ironBurnRate = 0;
    private float steelBurnRate = 0;

    private float forceMagnitudeTarget = 600;

    public bool IronPulling { get; private set; } = false;
    public bool SteelPushing { get; private set; } = false;
    public bool IsBurningIronSteel { get; private set; } = false;
    public float Mass {
        get {
            return rb.mass;
        }
    }

    private void Awake() {
        //ignorePlayerLayer = ~(1 << LayerMask.NameToLayer("Player"));
        firstPersonView = GetComponentInChildren<Camera>();
        rb = GetComponent<Rigidbody>();
        gamepad = GameObject.FindGameObjectWithTag("GameController").GetComponent<GamepadController>();
        metalLines = new List<VolumetricLineBehavior>();
        metalLineTemplate = GetComponentInChildren<VolumetricLineBehavior>();
        centerOfMass.localPosition = Vector3.zero;
        metalLinesAnchor.localPosition = centerOfMass.localPosition;
    }

    public void Clear() {
        IsBurningIronSteel = false;
        IronPulling = false;
        SteelPushing = false;
        ironBurnRate = 0;
        steelBurnRate = 0;
        PullCount = 0;
        PushCount = 0;
        pullTargets = new Magnetic[maxNumberOfTargets];
        pushTargets = new Magnetic[maxNumberOfTargets];
        HUD.TargetOverlayController.SetTargets(pullTargets, pushTargets);
        metalLines = new List<VolumetricLineBehavior>();
        lastHoveredOverTarget = null;
        lastExpectedAllomancerAcceleration = Vector3.zero;
        //lastAllomanticForce = Vector3.zero;
        //lastNormalForce = Vector3.zero;
        lastMaximumAllomanticForce = Vector3.zero;
        lastMaximumNormalForce = Vector3.zero;
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
                    if (GamepadController.currentForceStyle == ForceStyle.Percentage)
                        ChangeTargePercent(Keybinds.ScrollWheelAxis());
                    else
                        ChangeTargetForceMagnitude(Keybinds.ScrollWheelAxis());
                }
            } else {
                float scrollValue = Keybinds.ScrollWheelAxis();
                if (scrollValue > 0) {
                    IncrementNumberOfTargets();
                }
                if (scrollValue < 0) {
                    DecrementNumberOfTargets();
                }
            }

            IronPulling = Keybinds.IronPulling();
            SteelPushing = Keybinds.SteelPushing();

            // If just stopped pulling/pushing on targets, clear their values
            //if (IronPulling)
            //    justClearedIron = false;
            //else if(!justClearedIron) {
            //    for (int i = 0; i < PullCount; i++)
            //        pullTargets[i].SoftClear();
            //    Debug.Log("Cleared iroon");
            //    justClearedIron = true;
            //}
            //if (SteelPushing)
            //    justClearedSteel = false;
            //else if (!justClearedSteel) {
            //    for (int i = 0; i < PushCount; i++)
            //        pushTargets[i].SoftClear();
            //    Debug.Log("Cleared steel");
            //    justClearedSteel = true;
            //}


            // Check input for target selection
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
            // Stop burning altogether, hide metal lines
            if (Keybinds.Negate()) {
                timeToStopBurning += Time.deltaTime;
                if (Keybinds.Select() && Keybinds.SelectAlternate() && timeToStopBurning > timeToHoldDown) {
                    StopBurningIronSteel();
                    timeToStopBurning = 0;
                }
            } else {
                timeToStopBurning = 0;
            }
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

        // Set burn rates
        if (IsBurningIronSteel) {
            if (GamepadController.currentForceStyle == ForceStyle.Percentage) {
                if (GamepadController.UsingGamepad) {
                    ironBurnRate = Keybinds.RightBurnRate();
                    steelBurnRate = Keybinds.LeftBurnRate();
                }
                SetPullRate(ironBurnRate);
                SetPushRate(steelBurnRate);
            } else {
                float maxNetForce = (lastMaximumAllomanticForce + lastMaximumNormalForce).magnitude;
                SetPullRate(forceMagnitudeTarget / maxNetForce);
                SetPushRate(forceMagnitudeTarget / maxNetForce);
            }
            UpdateBurnRateMeter();
        }

        if (IsBurningIronSteel && !searchingForTarget) { // not searching for a new target but still burning passively, show lines
            SearchForMetals(searchingForTarget);
        }
    }

    private void FixedUpdate() {
        if (IsBurningIronSteel) {
            CalculatePushPullForces(iron);
            CalculatePushPullForces(steel);

            if (HasPullTarget) { // if has pull targets...
                if (HasPushTarget) { // has push targets AND pull targets
                    if (IronPulling) {
                        PullPushOnTargets(iron, iron);
                    }
                    if (SteelPushing) {
                        PullPushOnTargets(steel, steel);
                    }
                } else { // has pull targets and NO push targets
                    if (IronPulling) {
                        PullPushOnTargets(iron, iron);
                    } else {
                        if (SteelPushing) {
                            PullPushOnTargets(steel, iron);
                        }
                    }
                }
            } else { // has no pull targets
                if (HasPushTarget) { // has push targets and NO pull targets
                    if (IronPulling) {
                        PullPushOnTargets(iron, steel);
                    } else {
                        if (SteelPushing) {
                            PullPushOnTargets(steel, steel);
                        }
                    }
                }
            }
            lastAllomancerVelocity = rb.velocity;

            // Debug
            lastExpectedAllomancerAcceleration = thisFrameExpectedAllomancerAcceleration;
            //lastAllomanticForce = thisFrameAllomanticForce;
            //lastNormalForce = thisFrameNormalForce;
            lastMaximumAllomanticForce = thisFrameMaximumAllomanticForce;
            lastMaximumNormalForce = thisFrameMaximumNormalForce;
            thisFrameExpectedAllomancerAcceleration = Vector3.zero;
            //thisFrameAllomanticForce = Vector3.zero;
            //thisFrameNormalForce = Vector3.zero;
            thisFrameMaximumAllomanticForce = Vector3.zero;
            thisFrameMaximumNormalForce = Vector3.zero;
        }
    }

    private void CalculatePushPullForces(bool usingIronTargets) {
        if (usingIronTargets)
            for (int i = 0; i < PullCount; i++) {
                CalculateForce(pullTargets[i], iron);
            } else
            for (int i = 0; i < PushCount; i++) {
                CalculateForce(pushTargets[i], steel);
            }
    }

    private void PullPushOnTargets(bool pulling, bool usingIronTargets) {
        if (usingIronTargets) {
            for (int i = 0; i < PullCount; i++) {
                AddForce(pullTargets[i], pulling);
            }
        } else {
            for (int i = 0; i < PushCount; i++) {
                AddForce(pushTargets[i], pulling);
            }
        }
    }

    //private void ResetPullStatus(bool usingIronTargets) {
    //    if (usingIronTargets) {
    //        for (int i = 0; i < PullCount; i++) {
    //            pullTargets[i].LastWasPulled = true;
    //        }
    //    } else {
    //        for (int i = 0; i < PushCount; i++) {
    //            pushTargets[i].LastWasPulled = false;
    //        }
    //    }
    //}

    // Debug
    public float allomanticsForce;
    public float netAllomancersForce;
    public float netTargetsForce;
    public Vector3 allomanticsForces;
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
    private void CalculateForce(Magnetic target, bool usingIronTargets) {
        Vector3 positionDifference = target.CenterOfMass - CenterOfMass;
        // If the target is extremely close to the player, prevent the distance from being so low that the force approaches infinity
        float distance = Mathf.Max(positionDifference.magnitude, closenessThreshold);


        Vector3 distanceFactor;
        switch (PhysicsController.distanceRelationshipMode) {
            case ForceDistanceRelationship.InverseSquareLaw: {
                    distanceFactor = (positionDifference / distance / distance);
                    break;
                }
            case ForceDistanceRelationship.Linear: {
                    distanceFactor = positionDifference.normalized * (1 - positionDifference.magnitude / maxRange);
                    break;
                }
            default: {
                    distanceFactor = positionDifference.normalized * Mathf.Exp(-distance / PhysicsController.distanceConstant);
                    break;
                }
        }

        Vector3 allomanticForce = AllomanticConstant * Mathf.Pow(target.Mass * rb.mass, chargePower) * distanceFactor / (usingIronTargets ? PullCount : PushCount) * (target.LastWasPulled ? 1 : -1);

        // If controlling the strength of the push by Percentage of the maximum possible push
        if (GamepadController.currentForceStyle == ForceStyle.Percentage) {
            allomanticForce *= (target.LastWasPulled ? ironBurnRate : steelBurnRate);
        }
        thisFrameMaximumAllomanticForce += allomanticForce;

        Vector3 restitutionForceFromTarget;
        Vector3 restitutionForceFromAllomancer;
        if ((IronPulling && target.LastWasPulled) || (SteelPushing && !target.LastWasPulled)) { //If pushing or pulling, ANF should be added to calculation
            switch (PhysicsController.anchorBoostMode) {
                case AnchorBoostMode.AllomanticNormalForce: {
                        if (target.IsStatic) {
                            // If the target has no rigidbody, let the let the restitution force equal the allomantic force. It's a perfect anchor.
                            // Thus:
                            // a push against a perfectly anchored metal structure is exactly twice as powerful as a push against a completely unanchored, freely-moving metal structure
                            restitutionForceFromTarget = allomanticForce;
                            restitutionForceFromAllomancer = Vector3.zero; // irrelevant
                        } else {
                            // Calculate Allomantic Normal Forces

                            if (target.IsPerfectlyAnchored) { // If target is perfectly anchored, its ANF = AF.
                                restitutionForceFromTarget = allomanticForce;
                            } else { // Target is partially anchored
                                     //calculate changes from the last frame

                                // sign is swapping when pushing/pulling. good?.

                                Vector3 newTargetVelocity = target.Rb.velocity;
                                Vector3 lastTargetAcceleration = (newTargetVelocity - target.LastVelocity) / Time.fixedDeltaTime;
                                Vector3 unaccountedForTargetAcceleration = lastTargetAcceleration - target.LastExpectedAcceleration;// + Physics.gravity;
                                restitutionForceFromTarget = Vector3.Project(unaccountedForTargetAcceleration * target.Mass, positionDifference.normalized);
                                //Debug.Log(lastTargetAcceleration);
                            }

                            // sign is not swapping when pushing/pulling. it should. BAD.

                            Vector3 newAllomancerVelocity = rb.velocity;
                            Vector3 lastAllomancerAcceleration = (newAllomancerVelocity - lastAllomancerVelocity) / Time.fixedDeltaTime;
                            Vector3 unaccountedForAllomancerAcceleration = lastAllomancerAcceleration - lastExpectedAllomancerAcceleration;
                            //Debug.Log(lastAllomancerAcceleration);
                            //if (!movementController.IsGrounded) {
                            //    unaccountedForAllomancerAcceleration += Physics.gravity;
                            //}
                            restitutionForceFromAllomancer = Vector3.Project(unaccountedForAllomancerAcceleration * rb.mass, positionDifference.normalized);

                            if (PhysicsController.normalForceMaximum == NormalForceMaximum.AllomanticForce) {
                                restitutionForceFromTarget = Vector3.ClampMagnitude(restitutionForceFromTarget, allomanticForce.magnitude);
                                restitutionForceFromAllomancer = Vector3.ClampMagnitude(restitutionForceFromAllomancer, allomanticForce.magnitude);
                            }

                            // using Impulse strategy
                            //restitutionForceFromTarget = Vector3.ClampMagnitude(Vector3.Project(target.forceFromCollisionTotal, positionDifference.normalized), allomanticForce.magnitude);

                            target.LastPosition = target.transform.position;
                            target.LastVelocity = target.Rb.velocity;
                        }

                        // Prevents the ANF from being negative relative to the AF and prevents the ANF from ever decreasing the AF below its original value
                        switch (PhysicsController.normalForceMinimum) {
                            case NormalForceMinimum.Zero: {
                                    if (Vector3.Dot(restitutionForceFromAllomancer, allomanticForce) > 0) {
                                        restitutionForceFromAllomancer = Vector3.zero;
                                    }
                                    if (Vector3.Dot(restitutionForceFromTarget, allomanticForce) < 0) {
                                        restitutionForceFromTarget = Vector3.zero;
                                    }
                                    break;
                                }
                            case NormalForceMinimum.ZeroButNegate: {
                                    if (Vector3.Dot(restitutionForceFromAllomancer, allomanticForce) > 0) {
                                        restitutionForceFromAllomancer = -restitutionForceFromAllomancer;
                                    }
                                    if (Vector3.Dot(restitutionForceFromTarget, allomanticForce) < 0) {
                                        restitutionForceFromTarget = -restitutionForceFromTarget;
                                    }
                                    break;
                                }
                            default: break;
                        }

                        break;
                    }
                case AnchorBoostMode.ExponentialWithVelocity: {
                        // The restitutionForceFromTarget is actually negative, rather than positive, unlike in ANF mode. It contains the percentage of the AF that is subtracted from the AF to get the net AF.
                        float velocityFactorTarget;
                        float velocityFactorAllomancer;
                        if (target.IsStatic) {
                            velocityFactorTarget = 0;
                            velocityFactorAllomancer = 0;
                        } else {
                            velocityFactorTarget = 1 - Mathf.Exp(-(Vector3.Project(target.Velocity, positionDifference.normalized).magnitude / PhysicsController.velocityConstant));
                            velocityFactorAllomancer = 1 - Mathf.Exp(-(Vector3.Project(rb.velocity, positionDifference.normalized).magnitude / PhysicsController.velocityConstant));

                            if (PhysicsController.exponentialWithVelocityMinimum == ExponentialWithVelocityMinimum.BackwardsDecreasesAndForwardsIncreasesForce) {
                                if (Vector3.Dot(rb.velocity, positionDifference) > 0) {
                                    velocityFactorTarget *= -1;
                                }
                                if (Vector3.Dot(target.Velocity, positionDifference) < 0) {
                                    velocityFactorAllomancer *= -1;
                                }
                            } else if (PhysicsController.exponentialWithVelocityMinimum == ExponentialWithVelocityMinimum.OnlyBackwardsDecreasesForce) {
                                if (Vector3.Dot(rb.velocity, positionDifference) > 0) {
                                    velocityFactorTarget *= 0;
                                }
                                if (Vector3.Dot(target.Velocity, positionDifference) < 0) {
                                    velocityFactorAllomancer *= 0;
                                }
                            }
                        }

                        restitutionForceFromAllomancer = allomanticForce * velocityFactorAllomancer;
                        restitutionForceFromTarget = allomanticForce * -velocityFactorTarget;

                        break;
                    }
                default: {
                        restitutionForceFromTarget = Vector3.zero;
                        restitutionForceFromAllomancer = Vector3.zero;
                        break;
                    }
            }
        } else {
            // Not pushing or pulling, do not try to add any force modifications
            restitutionForceFromAllomancer = Vector3.zero;
            restitutionForceFromTarget = Vector3.zero;
        }
        thisFrameMaximumNormalForce += restitutionForceFromTarget;

        // If controlling the push strength by using a target force to push with
        if (GamepadController.currentForceStyle == ForceStyle.ForceMagnitude) {
            float percent = forceMagnitudeTarget / (allomanticForce + restitutionForceFromTarget).magnitude;
            if (percent < 1f) {
                allomanticForce *= percent;
                restitutionForceFromTarget *= percent;
                restitutionForceFromAllomancer *= percent;
            }
        }
        target.LastAllomanticForce = allomanticForce;
        target.LastAllomanticNormalForceFromAllomancer = restitutionForceFromAllomancer;
        target.LastAllomanticNormalForceFromTarget = restitutionForceFromTarget;

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

        target.AddForce(target.LastNetAllomanticForceOnTarget, target.LastAllomanticForceOnTarget, ForceMode.Force);
        // apply force to player
        rb.AddForce(target.LastNetAllomanticForceOnAllomancer, ForceMode.Force);

        //target.Rb.AddForce(targetVelocity, ForceMode.VelocityChange);

        //rb.AddForce(allomancerVelocity, ForceMode.VelocityChange);

        // set up for next frame
        //lastExpectedNormalTargetAcceleration = -restitutionForceFromAllomancer / target.Mass * Time.fixedDeltaTime;
        //lastAllomancerVelocity = rb.velocity;
        thisFrameExpectedAllomancerAcceleration += target.LastAllomanticForceOnAllomancer / rb.mass;
        //thisFrameNormalForce += target.LastAllomanticNormalForceFromTarget;
        //thisFrameAllomanticForce += target.LastAllomanticForce;

        //lastExpectedNormalAllomancerAcceleration = restitutionForceFromTarget / rb.mass * Time.fixedDeltaTime;

        // Debug
        allomanticsForce = target.LastAllomanticForce.magnitude;
        allomanticsForces = target.LastAllomanticForce;
        netAllomancersForce = target.LastNetAllomanticForceOnAllomancer.magnitude;
        resititutionFromTargetsForce = target.LastAllomanticNormalForceFromTarget;
        resititutionFromPlayersForce = target.LastAllomanticNormalForceFromAllomancer;
        percentOfTargetForceReturned = resititutionFromTargetsForce.magnitude / allomanticsForce;
        percentOfAllomancerForceReturned = resititutionFromPlayersForce.magnitude / allomanticsForce;
        netTargetsForce = target.LastNetAllomanticForceOnTarget.magnitude;
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
        float centerestDistanceFromCenter = 1f;
        Magnetic centerestObject = null;
        Collider[] nearbyMetals = Physics.OverlapSphere(CenterOfMass, maxRange);

        int lines = 0;
        int colIndex = 0;
        while (colIndex < nearbyMetals.Length) {
            Magnetic objectToTarget = nearbyMetals[colIndex].GetComponent<Magnetic>();
            if (objectToTarget != null) { // If object is magnetic
                objectToTarget.InRange = true;
                if (searchingForTargets) {
                    // If searching for a pullTarget, calculate the object's position on screen.
                    Vector3 screenPosition = firstPersonView.WorldToViewportPoint(objectToTarget.transform.position);
                    if (screenPosition.z > 0 && screenPosition.x > horizontalMin && screenPosition.x < horizontalMax && screenPosition.y > verticalMin && screenPosition.y < verticalMax) {
                        // Test if the new object is the more ideal pullTarget than the last most ideal pullTarget
                        float distanceFromCenter = verticalImportanceFactor * Mathf.Pow(screenPosition.x - .5f, 2) + Mathf.Pow(screenPosition.y - .5f, 2);
                        if (distanceFromCenter < centerestDistanceFromCenter) {
                            centerestDistanceFromCenter = distanceFromCenter;
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
                    metalLines[lines].LineColor = new Color(0, 1, gMaxLines, 1);
                } else if (IsTarget(objectToTarget, steel)) { // if this line is being pushed on
                                                              // Metal is a steel target
                    metalLines[lines].LineColor = new Color(1, gMaxLines, 0, 1);
                    objectToTarget.LightSaberFactor = Mathf.Lerp(objectToTarget.LightSaberFactor, lightSaberConstant / (lightSaberConstant + (objectToTarget.LastNetAllomanticForceOnAllomancer).magnitude), MetalLinesLerpConstant);
                    metalLines[lines].LightSaberFactor = objectToTarget.LightSaberFactor;
                } else {
                    // Metal is not a target
                    metalLines[lines].LineColor = new Color(0, closeness * gMaxLines, closeness * bMaxLines, 1);
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

            if (HasPullTarget && index < PullCount) {
                pullTargets[index].Clear();
                for (int i = index; i < PullCount - 1; i++) {
                    pullTargets[i] = pullTargets[i + 1];
                }
                PullCount--;
                pullTargets[PullCount] = null;
            }
        } else {

            if (HasPushTarget && index < PushCount) {
                pushTargets[index].Clear();
                for (int i = index; i < PushCount - 1; i++) {
                    pushTargets[i] = pushTargets[i + 1];
                }
                PushCount--;
                pushTargets[PushCount] = null;
            }
        }

        HUD.TargetOverlayController.HardRefresh();
    }

    public void RemoveTarget(Magnetic target, bool ironTarget, bool searchBoth = false) {
        lastAllomancerVelocity = Vector3.zero;
        lastExpectedAllomancerAcceleration = Vector3.zero;

        if (ironTarget || searchBoth) {

            if (HasPullTarget) {
                for (int i = 0; i < PullCount; i++) {
                    if (pullTargets[i] == target) { // Magnetic was found, move targets along
                        for (int j = i; j < PullCount - 1; j++) {
                            pullTargets[j] = pullTargets[j + 1];
                        }
                        PullCount--;
                        pullTargets[PullCount].Clear();
                        pullTargets[PullCount] = null;

                        if (!searchBoth) {
                            HUD.TargetOverlayController.HardRefresh();
                            return;
                        }
                        break;
                    }
                }
            }
        }
        if (!ironTarget || searchBoth) {

            if (HasPushTarget) {
                for (int i = 0; i < PushCount; i++) {
                    if (pushTargets[i] == target) { // Magnetic was found, move targets along
                        for (int j = i; j < PushCount - 1; j++) {
                            pushTargets[j] = pushTargets[j + 1];
                        }
                        PushCount--;
                        pushTargets[PushCount].Clear();
                        pushTargets[PushCount] = null;

                        HUD.TargetOverlayController.HardRefresh();
                        return;
                    }
                }
            }
        }
    }

    private void RemoveAllTargets() {
        for (int i = 0; i < PullCount; i++) {
            pullTargets[i].Clear();
        }
        for (int i = 0; i < PushCount; i++) {
            pushTargets[i].Clear();
        }
        PullCount = 0;
        PushCount = 0;
        pullTargets = new Magnetic[maxNumberOfTargets];
        pushTargets = new Magnetic[maxNumberOfTargets];

        HUD.TargetOverlayController.SetTargets(pullTargets, pushTargets);
    }

    public void AddTarget(Magnetic newTarget, bool usingIron) {
        StartBurningIronSteel();
        if (newTarget != null) {
            newTarget.Allomancer = this;
            newTarget.LastWasPulled = usingIron;
            if (AvailableNumberOfTargets != 0) {
                if (usingIron) {
                    // Begin iterating through the array
                    // Check if target is already in the array
                    //      if so, remove old version of the target and put the new one on the end
                    // If size == length, remove oldest target, add newest target
                    for (int i = 0; i < AvailableNumberOfTargets; i++) {
                        if (pullTargets[i] == null) { // empty space found, add target here
                            pullTargets[i] = newTarget;
                            PullCount++;

                            HUD.TargetOverlayController.HardRefresh();
                            return;
                        }
                        // this space in the array is taken.
                        if (pullTargets[i] == newTarget) { // newTarget is already in the array. Remove it, then continue adding this target to the end.
                            for (int j = i; j < PullCount - 1; j++) {
                                pullTargets[j] = pullTargets[j + 1];
                            }
                            pullTargets[PullCount - 1] = newTarget;

                            HUD.TargetOverlayController.HardRefresh();
                            return;
                        }
                        // An irrelevant target was iterated through.
                    }
                    // Array was iterated through and no space was found. Remove oldest target, push targets along, and add new one.
                    pullTargets[0].Clear();
                    for (int i = 0; i < AvailableNumberOfTargets - 1; i++) {
                        pullTargets[i] = pullTargets[i + 1];
                    }
                    pullTargets[AvailableNumberOfTargets - 1] = newTarget;

                    HUD.TargetOverlayController.HardRefresh();
                    return;

                } else {
                    // Begin iterating through the array
                    // Check if target is already in the array
                    //      if so, remove old version of the target and put the new one on the end
                    // If size == length, remove oldest target, add newest target
                    for (int i = 0; i < AvailableNumberOfTargets; i++) {
                        if (pushTargets[i] == null) { // empty space found, add target here
                            pushTargets[i] = newTarget;
                            PushCount++;

                            HUD.TargetOverlayController.HardRefresh();
                            return;
                        }
                        // this space in the array is taken.
                        if (pushTargets[i] == newTarget) { // newTarget is already in the array. Remove it, then continue adding this target to the end.
                            for (int j = i; j < PushCount - 1; j++) {
                                pushTargets[j] = pushTargets[j + 1];
                            }
                            pushTargets[PushCount - 1] = newTarget;

                            HUD.TargetOverlayController.HardRefresh();
                            return;
                        }
                        // An irrelevant target was iterated through.
                    }
                    // Code is only reachable here if Array was iterated through and no target was added. Remove oldest target, push targets along, and add new one.
                    pushTargets[0].Clear();
                    for (int i = 0; i < AvailableNumberOfTargets - 1; i++) {
                        pushTargets[i] = pushTargets[i + 1];
                    }
                    pushTargets[AvailableNumberOfTargets - 1] = newTarget;

                    HUD.TargetOverlayController.HardRefresh();
                    return;
                }
            }
        }
    }

    private void StartBurningIronSteel() {
        if (!IsBurningIronSteel) { // just started burning metal
            gamepad.Shake(.1f, .1f, .3f);
            IsBurningIronSteel = true;
            UpdateBurnRateMeter();
            HUD.BurnRateMeter.MetalLineText = AvailableNumberOfTargets.ToString();
        }
    }

    public void StopBurningIronSteel() {
        RemoveAllTargets();
        //if (IsBurningIronSteel) {
        RemoveTargetGlow(lastHoveredOverTarget);
        IsBurningIronSteel = false;
        if (HUD.BurnRateMeter) {
            HUD.BurnRateMeter.Clear();
        }
        if (gamepad)
            gamepad.SetRumble(0, 0);

        // make blue lines disappear
        for (int i = 0; i < metalLines.Count; i++) {
            metalLines[i].GetComponent<MeshRenderer>().enabled = false;
        }
        //}
    }

    private void SetPullRate(float rate) {
        if (rate > .01f) {
            if (GamepadController.currentForceStyle == ForceStyle.Percentage)
                ironBurnRate = rate;
            else
                ironBurnRate = Mathf.Min(1, rate);
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
            if (GamepadController.currentForceStyle == ForceStyle.Percentage)
                steelBurnRate = rate;
            else
                steelBurnRate = Mathf.Min(1, rate);
            if (HasPullTarget || HasPushTarget)
                gamepad.SetRumble(steelBurnRate, ironBurnRate);
        } else {
            //SteelPushing = false;
            steelBurnRate = 0;
            gamepad.SetRumbleLeft(0);
        }
    }

    public bool IsTarget(Magnetic potentialTarget) {
        for (int i = 0; i < PullCount; i++) {
            if (potentialTarget == pullTargets[i])
                return true;
        }
        for (int i = 0; i < PushCount; i++) {
            if (potentialTarget == pushTargets[i])
                return true;
        }
        return false;
    }

    private bool IsTarget(Magnetic potentialTarget, bool ironTarget) {
        if (ironTarget)
            for (int i = 0; i < PullCount; i++) {
                if (potentialTarget == pullTargets[i])
                    return true;
            } else
            for (int i = 0; i < PushCount; i++) {
                if (potentialTarget == pushTargets[i])
                    return true;
            }
        return false;
    }

    private void SwapPullPushTargets() {
        Magnetic[] tempArray = pullTargets;
        pullTargets = pushTargets;
        pushTargets = tempArray;
        int tempSize = PullCount;
        PullCount = PushCount;
        PushCount = tempSize;

        for (int i = 0; i < PullCount; i++) {
            pullTargets[i].LastWasPulled = true;
        }
        for (int i = 0; i < PushCount; i++) {
            pushTargets[i].LastWasPulled = false;
        }

        HUD.TargetOverlayController.SetTargets(pullTargets, pushTargets);
    }

    private void SetAllTargetsOutOfRange() {
        for (int i = 0; i < PullCount; i++) {
            pullTargets[i].InRange = false;
        }
        for (int i = 0; i < PushCount; i++) {
            pushTargets[i].InRange = false;
        }
    }

    private void RemoveAllOutOfRangeTargets() {
        for (int i = 0; i < PullCount; i++) {
            if (!pullTargets[i].InRange) {
                RemoveTarget(i, iron);
            }
        }
        for (int i = 0; i < PushCount; i++) {
            if (!pushTargets[i].InRange) {
                RemoveTarget(i, steel);
            }
        }
    }

    private void IncrementNumberOfTargets() {
        if (AvailableNumberOfTargets < maxNumberOfTargets) {
            AvailableNumberOfTargets++;
            HUD.BurnRateMeter.MetalLineText = AvailableNumberOfTargets.ToString();
        }
    }

    private void DecrementNumberOfTargets() {
        if (AvailableNumberOfTargets > 0) {
            AvailableNumberOfTargets--;
            if (PullCount > AvailableNumberOfTargets)
                RemoveTarget(0, iron);
            if (PushCount > AvailableNumberOfTargets)
                RemoveTarget(0, steel);
            // never actually have 0 available targets. Just remove targets, and stay at 1 available targets.
            if (AvailableNumberOfTargets == 0) {
                AvailableNumberOfTargets++;
            } else {
                HUD.BurnRateMeter.MetalLineText = AvailableNumberOfTargets.ToString();
            }
        }

        HUD.TargetOverlayController.HardRefresh();
    }

    private void ChangeTargetForceMagnitude(float change) {
        if (change > 0) {
            //change = Mathf.Max(10, (lastMaximumAllomanticForce + lastMaximumNormalForce).magnitude / 10f);
            change = 100;
        } else if (change < 0) {
            //change = -Mathf.Max(10, (lastMaximumAllomanticForce + lastMaximumNormalForce).magnitude / 10f);
            change = -100;
        }
        forceMagnitudeTarget = Mathf.Max(0, forceMagnitudeTarget + change);
    }

    private void ChangeTargePercent(float change) {
        if (change > 0) {
            change = .1f;
        } else if (change < 0) {
            change = -.1f;
        }
        ironBurnRate = Mathf.Clamp(ironBurnRate + change, 0, 1);
        steelBurnRate = Mathf.Clamp(steelBurnRate + change, 0, 1);
    }

    private void UpdateBurnRateMeter() {
        if (GamepadController.currentForceStyle == ForceStyle.Percentage)
            HUD.BurnRateMeter.SetBurnRateMeterPercentage(lastMaximumAllomanticForce, lastMaximumNormalForce, Mathf.Max(ironBurnRate, steelBurnRate));
        else
            HUD.BurnRateMeter.SetBurnRateMeterForceMagnitude(lastMaximumAllomanticForce, lastMaximumNormalForce, forceMagnitudeTarget);
        //HUD.BurnRateMeter.SetBurnRateMeterForceMagnitude(lastAllomanticForce, lastNormalForce, forceMagnitudeTarget, lastMaximumAllomanticForce, lastMaximumNormalForce);
    }
}
