using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VolumetricLines;
/*
 * Controls all facets of Ironpulling and Steelpushing.
 */
public class AllomanticIronSteel : MonoBehaviour {

    // Constants for Metal Lines
    private const float horizontalMin = .45f;
    private const float horizontalMax = .55f;
    private const float verticalMin = .2f;
    private const float verticalMax = .8f;
    private const float targetFocusRadius = .1f;                   // Determines the range around the center of the screen within which blue lines are in focus.
    private const float verticalImportanceFactor = 1 / 64f;        // Determines how elliptical the range around the center of the screen is. Squared.
    private const float targetFocusFalloffConstant = 128;           // Determines how quickly blue lines blend from in-focus to out-of-focus
    private const float targetFocusLowerBound = .2f;               // Determines the luminosity of blue lines that are out of foucus
    private const float targetFocusOffScreenBound = .035f;           // Determines the luminosity of blue lines that are off-screen
    private const float lowLineColor = .1f;
    private const float highLineColor = .85f;
    private readonly Color targetedRedLine = new Color(1, 0, 1);
    private readonly Color targetedGreenLine = new Color(0, 1, 0);
    private readonly Color targetedBlueLine = new Color(0, 0, 1);
    private const float blueLineWidthBaseFactor = .04f;
    private const float blueLineTargetedWidthFactor = 1 / 1.5f;
    private const float blueLineStartupFactor = 2f;
    private const float blueLineBrightnessFactor = 1 / 1f;
    private const float forceDetectionThreshold = 50f;              // If the potential force on a target is above this, a blue line will point to it
    private const float lightSaberConstant = 1024;
    // Physics Constants
    public const float chargePower = 1f / 8f;
    // Button-press time constants
    private const float timeToHoldDown = .5f;
    private const float timeDoubleTapWindow = .5f;
    // Other Constants
    private const float burnRateLerpConstant = .30f;
    private const int blueLineLayer = 10;
    public const int maxNumberOfTargets = 10;
    // Simple metal booleans for passing to methods
    private const bool steel = false;
    private const bool iron = true;

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
    //private Vector3 thisFrameNetForceOnAllomancer = Vector3.zero;
    private Vector3 lastNormalForce = Vector3.zero;
    private Vector3 thisFrameNormalForce = Vector3.zero;
    private Vector3 lastAllomanticForce = Vector3.zero;
    private Vector3 thisFrameAllomanticForce = Vector3.zero;

    //private Vector3 thisFrameMaximumAllomanticForce = Vector3.zero;
    //private Vector3 lastMaximumAllomanticForce = Vector3.zero;
    private Vector3 thisFrameMaximumNetForce = Vector3.zero;
    private Vector3 lastMaximumNetForce = Vector3.zero;
    //private Vector3 thisFrameMaximumNormalForce = Vector3.zero;
    //private Vector3 lastMaximumNormalForce = Vector3.zero;

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
    public Vector3 CenterOfMass {
        get {
            return centerOfMass.position;
        }
    }
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
        rb = GetComponent<Rigidbody>();
        centerOfMass.localPosition = Vector3.zero;
        metalLinesAnchor.localPosition = centerOfMass.localPosition;
    }

    public void Clear() {
        IsBurningIronSteel = false;
        IronPulling = false;
        SteelPushing = false;
        forceMagnitudeTarget = 600;
        ironBurnRate = 0;
        steelBurnRate = 0;
        PullCount = 0;
        PushCount = 0;
        pullTargets = new Magnetic[maxNumberOfTargets];
        pushTargets = new Magnetic[maxNumberOfTargets];
        HUD.TargetOverlayController.SetTargets(pullTargets, pushTargets);
        HUD.TargetOverlayController.Clear();
        HighlightedTarget = null;
        lastExpectedAllomancerAcceleration = Vector3.zero;
        LastNetForceOnAllomancer = Vector3.zero;
        lastAllomanticForce = Vector3.zero;
        lastNormalForce = Vector3.zero;
        lastMaximumNetForce = Vector3.zero;
        //lastMaximumAllomanticForce = Vector3.zero;
        //lastMaximumNormalForce = Vector3.zero;
    }

    private void Update() {
        if (!PauseMenu.IsPaused && Player.CanControlPlayer) {
            // Start burning
            if ((Keybinds.SelectDown() || Keybinds.SelectAlternateDown()) && !Keybinds.Negate()) {
                StartBurningIronSteel();
            }

            if (IsBurningIronSteel) {


                // Swap pull- and push- targets
                if (Keybinds.NegateDown() && timeToSwapBurning > Time.time) {
                    // Double-tapped, Swap targets
                    SwapPullPushTargets();
                } else {
                    if (Keybinds.NegateDown()) {
                        timeToSwapBurning = Time.time + timeDoubleTapWindow;
                    }
                }

                // Check scrollwheel for changing the max number of targets and burn rate
                if (SettingsMenu.settingsData.controlScheme == 2) {
                    float scrollValue = Keybinds.ScrollWheelAxis();
                    if (scrollValue > 0) {
                        IncrementNumberOfTargets();
                    }
                    if (scrollValue < 0) {
                        DecrementNumberOfTargets();
                    }
                } else {
                    if (Keybinds.ScrollWheelButton()) {
                        if (Keybinds.ScrollWheelAxis() != 0) {
                            if (Keybinds.ScrollWheelAxis() > 0) {
                                IncrementNumberOfTargets();
                            } else if (Keybinds.ScrollWheelAxis() < 0) {
                                DecrementNumberOfTargets();
                            }
                        }
                    } else {
                        if (SettingsMenu.settingsData.pushControlStyle == 1)
                            ChangeTargetForceMagnitude(Keybinds.ScrollWheelAxis());
                        else
                            ChangeBurnRateTarget(Keybinds.ScrollWheelAxis());
                    }
                }
                // If controlling push Magnitude, assign the burn rate to be the % of the max net force on player
                if (SettingsMenu.settingsData.pushControlStyle == 1) {
                    float maxNetForce = (lastMaximumNetForce).magnitude;
                    SetPullRateTarget(forceMagnitudeTarget / maxNetForce);
                    SetPushRateTarget(forceMagnitudeTarget / maxNetForce);
                } else {
                    if (SettingsMenu.settingsData.controlScheme == 2) {
                        SetPullRateTarget(Keybinds.RightBurnRate());
                        SetPushRateTarget(Keybinds.LeftBurnRate());
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

                LerpToBurnRates();
                UpdateBurnRateMeter();
            }

            // Could have stopped burning above. Check if the Allomancer is still burning.
            if (IsBurningIronSteel) {

                IronPulling = Keybinds.IronPulling();
                SteelPushing = Keybinds.SteelPushing();

                // If you are trying to push and pull and don't have both push and pull targets, only pull.
                if (IronPulling && SteelPushing && !(HasPullTarget && HasPushTarget)) {
                    SteelPushing = false;
                }

                // Change colors of target labels when toggling pushing/pulling
                if (IronPulling) {
                    if (!lastWasPulling) { // first frame of pulling
                        RefreshHUDColorsOnly();
                        lastWasPulling = true;
                        //StartPushPullingOnTargets(iron);
                    }
                } else {
                    if (lastWasPulling) { // first frame of NOT pulling
                        RefreshHUDColorsOnly();
                        lastWasPulling = false;
                        if (HasPullTarget) {
                            StopOnPullTargets();
                        } else {
                            StopOnPushTargets();
                        }
                    }
                }
                if (SteelPushing) {
                    if (!lastWasPushing) { // first frame of pushing
                        RefreshHUDColorsOnly();
                        lastWasPushing = true;
                        //StartPushPullingOnTargets(steel);
                    }
                } else {
                    if (lastWasPushing) { // first frame of NOT pushing
                        RefreshHUDColorsOnly();
                        lastWasPushing = false;
                        if (HasPushTarget) {
                            StopOnPushTargets();
                        } else {
                            StopOnPullTargets();
                        }
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
            }
        }
    }

    private void FixedUpdate() {
        if (!PauseMenu.IsPaused && Player.CanControlPlayer) {
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

                lastAllomanticForce = thisFrameAllomanticForce;
                thisFrameAllomanticForce = Vector3.zero;
                lastNormalForce = thisFrameNormalForce;
                thisFrameNormalForce = Vector3.zero;
                //lastMaximumAllomanticForce = thisFrameMaximumAllomanticForce;
                //lastMaximumNormalForce = thisFrameMaximumNormalForce;
                lastMaximumNetForce = thisFrameMaximumNetForce;
                thisFrameMaximumNetForce = Vector3.zero;
                LastNetForceOnAllomancer = lastAllomanticForce + lastNormalForce;
                lastExpectedAllomancerAcceleration = LastNetForceOnAllomancer / rb.mass;
                //thisFrameNetForceOnAllomancer = Vector3.zero;
                //thisFrameMaximumAllomanticForce = Vector3.zero;
                //thisFrameMaximumNormalForce = Vector3.zero;
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
        HUD.BurnRateMeter.HardRefresh();
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
     * Formula subject to change:
     * F =  C * Burn rate * sixteenth or eighth root (depending on my mood) of (Allomancer mass * target mass) 
     *      / squared distance between the two
     *      / (depending on my mood) number of targets currently being pushed on
     *              (i.e. push on one target => 100% of the push going to them, push on three targets => 33% of each push going to each, etc. Not thought out very in-depth.)
     *  C: the Allomantic Constant.
     */
    private void CalculateForce(Magnetic target, bool usingIronTargets) {
        Vector3 distanceFactor = DistanceFactor(target);
        Vector3 allomanticForce = SettingsMenu.settingsData.allomanticConstant * target.Charge * Mathf.Pow(rb.mass, chargePower) * distanceFactor /* / (usingIronTargets ? PullCount : PushCount) */ * (target.LastWasPulled ? 1 : -1);

        //thisFrameMaximumAllomanticForce += allomanticForce;
        thisFrameMaximumNetForce += allomanticForce;

        // The AF is proportional to the burn rate
        allomanticForce *= (target.LastWasPulled ? ironBurnRate : steelBurnRate);

        Vector3 restitutionForceFromTarget;
        Vector3 restitutionForceFromAllomancer;

        //if ((IronPulling && target.LastWasPulled) || (SteelPushing && !target.LastWasPulled)) { //If pushing or pulling, ANF should be added to calculation
        switch (SettingsMenu.settingsData.anchoredBoost) {
            case 0: { // Disabled
                    restitutionForceFromTarget = Vector3.zero;
                    restitutionForceFromAllomancer = Vector3.zero;
                    break;
                }
            case 1: { // ANF
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
                            restitutionForceFromTarget = Vector3.Project(unaccountedForTargetAcceleration * target.NetMass, distanceFactor.normalized);
                        }
                    }

                    Vector3 lastAllomancerAcceleration = (rb.velocity - lastAllomancerVelocity) / Time.fixedDeltaTime;
                    Vector3 unaccountedForAllomancerAcceleration = lastAllomancerAcceleration - lastExpectedAllomancerAcceleration;
                    restitutionForceFromAllomancer = Vector3.Project(unaccountedForAllomancerAcceleration * rb.mass, distanceFactor.normalized);

                    if (SettingsMenu.settingsData.normalForceMax == 1) {
                        restitutionForceFromTarget = Vector3.ClampMagnitude(restitutionForceFromTarget, allomanticForce.magnitude);
                        restitutionForceFromAllomancer = Vector3.ClampMagnitude(restitutionForceFromAllomancer, allomanticForce.magnitude);
                    }

                    // Prevents the ANF from being negative relative to the AF and prevents the ANF from ever decreasing the AF below its original value
                    switch (SettingsMenu.settingsData.normalForceMin) {
                        case 0: {
                                break;
                            }
                        case 1: {
                                if (Vector3.Dot(restitutionForceFromAllomancer, allomanticForce) > 0) {
                                    restitutionForceFromAllomancer = Vector3.zero;
                                }
                                if (Vector3.Dot(restitutionForceFromTarget, allomanticForce) < 0) {
                                    restitutionForceFromTarget = Vector3.zero;
                                }
                                break;
                            }
                        default: {
                                if (Vector3.Dot(restitutionForceFromAllomancer, allomanticForce) > 0) {
                                    restitutionForceFromAllomancer = -restitutionForceFromAllomancer;
                                }
                                if (Vector3.Dot(restitutionForceFromTarget, allomanticForce) < 0) {
                                    restitutionForceFromTarget = -restitutionForceFromTarget;
                                }
                                break;
                            }
                    }

                    // Makes the ANF on the target and Allomancer equal
                    if (SettingsMenu.settingsData.normalForceEquality == 1) {
                        if (restitutionForceFromAllomancer.magnitude > restitutionForceFromTarget.magnitude) {
                            restitutionForceFromTarget = -restitutionForceFromAllomancer;
                        } else {
                            restitutionForceFromAllomancer = -restitutionForceFromTarget;
                        }
                    }

                    break;
                }
            default: { // EWV
                    // The restitutionForceFromTarget is actually negative, rather than positive, unlike in ANF mode. It contains the percentage of the AF that is subtracted from the AF to get the net AF.
                    float velocityFactorTarget;
                    float velocityFactorAllomancer;
                    if (SettingsMenu.settingsData.exponentialWithVelocityRelativity == 1) {
                        velocityFactorTarget = 1 - Mathf.Exp(-(Vector3.Project(target.Velocity, distanceFactor.normalized).magnitude / SettingsMenu.settingsData.velocityConstant));
                        velocityFactorAllomancer = 1 - Mathf.Exp(-(Vector3.Project(rb.velocity, distanceFactor.normalized).magnitude / SettingsMenu.settingsData.velocityConstant));
                    } else {
                        velocityFactorTarget = 1 - Mathf.Exp(-(Vector3.Project(rb.velocity - target.Velocity, distanceFactor.normalized).magnitude / SettingsMenu.settingsData.velocityConstant));
                        velocityFactorAllomancer = 1 - Mathf.Exp(-(Vector3.Project(target.Velocity - rb.velocity, distanceFactor.normalized).magnitude / SettingsMenu.settingsData.velocityConstant));
                    }
                    switch (SettingsMenu.settingsData.exponentialWithVelocitySignage) {
                        case 0: {
                                // Do nothing
                                break;
                            }
                        case 1: {
                                if (Vector3.Dot(rb.velocity, distanceFactor) > 0) {
                                    velocityFactorTarget *= 0;
                                }
                                if (Vector3.Dot(target.Velocity, distanceFactor) < 0) {
                                    velocityFactorAllomancer *= 0;
                                }
                                break;
                            }
                        default: {
                                if (Vector3.Dot(rb.velocity, distanceFactor) > 0) {
                                    velocityFactorTarget *= -1;
                                }
                                if (Vector3.Dot(target.Velocity, distanceFactor) < 0) {
                                    velocityFactorAllomancer *= -1;
                                }
                                break;
                            }
                    }

                    restitutionForceFromAllomancer = allomanticForce * velocityFactorAllomancer;
                    restitutionForceFromTarget = allomanticForce * -velocityFactorTarget;

                    break;
                }
        }
        //} else { // Player is neither pushing nor pulling
        //        restitutionForceFromAllomancer = Vector3.zero;
        //        restitutionForceFromTarget = Vector3.zero;
        //}
        //thisFrameMaximumNormalForce += restitutionForceFromTarget;

        //// If controlling the push strength by using a target force to push with
        //if (SettingsMenu.settingsData.pushControlStyle == 1) {
        //    float percent = forceMagnitudeTarget / (allomanticForce + restitutionForceFromTarget).magnitude;
        //    if (percent < 1f) {
        //        allomanticForce *= percent;
        //        restitutionForceFromTarget *= percent;
        //        restitutionForceFromAllomancer *= percent;
        //    }
        //}

        //target.LastExpectedAcceleration = -allomanticForce / target.Mass;
        //thisFrameNetForceOnAllomancer += allomanticForce;
        thisFrameAllomanticForce += allomanticForce;
        thisFrameNormalForce += restitutionForceFromTarget;
        thisFrameMaximumNetForce += restitutionForceFromTarget / (target.LastWasPulled ? ironBurnRate : steelBurnRate);

        //target.LastPosition = target.transform.position;
        //target.LastVelocity = target.Velocity;
        target.LastAllomanticForce = allomanticForce;
        target.LastAllomanticNormalForceFromAllomancer = restitutionForceFromAllomancer;
        target.LastAllomanticNormalForceFromTarget = restitutionForceFromTarget;
    }

    private Vector3 DistanceFactor(Magnetic target) {
        Vector3 positionDifference = target.CenterOfMass - CenterOfMass;
        float distance = positionDifference.magnitude;
        switch (SettingsMenu.settingsData.forceDistanceRelationship) {
            case 0: {
                    return positionDifference.normalized * (1 - positionDifference.magnitude / SettingsMenu.settingsData.maxPushRange);
                }
            case 1: {
                    return (positionDifference / distance / distance);
                }
            default: {
                    return positionDifference.normalized * Mathf.Exp(-distance / SettingsMenu.settingsData.distanceConstant);
                }
        }
    }

    /*
     * Applys the force that was calculated in CalculateForce to the target and player.
     * This effectively executes the Push or Pull.
     */
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
        target.AddForce(target.LastNetAllomanticForceOnTarget);
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
            if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < SettingsMenu.settingsData.maxPushRange * SettingsMenu.settingsData.maxPushRange) {
                //if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < sqrMaxRange) {
                float softForce = SettingsMenu.settingsData.allomanticConstant * target.Charge * Mathf.Pow(Mass, chargePower) * DistanceFactor(target).magnitude;
                // If using Percentage force mode, burn rate affects your range for burning
                if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != 2)
                    softForce *= Mathf.Max(ironBurnRate, steelBurnRate);
                softForce -= forceDetectionThreshold;
                if (softForce > 0) {
                    //if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < effectiveMaxRange * effectiveMaxRange) {
                    // Magnetic is within range, as determined by its mass
                    target.InRange = true;
                    // calculate the object's position on screen and find the one closest to the center to highlight.
                    Vector3 screenPosition = CameraController.ActiveCamera.WorldToViewportPoint(target.transform.position);

                    // Calculate the distance from the center for deciding which blue lines are "in-focus"
                    float weightedDistanceFromCenter = Mathf.Sqrt(Mathf.Pow(screenPosition.x - .5f, 2) + verticalImportanceFactor * Mathf.Pow(screenPosition.y - .5f, 2));
                    if (screenPosition.z < 0) { // the target is behind the player, off-screen
                        weightedDistanceFromCenter = 1;
                    }

                    // If the Magnetic could be targeted
                    // A Magnetic can be targeted if it is within an ellipse. The ellipse's x axis is targetFocusRadius, and its y is verticalImportanceFactor * targetFocusRadius.
                    if (weightedDistanceFromCenter < targetFocusRadius) {
                        // IF the new Magnetic is closer to the center of the screen than the previous most-center Magnetic
                        if (weightedDistanceFromCenter < smallestDistanceFromCenter) {
                            smallestDistanceFromCenter = weightedDistanceFromCenter;
                            centerObject = target;
                        }
                    }

                    // Set line properties
                    if (SettingsMenu.settingsData.renderblueLines == 1) {
                        //float closeness = Mathf.Pow(softForce, 2) / 64;
                        //float closeness = Mathf.Exp(-blueLineBrightnessFactor / softForce);
                        float closeness = Mathf.Exp(-blueLineStartupFactor * Mathf.Pow(1 / softForce, blueLineBrightnessFactor));
                        // Make lines in-focus if near the center of the screen
                        if (screenPosition.z < 0)
                            closeness = targetFocusOffScreenBound;
                        else
                            closeness *= targetFocusLowerBound + (1 - targetFocusLowerBound) * Mathf.Exp(-Mathf.Pow(weightedDistanceFromCenter + 1 - targetFocusRadius, targetFocusFalloffConstant));
                        target.SetBlueLine(
                            CenterOfMass,
                            blueLineWidthBaseFactor * target.Charge,
                            1,
                            new Color(0, closeness * lowLineColor, closeness * highLineColor, 1)
                            );
                    } else {
                        target.DisableBlueLine();
                    }
                } else {
                    // Magnetic is out of force-determined range
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
            if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < SettingsMenu.settingsData.maxPushRange * SettingsMenu.settingsData.maxPushRange) {
                if (target.LastAllomanticForce.magnitude > forceDetectionThreshold || target.LastAllomanticForce.magnitude == 0) { // == 0 only happens on the first frame of pushing
                    target.InRange = true;
                    target.SetBlueLine(
                        CenterOfMass,
                        Mathf.Pow(blueLineWidthBaseFactor * target.Charge, blueLineTargetedWidthFactor),
                        Mathf.Exp(-target.LastNetAllomanticForceOnAllomancer.magnitude / lightSaberConstant),
                        SettingsMenu.settingsData.pullTargetLineColor == 0 ? targetedBlueLine : targetedGreenLine);
                } else {
                    // Target is out of force-determined range
                    target.InRange = false;
                    target.DisableBlueLine();
                }
            } else {
                // Magnetic is out of max range
                target.InRange = false;
                target.DisableBlueLine();
            }
        }
        for (int i = 0; i < PushCount; i++) {
            Magnetic target = pushTargets[i];
            if ((target.CenterOfMass - CenterOfMass).sqrMagnitude < SettingsMenu.settingsData.maxPushRange * SettingsMenu.settingsData.maxPushRange) {
                if (target.LastAllomanticForce.magnitude > forceDetectionThreshold || target.LastAllomanticForce.magnitude == 0) { // == 0 only happens on the first frame of pushing
                    target.InRange = true;
                    target.SetBlueLine(
                        CenterOfMass,
                        Mathf.Pow(blueLineWidthBaseFactor * target.Charge, blueLineTargetedWidthFactor),
                        Mathf.Exp(-target.LastNetAllomanticForceOnAllomancer.magnitude / lightSaberConstant),
                        SettingsMenu.settingsData.pushTargetLineColor == 0 ? targetedBlueLine : targetedRedLine);
                } else {
                    // Target is out of force-determined range
                    target.InRange = false;
                    target.DisableBlueLine();
                }
            } else {
                target.InRange = false;
                target.DisableBlueLine();
            }
        }
        return centerObject;
    }

    private void StartBurningIronSteel() {
        if (!IsBurningIronSteel) { // just started burning metal
            GamepadController.Shake(.1f, .1f, .3f);
            IsBurningIronSteel = true;
            UpdateBurnRateMeter();
            HUD.BurnRateMeter.MetalLineText = AvailableNumberOfTargets.ToString();
            CameraController.ActiveCamera.cullingMask = ~0;
            SetPullRateTarget(1);
            SetPushRateTarget(1);
            forceMagnitudeTarget = 600;
        }
    }

    public void StopBurningIronSteel() {
        RemoveAllTargets();
        //if (IsBurningIronSteel) {
        if (HasHighlightedTarget) {
            HighlightedTarget.RemoveTargetGlow();
            HighlightedTarget = null;
        }
        IsBurningIronSteel = false;
        GamepadController.SetRumble(0, 0);
        ironBurnRate = 0;
        steelBurnRate = 0;
        forceMagnitudeTarget = 0;

        // make blue lines disappear
        CameraController.ActiveCamera.cullingMask = ~(1 << blueLineLayer);
        //for (int i = 0; i < metalLines.Count; i++) {
        //    metalLines[i].GetComponent<MeshRenderer>().enabled = false;
        //}
        //}
        RefreshHUD();
    }

    //private void StartPushPullingOnTargets(bool pulling) {
    //    if ((HasPullTarget && pulling) || !HasPushTarget) {
    //        for (int i = 0; i < PullCount; i++) {
    //            pullTargets[i].StartBeingPullPushed(pulling);
    //        }
    //    }
    //    if (HasPushTarget) {
    //        for (int i = 0; i < PushCount; i++) {
    //            pushTargets[i].StartBeingPullPushed(pulling);
    //        }
    //    }
    //}

    // Stop pushing or pulling on Pull targets
    private void StopOnPullTargets() {
        for (int i = 0; i < PullCount; i++) {
            pullTargets[i].StopBeingPullPushed();
        }
    }

    // Stop pushing or pulling on Push targets
    private void StopOnPushTargets() {
        for (int i = 0; i < PushCount; i++) {
            pushTargets[i].StopBeingPullPushed();
        }
    }

    // Removes a target by index
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

    // Removes a target by reference
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
            change = 100;
            if (forceMagnitudeTarget < 100) {
                change /= 10f;
            }
        } else if (change < 0) {
            change = -100;
            if (forceMagnitudeTarget <= 100) {
                change /= 10f;
            }
        }
        forceMagnitudeTarget = forceMagnitudeTarget + change;
        if (forceMagnitudeTarget <= 0.01f)
            StopBurningIronSteel();
    }

    // Increments or decrements the current burn rate
    private void ChangeBurnRateTarget(float change) {
        if (change > 0) {
            change = .10f;
            if (ironBurnRateTarget < .09f || steelBurnRateTarget < .09f) {
                change /= 10f;
            }
        } else if (change < 0) {
            change = -.10f;
            if (ironBurnRateTarget <= .10f || steelBurnRateTarget <= .10f) {
                change /= 10f;
            }
        }
        SetPullRateTarget(Mathf.Clamp(ironBurnRateTarget + change, 0, 1));
        SetPushRateTarget(Mathf.Clamp(steelBurnRateTarget + change, 0, 1));
    }

    // Sets the target Iron burn rate
    private void SetPullRateTarget(float rate) {
        if (rate > .001f) {
            ironBurnRateTarget = Mathf.Min(1, rate);
            if (HasPullTarget || HasPushTarget)
                GamepadController.SetRumbleRight(ironBurnRateTarget * GamepadController.rumbleFactor);
            else
                GamepadController.SetRumbleRight(0);
        } else {
            //IronPulling = false;
            ironBurnRateTarget = 0;
            GamepadController.SetRumbleRight(0);
        }
    }
    // Sets the target Steel burn rate
    private void SetPushRateTarget(float rate) {
        if (rate > .001f) {
            steelBurnRateTarget = Mathf.Min(1, rate);
            if (HasPullTarget || HasPushTarget)
                GamepadController.SetRumbleLeft(steelBurnRateTarget * GamepadController.rumbleFactor);
            else
                GamepadController.SetRumbleLeft(0);
        } else {
            //SteelPushing = false;
            steelBurnRateTarget = 0;
            GamepadController.SetRumbleLeft(0);
        }
    }

    private void LerpToBurnRates() {
        ironBurnRate = Mathf.Lerp(ironBurnRate, ironBurnRateTarget, burnRateLerpConstant);
        steelBurnRate = Mathf.Lerp(steelBurnRate, steelBurnRateTarget, burnRateLerpConstant);
        if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != 2 && (ironBurnRate < .001f || steelBurnRate < .001f)) {
            StopBurningIronSteel();
        }
    }

    private void UpdateBurnRateMeter() {
        if (IsBurningIronSteel) {
            if (SettingsMenu.settingsData.pushControlStyle == 1)
                HUD.BurnRateMeter.SetBurnRateMeterForceMagnitude(lastAllomanticForce, lastNormalForce, Mathf.Max(ironBurnRate, steelBurnRate), forceMagnitudeTarget);
            else
                HUD.BurnRateMeter.SetBurnRateMeterPercentage(lastAllomanticForce, lastNormalForce, Mathf.Max(ironBurnRate, steelBurnRate));
        } else {
            HUD.BurnRateMeter.Clear();
        }
    }
}
