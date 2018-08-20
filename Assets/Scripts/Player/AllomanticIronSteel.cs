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
    //private readonly Vector3 centerOfScreen = new Vector3(.5f, .5f, 0);
    // Constants for Metal Lines
    private const float horizontalMin = .45f;
    private const float horizontalMax = .55f;
    private const float verticalMin = .2f;
    private const float verticalMax = .8f;
    private const float gMaxLines = .1f;
    private const float bMaxLines = .85f;
    private const float blueLineWidthConstant = .04f;
    private const float blueLineTargetedWidthConstant = 1.5f;
    private const float blueLineBrightnessConstant = 1 / chargePower;
    private const float blueLineForceCutoff = 100f;
    private const float verticalImportanceFactor = 100f;
    private const float lightSaberConstant = 1024;
    private const float burnRateLerpConstant = .30f;
    private const int blueLineLayer = 10;
    public const int maxNumberOfTargets = 10;
    public const float chargePower = 1f / 8f;
    public static float AllomanticConstant { get; set; } = 1200;
    public static float MaxRange {
        get {
            return maxRange;
        }
        set {
            maxRange = value;
            sqrMaxRange = value * value;
        }
    }
    private static float maxRange = 100;
    private static float sqrMaxRange = maxRange * maxRange;

    // Button-press time constants
    private const float timeToHoldDown = .5f;
    private const float timeDoubleTapWindow = .5f;

    // Simple metal booleans for passing to methods
    private const bool steel = false;
    private const bool iron = true;

    //private LayerMask ignorePlayerLayer;
    private GamepadController gamepad;
    private Rigidbody rb;
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

    // Magnetic variables
    public int AvailableNumberOfTargets { get; private set; } = 1;
    public int PullCount { get; private set; } = 0;
    public int PushCount { get; private set; } = 0;

    private Magnetic[] pullTargets;
    private Magnetic[] pushTargets;

    // Used for calculating the acceleration over the last frame for pushing/pulling
    private Vector3 lastAllomancerVelocity = Vector3.zero;
    private Vector3 lastExpectedAllomancerAcceleration = Vector3.zero;

    public Vector3 LastNetForceOnAllomancer { get; private set; } = Vector3.zero;
    private Vector3 thisFrameNetForceOnAllomancer = Vector3.zero;
    //private Vector3 lastNormalForce = Vector3.zero;
    //private Vector3 thisFrameNormalForce = Vector3.zero;
    //private Vector3 lastAllomanticForce = Vector3.zero;
    //private Vector3 thisFrameAllomanticForce = Vector3.zero;

    private Vector3 thisFrameMaximumAllomanticForce = Vector3.zero;
    private Vector3 lastMaximumAllomanticForce = Vector3.zero;
    private Vector3 thisFrameMaximumNormalForce = Vector3.zero;
    private Vector3 lastMaximumNormalForce = Vector3.zero;

    // Determines if the player just toggled between pushing/pulling and not pushing/pulling
    private bool lastWasPulling = false;
    private bool lastWasPushing = false;

    // Used for burning metals
    private float ironBurnRate = 0;
    private float steelBurnRate = 0;
    // Lerp targets
    private float ironBurnRateTarget = 0;
    private float steelBurnRateTarget = 0;

    private float forceMagnitudeTarget = 600;

    // Currently hovered-over Magnetic
    public Magnetic HighlightedTarget { get; private set; } = null;
    public bool IronPulling { get; private set; } = false;
    public bool SteelPushing { get; private set; } = false;
    public bool IsBurningIronSteel { get; private set; } = false;
    public float Mass {
        get {
            return rb.mass;
        }
    }
    public bool HasHighlightedTarget {
        get {
            return HighlightedTarget != null;
        }
    }

    private void Awake() {
        //ignorePlayerLayer = ~(1 << LayerMask.NameToLayer("Player"));
        rb = GetComponent<Rigidbody>();
        gamepad = GameObject.FindGameObjectWithTag("GameController").GetComponent<GamepadController>();
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
        HUD.TargetOverlayController.Clear();
        //metalLines.Clear();
        HighlightedTarget = null;
        lastExpectedAllomancerAcceleration = Vector3.zero;
        LastNetForceOnAllomancer = Vector3.zero;
        //lastAllomanticForce = Vector3.zero;
        //lastNormalForce = Vector3.zero;
        lastMaximumAllomanticForce = Vector3.zero;
        lastMaximumNormalForce = Vector3.zero;
    }

    private void Update() {
        if (!PauseMenu.IsPaused) {

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
                        if (SettingsMenu.currentForceStyle == ForceStyle.Percentage)
                            ChangeBurnRateTarget(Keybinds.ScrollWheelAxis());
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
                if (SettingsMenu.currentForceStyle == ForceStyle.Percentage) {
                    if (GamepadController.UsingGamepad) {
                        SetPullRateTarget(Keybinds.RightBurnRate());
                        SetPushRateTarget(Keybinds.LeftBurnRate());
                    }
                } else {
                    float maxNetForce = (lastMaximumAllomanticForce + lastMaximumNormalForce).magnitude;
                    SetPullRateTarget(forceMagnitudeTarget / maxNetForce);
                    SetPushRateTarget(forceMagnitudeTarget / maxNetForce);
                }
                LerpToBurnRates();
                UpdateBurnRateMeter();

                IronPulling = Keybinds.IronPulling();
                SteelPushing = Keybinds.SteelPushing();

                // Change colors of target labels when toggling pushing/pulling
                if (IronPulling) {
                    if (!lastWasPulling) { // first frame of pulling
                        RefreshHUDColorsOnly();
                        lastWasPulling = true;
                    }
                } else {
                    if (lastWasPulling) {
                        RefreshHUDColorsOnly();
                        lastWasPulling = false;
                    }
                }
                if (SteelPushing) {
                    if (!lastWasPushing) { // first frame of pushing
                        RefreshHUDColorsOnly();
                        lastWasPushing = true;
                    }
                } else {
                    if (lastWasPushing) {
                        RefreshHUDColorsOnly();
                        lastWasPushing = false;
                    }
                }

                // Check input for target selection
                bool selecting = (Keybinds.Select() || Keybinds.SelectAlternate()) && !Keybinds.Negate();
                Magnetic target = SearchForMetals();
                // If any targets were not in range from SearchForMetals, remove them
                RemoveAllOutOfRangeTargets();

                // highlight the potential target you would select, if you targeted it
                if (target != null && target != HighlightedTarget) {
                    if (HasHighlightedTarget)
                        HighlightedTarget.RemoveTargetGlow();
                    target.AddTargetGlow();
                    HighlightedTarget = target;
                } else if (target == null) {
                    if (HasHighlightedTarget)
                        HighlightedTarget.RemoveTargetGlow();
                    HighlightedTarget = null;
                }


                if (Keybinds.Select() || Keybinds.SelectAlternate()) {
                    // Select or Deselect pullTarget and/or pushTarget
                    if (Keybinds.Select()) {
                        if (selecting) {
                            AddTarget(target, iron);
                        } else {
                            if (Keybinds.SelectDown()) {
                                if (IsTarget(target, iron)) {
                                    // Remove the target, but keep it highlighted
                                    RemoveTarget(target, iron);
                                    target.AddTargetGlow();
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
                                    // Remove the target, but keep it highlighted
                                    RemoveTarget(target, steel);
                                    target.AddTargetGlow();
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


            //if (IsBurningIronSteel && !searchingForTarget) { // not searching for a new target but still burning passively, show lines
            //    //SearchForMetals(searchingForTarget);
            //}
        }
    }

    private void FixedUpdate() {
        if (!PauseMenu.IsPaused) {
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
                LastNetForceOnAllomancer = thisFrameNetForceOnAllomancer;
                lastExpectedAllomancerAcceleration = LastNetForceOnAllomancer / rb.mass;
                //lastAllomanticForce = thisFrameAllomanticForce;
                //lastNormalForce = thisFrameNormalForce;
                lastMaximumAllomanticForce = thisFrameMaximumAllomanticForce;
                lastMaximumNormalForce = thisFrameMaximumNormalForce;
                thisFrameNetForceOnAllomancer = Vector3.zero;
                //thisFrameAllomanticForce = Vector3.zero;
                //thisFrameNormalForce = Vector3.zero;
                thisFrameMaximumAllomanticForce = Vector3.zero;
                thisFrameMaximumNormalForce = Vector3.zero;
            }
        }
    }

    // Refreshes the colors of the text of target labels and the burn rate meter.
    private void RefreshHUDColorsOnly() {
        if (IronPulling) {
            if (!HasPullTarget) {
                HUD.TargetOverlayController.SetPushTextColorStrong();
            } else { // has pull target
                HUD.TargetOverlayController.SetPullTextColorStrong();
            }
            HUD.BurnRateMeter.SetForceTextColorStrong();
        } else {
            if (!HasPullTarget || !SteelPushing || (HasPushTarget)) {
                HUD.TargetOverlayController.SetPullTextColorWeak();
            }
        }
        if (SteelPushing) {
            if (!HasPushTarget) {
                HUD.TargetOverlayController.SetPullTextColorStrong();
            } else { // has push target
                HUD.TargetOverlayController.SetPushTextColorStrong();
            }
            HUD.BurnRateMeter.SetForceTextColorStrong();
        } else {
            if (!IronPulling) {
                HUD.BurnRateMeter.SetForceTextColorWeak();
            }

            if (!HasPushTarget || !IronPulling || (HasPullTarget)) {
                HUD.TargetOverlayController.SetPushTextColorWeak();
            }
        }
    }

    private void RefreshHUD() {
        RefreshHUDColorsOnly();
        HUD.TargetOverlayController.HardRefresh();
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
     * F =  C * Burn rate * sixteenth or eighth root (depending on my mood) of (Allomancer mass * target mass) 
     *      / squared distance between the two
     *      / number of targets currently being pushed on (push on one target => 100% of the push going to them, push on three targets => 33% of each push going to each, etc. Not thought out very in-depth.)
     *  C: an Allomantic Constant. I chose a value that felt right.
     */
    private void CalculateForce(Magnetic target, bool usingIronTargets) {
        Vector3 distanceFactor = DistanceFactor(target);
        Vector3 allomanticForce = AllomanticConstant * Mathf.Pow(target.Mass * rb.mass, chargePower) * distanceFactor / (usingIronTargets ? PullCount : PushCount) * (target.LastWasPulled ? 1 : -1);

        // If controlling the strength of the push by Percentage of the maximum possible push
        if (SettingsMenu.currentForceStyle == ForceStyle.Percentage) {
            allomanticForce *= (target.LastWasPulled ? ironBurnRate : steelBurnRate);
        }
        thisFrameMaximumAllomanticForce += allomanticForce;

        Vector3 restitutionForceFromTarget;
        Vector3 restitutionForceFromAllomancer;

        //if ((IronPulling && target.LastWasPulled) || (SteelPushing && !target.LastWasPulled)) { //If pushing or pulling, ANF should be added to calculation
        switch (PhysicsController.anchorBoostMode) {
            case AnchorBoostMode.AllomanticNormalForce: {
                    if (target.IsStatic) {
                        // If the target has no rigidbody, let the let the restitution force equal the allomantic force. It's a perfect anchor.
                        // Thus:
                        // a push against a perfectly anchored metal structure is exactly twice as powerful as a push against a completely unanchored, freely-moving metal structure
                        restitutionForceFromTarget = allomanticForce;
                    } else {
                        // Calculate Allomantic Normal Forces

                        if (target.IsPerfectlyAnchored) { // If target is perfectly anchored, pushes are perfectly resisted. Its ANF = AF.
                            restitutionForceFromTarget = allomanticForce;
                        } else { // Target is partially anchored
                                 //calculate changes from the last frame

                            Vector3 newTargetVelocity = target.Velocity;
                            Vector3 lastTargetAcceleration = (newTargetVelocity - target.LastVelocity) / Time.fixedDeltaTime;
                            Vector3 unaccountedForTargetAcceleration = lastTargetAcceleration - target.LastExpectedAcceleration;// + Physics.gravity;
                            restitutionForceFromTarget = Vector3.Project(unaccountedForTargetAcceleration * target.Mass, distanceFactor.normalized);
                        }
                        // using Impulse strategy
                        //restitutionForceFromTarget = Vector3.ClampMagnitude(Vector3.Project(target.forceFromCollisionTotal, positionDifference.normalized), allomanticForce.magnitude);

                        target.LastPosition = target.transform.position;
                        target.LastVelocity = target.Velocity;
                    }

                    Vector3 newAllomancerVelocity = rb.velocity;
                    Vector3 lastAllomancerAcceleration = (newAllomancerVelocity - lastAllomancerVelocity) / Time.fixedDeltaTime;
                    Vector3 unaccountedForAllomancerAcceleration = lastAllomancerAcceleration - lastExpectedAllomancerAcceleration;
                    restitutionForceFromAllomancer = Vector3.Project(unaccountedForAllomancerAcceleration * rb.mass, distanceFactor.normalized);


                    if (PhysicsController.normalForceMaximum == NormalForceMaximum.AllomanticForce) {
                        restitutionForceFromTarget = Vector3.ClampMagnitude(restitutionForceFromTarget, allomanticForce.magnitude);
                        restitutionForceFromAllomancer = Vector3.ClampMagnitude(restitutionForceFromAllomancer, distanceFactor.magnitude);
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
                        case NormalForceMinimum.ZeroAndNegate: {
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

                    // Makes the ANF on the target and Allomancer equal
                    if (PhysicsController.normalForceEquality) {
                        if (restitutionForceFromAllomancer.magnitude > restitutionForceFromTarget.magnitude) {
                            restitutionForceFromTarget = -restitutionForceFromAllomancer;
                        } else {
                            restitutionForceFromAllomancer = -restitutionForceFromTarget;
                        }
                    }

                    break;
                }
            case AnchorBoostMode.ExponentialWithVelocity: {
                    // The restitutionForceFromTarget is actually negative, rather than positive, unlike in ANF mode. It contains the percentage of the AF that is subtracted from the AF to get the net AF.
                    float velocityFactorTarget;
                    float velocityFactorAllomancer;
                    if (PhysicsController.exponentialWithVelocityRelativity == ExponentialWithVelocityRelativity.Absolute) {
                        velocityFactorTarget = 1 - Mathf.Exp(-(Vector3.Project(target.Velocity, distanceFactor.normalized).magnitude / PhysicsController.velocityConstant));
                        velocityFactorAllomancer = 1 - Mathf.Exp(-(Vector3.Project(rb.velocity, distanceFactor.normalized).magnitude / PhysicsController.velocityConstant));
                    } else {
                        velocityFactorTarget = 1 - Mathf.Exp(-(Vector3.Project(rb.velocity - target.Velocity, distanceFactor.normalized).magnitude / PhysicsController.velocityConstant));
                        velocityFactorAllomancer = 1 - Mathf.Exp(-(Vector3.Project(target.Velocity - rb.velocity, distanceFactor.normalized).magnitude / PhysicsController.velocityConstant));
                    }
                    if (PhysicsController.exponentialWithVelocitySignage == ExponentialWithVelocitySignage.BackwardsDecreasesAndForwardsIncreasesForce) {
                        if (Vector3.Dot(rb.velocity, distanceFactor) > 0) {
                            velocityFactorTarget *= -1;
                        }
                        if (Vector3.Dot(target.Velocity, distanceFactor) < 0) {
                            velocityFactorAllomancer *= -1;
                        }
                    } else if (PhysicsController.exponentialWithVelocitySignage == ExponentialWithVelocitySignage.OnlyBackwardsDecreasesForce) {
                        if (Vector3.Dot(rb.velocity, distanceFactor) > 0) {
                            velocityFactorTarget *= 0;
                        }
                        if (Vector3.Dot(target.Velocity, distanceFactor) < 0) {
                            velocityFactorAllomancer *= 0;
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
        //} else { // Player is neither pushing nor pulling
        //        restitutionForceFromAllomancer = Vector3.zero;
        //        restitutionForceFromTarget = Vector3.zero;
        //}
        thisFrameMaximumNormalForce += restitutionForceFromTarget;

        // If controlling the push strength by using a target force to push with
        if (SettingsMenu.currentForceStyle == ForceStyle.ForceMagnitude) {
            float percent = forceMagnitudeTarget / (allomanticForce + restitutionForceFromTarget).magnitude;
            if (percent < 1f) {
                allomanticForce *= percent;
                restitutionForceFromTarget *= percent;
                //restitutionForceFromAllomancer *= percent;
            }
        }

        target.LastExpectedAcceleration = -allomanticForce / target.Mass;
        thisFrameNetForceOnAllomancer += allomanticForce;

        target.LastAllomanticForce = allomanticForce;
        target.LastAllomanticNormalForceFromAllomancer = restitutionForceFromAllomancer;
        target.LastAllomanticNormalForceFromTarget = restitutionForceFromTarget;
    }

    private Vector3 DistanceFactor(Magnetic target) {
        Vector3 positionDifference = target.CenterOfMass - CenterOfMass;
        float distance = positionDifference.magnitude;
        switch (PhysicsController.distanceRelationshipMode) {
            case ForceDistanceRelationship.InverseSquareLaw: {
                    return (positionDifference / distance / distance);
                }
            case ForceDistanceRelationship.Linear: {
                    return positionDifference.normalized * (1 - positionDifference.magnitude / MaxRange);
                }
            default: {
                    return positionDifference.normalized * Mathf.Exp(-distance / PhysicsController.distanceConstant);
                }
        }
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

        target.AddForce(target.LastNetAllomanticForceOnTarget, ForceMode.Force);
        // apply force to player
        rb.AddForce(target.LastNetAllomanticForceOnAllomancer, ForceMode.Force);

        //target.Rb.AddForce(targetVelocity, ForceMode.VelocityChange);

        //rb.AddForce(allomancerVelocity, ForceMode.VelocityChange);

        // set up for next frame
        //thisFrameExpectedAllomancerAcceleration += target.LastAllomanticForceOnAllomancer / rb.mass;

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
    

    /*
     * Searches all Magnetics in the scene for those that are within detection range of the player.
     * Shows metal lines drawing from them to the player.
     * Returns the Magnetic "closest" of these to the center of the screen.
     * 
     * Rules for the metal lines:
     *  - The WIDTH of the line is dependant on the MASS of the target
     *  - The BRIGHTNESS of the line is dependent on the DISTANCE from the player
     *  - The LIGHT SABER FACTOR is dependent on the FORCE acting on the target. If the metal is not a target, it is 1.
     */
    private Magnetic SearchForMetals() {
        float smallestDistanceFromCenter = 1f;
        Magnetic centerObject = null;

        for (int i = 0; i < GameManager.MagneticsInScene.Count; i++) {
            Magnetic target = GameManager.MagneticsInScene[i];
            if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < sqrMaxRange) {
                float softForce = AllomanticConstant * target.Charge * Mathf.Pow(Mass, chargePower) * DistanceFactor(target).magnitude;
                // If using Percentage force mode, burn rate affects your range for burning
                if (SettingsMenu.currentForceStyle == ForceStyle.Percentage && SettingsMenu.currentControlScheme != ControlScheme.Gamepad)
                    softForce *= Mathf.Max(ironBurnRate, steelBurnRate);
                softForce -= blueLineForceCutoff;
                if (softForce > 0) {
                    //if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < effectiveMaxRange * effectiveMaxRange) {
                    // Magnetic is within range, as determined by its mass
                    target.InRange = true;

                    // calculate the object's position on screen and find the one closest to the center to highlight.
                    Vector3 screenPosition = CameraController.ActiveCamera.WorldToViewportPoint(target.transform.position);
                    if (screenPosition.z > 0 && screenPosition.x > horizontalMin && screenPosition.x < horizontalMax && screenPosition.y > verticalMin && screenPosition.y < verticalMax) {
                        // Test if the new object is the more ideal pullTarget than the last most ideal pullTarget
                        float distanceFromCenter = verticalImportanceFactor * Mathf.Pow(screenPosition.x - .5f, 2) + Mathf.Pow(screenPosition.y - .5f, 2);
                        if (distanceFromCenter < smallestDistanceFromCenter) {
                            smallestDistanceFromCenter = distanceFromCenter;
                            centerObject = target;
                        }
                    }

                    // Set line properties
                    if (SettingsMenu.renderBlueLines) {
                        float closeness = Mathf.Exp(-blueLineBrightnessConstant / softForce);
                        target.SetBlueLine(
                            CenterOfMass,
                            blueLineWidthConstant * target.Charge,
                            1,
                            new Color(0, closeness * gMaxLines, closeness * bMaxLines, 1)
                            );
                    } else {
                        target.DisableBlueLine();
                    }
                } else {
                    // Magnetic is out of max range
                    target.InRange = false;
                    GameManager.MagneticsInScene[i].DisableBlueLine();
                }
            } else {
                // Magnetic is out of max range
                target.InRange = false;
                GameManager.MagneticsInScene[i].DisableBlueLine();
            }
        }

        // Go through targets and update their metal lines
        for (int i = 0; i < PullCount; i++) {
            Magnetic target = pullTargets[i];
            if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < sqrMaxRange) {
                target.InRange = true;
                target.SetBlueLine(
                    CenterOfMass,
                    blueLineWidthConstant * target.Charge * blueLineTargetedWidthConstant,
                    Mathf.Exp(-target.LastNetAllomanticForceOnAllomancer.magnitude / lightSaberConstant),
                    new Color(0, 1, gMaxLines, 1));
            } else {
                target.InRange = false;
                target.DisableBlueLine();
            }
        }
        for (int i = 0; i < PushCount; i++) {
            Magnetic target = pushTargets[i];
            if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < sqrMaxRange) {
                target.InRange = true;
                target.SetBlueLine(
                    CenterOfMass,
                    blueLineWidthConstant * target.Charge * blueLineTargetedWidthConstant,
                    Mathf.Exp(-target.LastNetAllomanticForceOnAllomancer.magnitude / lightSaberConstant),
                    new Color(1, gMaxLines, 0, 1));
            } else {
                target.InRange = false;
                target.DisableBlueLine();
            }
        }
        return centerObject;
    }

    private void RemoveTarget(int index, bool ironTarget) {

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
        RefreshHUD();
    }

    public void RemoveTarget(Magnetic target, bool ironTarget, bool searchBoth = false) {

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
                            RefreshHUD();
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

                        RefreshHUD();
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
        HUD.TargetOverlayController.Clear();
    }

    public void AddTarget(Magnetic newTarget, bool usingIron) {
        StartBurningIronSteel();
        if (newTarget != null && !IsTarget(newTarget)) {
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

                            RefreshHUD();
                            return;
                        }
                        // this space in the array is taken.
                        if (pullTargets[i] == newTarget) { // newTarget is already in the array. Remove it, then continue adding this target to the end.
                            for (int j = i; j < PullCount - 1; j++) {
                                pullTargets[j] = pullTargets[j + 1];
                            }
                            pullTargets[PullCount - 1] = newTarget;

                            RefreshHUD();
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

                    RefreshHUD();
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
                            RefreshHUD();
                            return;
                        }
                        // this space in the array is taken.
                        if (pushTargets[i] == newTarget) { // newTarget is already in the array. Remove it, then continue adding this target to the end.
                            for (int j = i; j < PushCount - 1; j++) {
                                pushTargets[j] = pushTargets[j + 1];
                            }
                            pushTargets[PushCount - 1] = newTarget;

                            RefreshHUD();
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

                    RefreshHUD();
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
            CameraController.ActiveCamera.cullingMask = ~0;
            SetPullRateTarget(1);
            SetPushRateTarget(1);
        }
    }

    public void StopBurningIronSteel() {
        RemoveAllTargets();
        //if (IsBurningIronSteel) {
        if (HasHighlightedTarget)
            HighlightedTarget.RemoveTargetGlow();
        IsBurningIronSteel = false;
        if (HUD.BurnRateMeter) {
            HUD.BurnRateMeter.Clear();
        }
        if (gamepad)
            gamepad.SetRumble(0, 0);
        ironBurnRate = 0;
        steelBurnRate = 0;

        // make blue lines disappear
        CameraController.ActiveCamera.cullingMask = ~(1 << blueLineLayer);
        //for (int i = 0; i < metalLines.Count; i++) {
        //    metalLines[i].GetComponent<MeshRenderer>().enabled = false;
        //}
        //}
    }

    private void SetPullRateTarget(float rate) {
        if (rate > .01f) {
            if (SettingsMenu.currentForceStyle == ForceStyle.Percentage)
                ironBurnRateTarget = rate;
            else
                ironBurnRateTarget = Mathf.Min(1, rate);
            if (HasPullTarget || HasPushTarget)
                gamepad.SetRumble(steelBurnRateTarget, ironBurnRateTarget);
        } else {
            //IronPulling = false;
            ironBurnRateTarget = 0;
            gamepad.SetRumbleRight(0);
        }
    }

    private void SetPushRateTarget(float rate) {
        if (rate > .01f) {
            if (SettingsMenu.currentForceStyle == ForceStyle.Percentage)
                steelBurnRateTarget = rate;
            else
                steelBurnRateTarget = Mathf.Min(1, rate);
            if (HasPullTarget || HasPushTarget)
                gamepad.SetRumble(steelBurnRateTarget, ironBurnRateTarget);
        } else {
            //SteelPushing = false;
            steelBurnRateTarget = 0;
            gamepad.SetRumbleLeft(0);
        }
    }

    private void LerpToBurnRates() {
        ironBurnRate = Mathf.Lerp(ironBurnRate, ironBurnRateTarget, burnRateLerpConstant);
        steelBurnRate = Mathf.Lerp(steelBurnRate, steelBurnRateTarget, burnRateLerpConstant);
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
        RefreshHUD();
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
        //if (AvailableNumberOfTargets > 0) {
        if (AvailableNumberOfTargets > 1) {
            AvailableNumberOfTargets--;
            if (PullCount > AvailableNumberOfTargets)
                RemoveTarget(0, iron);
            if (PushCount > AvailableNumberOfTargets)
                RemoveTarget(0, steel);
            //// never actually have 0 available targets. Just remove targets, and stay at 1 available targets.
            //if (AvailableNumberOfTargets == 0) {
            //    AvailableNumberOfTargets++;
            //} else {
            //    HUD.BurnRateMeter.MetalLineText = AvailableNumberOfTargets.ToString();
            //}
            HUD.BurnRateMeter.MetalLineText = AvailableNumberOfTargets.ToString();
        }

        RefreshHUD();
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

    private void ChangeBurnRateTarget(float change) {
        if (change > 0) {
            change = .1f;
        } else if (change < 0) {
            change = -.1f;
        }
        SetPullRateTarget(Mathf.Clamp(ironBurnRateTarget + change, 0, 1));
        SetPushRateTarget(Mathf.Clamp(steelBurnRateTarget + change, 0, 1));
    }

    private void UpdateBurnRateMeter() {
        if (SettingsMenu.currentForceStyle == ForceStyle.Percentage)
            HUD.BurnRateMeter.SetBurnRateMeterPercentage(lastMaximumAllomanticForce, lastMaximumNormalForce, Mathf.Max(ironBurnRate, steelBurnRate));
        else
            HUD.BurnRateMeter.SetBurnRateMeterForceMagnitude(lastMaximumAllomanticForce, lastMaximumNormalForce, forceMagnitudeTarget);
        //HUD.BurnRateMeter.SetBurnRateMeterForceMagnitude(lastAllomanticForce, lastNormalForce, forceMagnitudeTarget, lastMaximumAllomanticForce, lastMaximumNormalForce);
    }
}
