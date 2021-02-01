using UnityEngine;

/// <summary>
/// Controls Ironpulling and Steelpushing.
/// Should be attached to any Allomancer using Iron or Steel.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class AllomanticIronSteel : Allomancer {

    #region constants
    // Force calculation constants
    public const float chargePower = 1f / 8f;
    public const float lineOfSightFactor = 3 / 4f; // If a target is blocked by a wall, pushes are at 75% strength
    // Metal burn rates
    private const double gramsIronPerSecondPerNewton = .001f;
    private const double gramsSteelPerSecondPerNewton = gramsIronPerSecondPerNewton;
    public const double gramsPerSecondPassiveBurn = -.005f; // 5 mg/s for passively burning iron or steel to see metal lines
    // Bubble
    private const int bubbleSpeed = 10;
    // Simple metal boolean constants for passing to methods
    public const bool steel = false;
    public const bool iron = true;
    #endregion

    [HideInInspector]
    public Rigidbody rb;

    public MetalReserve IronReserve { get; private set; }
    public MetalReserve SteelReserve { get; private set; }

    // Pull and Push Target members
    public TargetArray PullTargets { get; protected set; }
    public TargetArray PushTargets { get; protected set; }
    // Optional TargetArray for Bubble pushing/pulling. Handled more by subclasses.
    public TargetArray BubbleTargets { get; protected set; } = null;
    protected Renderer bubbleRenderer = null;
    private float selectionBubbleRadius = 2;
    protected float SelectionBubbleRadius {
        get {
            return selectionBubbleRadius;
        }
        set {
            selectionBubbleRadius = value;
            // remove out-of-range targets
            BubbleTargets.RemoveAllOutOfBubble(SelectionBubbleRadius, this);
            BubbleTargets.Size = BubbleTargets.Count;
        }
    }

    #region macroProperties
    public bool PushingOrPullingOnTarget => PushingOrPulling && (HasPushTarget || HasPullTarget);
    public bool PushingOrPulling {
        get {
            return IronPulling || SteelPushing || BubbleIsOpen;
        }
    }
    public bool PullingOnPullTargets {
        get {
            return IronPulling && PullTargets.Count != 0;
        }
    }
    public bool PushingOnPullTargets {
        get {
            return SteelPushing && PushTargets.Count == 0;
        }
    }
    public bool PullingOnPushTargets {
        get {
            return IronPulling && PullTargets.Count == 0;
        }
    }
    public bool PushingOnPushTargets {
        get {
            return SteelPushing && PushTargets.Count != 0;
        }
    }

    public bool HasPullTarget {
        get {
            return PullTargets.Count != 0;
        }
    }
    public bool HasPushTarget {
        get {
            return PushTargets.Count != 0;
        }
    }
    public bool HasMarkedPullTarget {
        get {
            return HasPullTarget && PullTargets.VacuousCount != PullTargets.Count;
        }
    }
    public bool HasMarkedPushTarget {
        get {
            return HasPushTarget && PushTargets.VacuousCount != PushTargets.Count;
        }
    }
    public bool HasIron {
        get {
            return IronReserve.HasMass;
        }
    }
    public bool HasSteel {
        get {
            return SteelReserve.HasMass;
        }
    }
    public bool UsingBubble {
        get {
            return BubbleTargets != null;
        }
    }
    #endregion

    #region properties
    // Used for calculating the acceleration over the last frame for pushing/pulling
    public Vector3 LastAllomancerVelocity { get; private set; } = Vector3.zero;
    //private Vector3 lastExpectedAllomancerVelocityChange = Vector3.zero;
    //private float LastExpectedAllomancerEnergyUsed = 0;
    private Vector3 lastExpectedAllomancerAcceleration = Vector3.zero;

    // Used in calculation of Net force on allomancer, AF on allomancer, and APB on allomancer this/last frame
    public Vector3 LastNetForceOnAllomancer { get; private set; } = Vector3.zero;
    public Vector3 LastAllomanticForce { get; private set; } = Vector3.zero;
    private Vector3 thisFrameAllomanticForce = Vector3.zero;
    public Vector3 LastAnchoredPushBoost { get; private set; } = Vector3.zero;
    private Vector3 thisFrameAnchoredPushBoost = Vector3.zero;
    // Maximum possible Net Force on allomancer, regardless of burn percentage
    public Vector3 LastMaximumNetForce { get; private set; } = Vector3.zero;
    private Vector3 thisFrameMaximumNetForce = Vector3.zero;

    // Metal burn percentages
    // Used when burning metals, but not necessarily immediately Pushing or Pulling. Hence, they are "targets" and not the actual burn percentage of the Allomancer.
    public float IronBurnPercentageTarget { get; set; }
    public float SteelBurnPercentageTarget { get; set; }
    // Bubble burn percentage
    protected float BubbleBurnPercentageTarget { get; set; }
    // The passive burn percentages of the allomancer.
    protected float IronPassiveBurn { get; set; }
    protected float SteelPassiveBurn { get; set; }
    public float GreaterPassiveBurn {
        get {
            return Mathf.Max(IronPassiveBurn, SteelPassiveBurn);
        }
    }

    private bool lastWasPulling = false;
    private bool lastWasPushing = false;
    public bool IronPulling { get; set; }
    public bool SteelPushing { get; set; }
    // Bubble control
    public bool BubbleIsOpen { get; private set; } // true when the bubble is open at all
    public bool BubbleMetalStatus { get; protected set; } // true for iron, false for steel
    protected bool bubbleKeepOpen; // toggled by Q/E/MB4/MB5

    /*
     * When ControlledExternally is false, this Allomancy works normally.
     *  It sets a % of its maximum burn rates for iron and steel.
     * When it is true, the Allomancer Pushes/Pulls with the following External Commands every frame (if possible).
     */
    public bool ExternalControl { get; set; } = false;
    public float ExternalCommand { get; set; }

    public float BaseStrength { get; set; } = 1;
    public float Strength => BaseStrength * StrengthModifier;  // Allomantic Strength
    public float StrengthModifier { get; set; } = 1; // Another factor for Allomantic Strength
    protected float Charge { get; set; } // Allomantic Charge
    public Vector3 CenterOfMass {
        get {
            if (cachedCenterOfMassTime != Time.unscaledTime) {
                if (CustomCenterOfAllomancy == null)
                    cachedCenterOfMass = transform.TransformPoint(rb.centerOfMass);
                else
                    cachedCenterOfMass = CustomCenterOfAllomancy.position;
                cachedCenterOfMassTime = Time.unscaledTime;
            }
            return cachedCenterOfMass;
        }
    }
    private Vector3 cachedCenterOfMass;
    public Transform CustomCenterOfAllomancy { get; set; } = null; // if not null, the blue lines and forces lead to here.
    private float cachedCenterOfMassTime = -1;

    public float Mass {
        get {
            return rb.mass;
        }
    }
    #endregion

    #region clearing
    protected virtual void Awake() {
        rb = GetComponent<Rigidbody>();
        Charge = Mathf.Pow(Mass, chargePower);
        StrengthModifier = 1;
        IronReserve = gameObject.AddComponent<MetalReserve>();
        SteelReserve = gameObject.AddComponent<MetalReserve>();
        IronReserve.IsEndless = true;
        SteelReserve.IsEndless = true;
        InitArrays();
        GameManager.AddAllomancer(this);
    }
    protected virtual void InitArrays() {
        PullTargets = new TargetArray(TargetArray.smallArrayCapacity);
        PushTargets = new TargetArray(TargetArray.smallArrayCapacity);
    }

    public override void Clear() {
        base.Clear();
        StopBurning();
        lastExpectedAllomancerAcceleration = Vector3.zero;
        //lastExpectedAllomancerVelocityChange = Vector3.zero;
        //LastExpectedAllomancerEnergyUsed = 0;
        LastAllomancerVelocity = Vector3.zero;
        LastNetForceOnAllomancer = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastAnchoredPushBoost = Vector3.zero;
        LastMaximumNetForce = Vector3.zero;
        ExternalControl = false;
        ExternalCommand = 0;
        StrengthModifier = 1;
        CustomCenterOfAllomancy = null;
    }
    #endregion

    #region allomancyPhysics
    /// <summary>
    /// Calculate and apply forces to targets being pushed on.
    /// Drain metal reserves.
    /// </summary>
    protected virtual void FixedUpdate() {
        if (!GameManager.MenusController.pauseMenu.IsOpen) {
            if (IsBurning) {
                // Remove all targets that are out of pushing range
                // For Mouse/Keyboard, Iron and Steel burn percentages are equal, so it's somewhat redundant to specify
                if (!ExternalControl) {
                    PullTargets.RemoveAllOutOfRange(IronBurnPercentageTarget, this);
                    PushTargets.RemoveAllOutOfRange(SteelBurnPercentageTarget, this);
                }

                // Calculate net charges to know how pushes will be distributed amongst targets
                float netPullTargetsCharge = PullTargets.NetCharge();
                float netPushTargetsCharge = PushTargets.NetCharge();
                float sumPullTargetsCharge = PullTargets.SumOfCharges();
                float sumPushTargetsCharge = PushTargets.SumOfCharges();

                // Calculate Allomantic Forces and APBs
                // Execute AFs and APBs on target and Allomancer
                // First, handle Pushing in a Bubble
                if (UsingBubble) {
                    // Handle charge independently of manual tageting
                    float netBubbleTargetsCharge = BubbleTargets.NetCharge();
                    float sumBubbleTargetsCharge = BubbleTargets.SumOfCharges();

                    // If manually targeting a target within the bubble, don't apply the bubble's force on it
                    for (int i = 0; i < BubbleTargets.Count; i++) {
                        if (!PullTargets.IsTarget(BubbleTargets[i]) && !PushTargets.IsTarget(BubbleTargets[i])) {
                            if (BubbleMetalStatus == iron) {
                                CalculateForce(BubbleTargets[i], netBubbleTargetsCharge, sumBubbleTargetsCharge, iron, BubbleBurnPercentageTarget);
                                AddForce(BubbleTargets[i]);
                                BurnIron(BubbleTargets[i].LastNetForceOnAllomancer.magnitude);
                            } else {
                                CalculateForce(BubbleTargets[i], netBubbleTargetsCharge, sumBubbleTargetsCharge, steel, BubbleBurnPercentageTarget);
                                AddForce(BubbleTargets[i]);
                                BurnSteel(BubbleTargets[i].LastNetForceOnAllomancer.magnitude);
                            }
                        }
                    }
                }

                if (PullingOnPullTargets) {
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, iron, IronBurnPercentageTarget);
                        AddForce(PullTargets[i]);
                        BurnIron(PullTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (PushingOnPullTargets) {
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, steel, SteelBurnPercentageTarget);
                        AddForce(PullTargets[i]);
                        BurnSteel(PullTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (HasPullTarget && !(HasPushTarget && SteelPushing)) {  // If nothing else, at least calculate the force on your inactive targets
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, iron, IronBurnPercentageTarget);
                    }
                }

                if (PullingOnPushTargets) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, iron, IronBurnPercentageTarget);
                        AddForce(PushTargets[i]);
                        BurnIron(PushTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (PushingOnPushTargets) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, steel, SteelBurnPercentageTarget);
                        AddForce(PushTargets[i]);
                        BurnSteel(PushTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (HasPushTarget && !(HasPullTarget && IronPulling)) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, steel, SteelBurnPercentageTarget);
                    }
                }
                // Consume iron or steel for passively burning, depending on which metal was last used to push/pull
                if (HasIron && lastWasPulling || !HasSteel) {
                    IronReserve.Mass += IronPassiveBurn * gramsPerSecondPassiveBurn * Time.fixedDeltaTime;
                } else if (HasSteel && lastWasPushing || !HasIron) {
                    SteelReserve.Mass += SteelPassiveBurn * gramsPerSecondPassiveBurn * Time.fixedDeltaTime;
                } else {
                    IronReserve.Mass += IronPassiveBurn * gramsPerSecondPassiveBurn * Time.fixedDeltaTime / 2;
                    SteelReserve.Mass += SteelPassiveBurn * gramsPerSecondPassiveBurn * Time.fixedDeltaTime / 2;
                }

                // If out of metals, stop burning.
                if (!HasIron) {
                    if (HasPullTarget)
                        PullTargets.Clear();
                    if (UsingBubble && BubbleMetalStatus == iron)
                        BubbleClose();
                    if (!HasSteel)
                        StopBurning();
                }
                if (!HasSteel) {
                    if (HasPushTarget)
                        PushTargets.Clear();
                    if (UsingBubble && BubbleMetalStatus == steel)
                        BubbleClose();
                }


                lastWasPulling = (lastWasPulling || IronPulling) && !SteelPushing;
                lastWasPushing = (lastWasPushing || SteelPushing) && !IronPulling;

                // Update variables for calculating APBs and the like for next frame
                LastAllomancerVelocity = rb.velocity;
                LastAllomanticForce = thisFrameAllomanticForce;
                thisFrameAllomanticForce = Vector3.zero;
                LastAnchoredPushBoost = thisFrameAnchoredPushBoost;
                thisFrameAnchoredPushBoost = Vector3.zero;
                LastMaximumNetForce = thisFrameMaximumNetForce;
                thisFrameMaximumNetForce = Vector3.zero;
                LastNetForceOnAllomancer = LastAllomanticForce + LastAnchoredPushBoost;
                lastExpectedAllomancerAcceleration = LastNetForceOnAllomancer / rb.mass;
            }
        }
    }

    // Public function for CalculateAllomanticForce that only needs one argument
    public Vector3 CalculateAllomanticForce(Magnetic target, bool raycastForLOS = true) {
        return CalculateAllomanticForce(target.CenterOfMass, target.Charge, raycastForLOS);
    }

    /*
     * Calculates the maximum possible Allomantic Force between the allomancer and target.
     * Does not account for ABPs or burn percentage.
     * Accounts for no line-of-sight with the target decreasing force.
     * 
     *  Formula:
     *      F = A * S * C * d * w 
     *      
     *      F: Allomantic Force
     *      A: Allomantic Constant (~1000, subject to change)
     *      S: Allomantic Strength (1), a constant, different for each Allomancer
     *      C: Allomantic Charge (8th root of (Allomancer mass * target mass)), different for each Allomancer-target pair.
     *          Is the effect of mass on the force.
     *      d: Distance factor (between 0% and 100%)
     *          Is the effect of the distance between the target and Allomancer on the force.
     *      w: Wall factor (either 100% or 75%)
     *          If the target is behind a wall, the force is 75% as strong.
     *          
     *      raycastForLOS: Do a raycast towards the target to see if it is behind a wall (and reduce the force accordingly)
     *          
     */
    /// <summary>
    /// Calculates the maximum possible Allomantic Force between the allomancer and target.
    /// </summary>
    /// <param name="targetCenterOfMass">Center of mass for the target</param>
    /// <param name="targetCharge">Allomantic Charge for the target</param>
    /// <param name="raycastForLOS">whether the Allomantic Force should be lower if there is no line of sight</param>
    /// <returns>The Allomantic Force calculated for the target</returns>
    private Vector3 CalculateAllomanticForce(Vector3 targetCenterOfMass, float targetCharge, bool raycastForLOS = true) {
        Vector3 distanceFactor;
        Vector3 positionDifference = targetCenterOfMass - CenterOfMass;


        if (positionDifference == Vector3.zero) {
            // If the two objects truly occupy the same space, just say that one is slightly beneath the other. Physically this is impossible, but in simulation it can.
            positionDifference = new Vector3(0, -0.0001f, 0);
        }

        switch (SettingsMenu.settingsAllomancy.forceDistanceRelationship) {
            case 0: {
                    distanceFactor = positionDifference.normalized * (1 - positionDifference.magnitude / SettingsMenu.settingsAllomancy.maxPushRange);
                    break;
                }
            case 1: {
                    distanceFactor = (positionDifference / positionDifference.sqrMagnitude);
                    break;
                }
            default: {
                    distanceFactor = positionDifference.normalized * Mathf.Exp(-positionDifference.magnitude / SettingsMenu.settingsAllomancy.distanceConstant);
                    break;
                }
        }
        // Do the final calculation
        Vector3 force = SettingsMenu.settingsAllomancy.allomanticConstant * Strength * Charge * targetCharge * distanceFactor;
        // If there is something blocking line-of-sight, the force is reduced.
        //if (raycastForLOS && Physics.Raycast(targetCenterOfMass, -positionDifference, out RaycastHit hit, (targetCenterOfMass - CenterOfMass).magnitude, GameManager.Layer_IgnoreCamera) && hit.transform != transform) {
        //    force *= lineOfSightFactor;
        //}
        return force;
    }

    /* 
     * 
     * 
     * Formula:
     *      N = p * F + B
     *      
     *      N: Net Force
     *      p: Burn percentage (between 0% and 100%). Decided by the Allomancer to vary Push strength.
     *      F: Allomantic Force (see above function)
     *      B: Anchored Push Boost (between 0 and F in magnitude, usually)
     *          Is the reason that anchored targets provide stronger Pushes than unanchored Pushes.
     *          
     * */
    /// <summary>
    /// Calculates the Net Force between the allomancer and target, if it were Pushed or Pulled.
    /// </summary>
    /// <param name="target">The metal to calculate the force for</param>
    /// <param name="netMagneticCharge"></param>
    /// <param name="sumOfCharges">The sum of the charges on all targets being affected</param>
    /// <param name="pulling">true if Pulling. Flips the direction of the force.</param>
    /// <param name="burnPercentage">The % of of the maximum force to use.</param>
    private void CalculateForce(Magnetic target, float netMagneticCharge, float sumOfCharges, bool pulling, float burnPercentage) {
        target.LastWasPulled = pulling;
        /*
         * If you're Pushing on one target, then start Pushing on another, 
         *      your Push on each individually will decrease, but the net Push on you will increase.
         * Specifically:
         * When pushing on multiple targets, you are effectively pushing on a mass equal to the sum of the masses of each target.
         * This net mass is used here to calculate the effective charge against which the allomancer can Push for this specific target
         *      SUCH THAT the sum of the pushes on each target will equal a single push on an imaginary target
         *          with a mass equal to the sum of each target's mass.
         * 
         */
        float effectiveCharge = target.Charge / sumOfCharges * netMagneticCharge;

        Vector3 allomanticForce;
        Vector3 direction;

        Vector3 restitutionForceFromTarget;
        Vector3 restitutionForceFromAllomancer;

        if (SettingsMenu.settingsAllomancy.anchoredBoost != 3) {
            // APB: Disabled, Allomantic Normal Force, or Exponential with Velocity
            allomanticForce = CalculateAllomanticForce(target.CenterOfMass, effectiveCharge) * (pulling ? 1 : -1) /* / (usingIronTargets ? PullCount : PushCount) */;
            direction = allomanticForce.normalized;


            // If using an external force command, use that command instead if possible.
            if (ExternalControl) {
                // If you've already generated the desired force from other Pushes/Pulls this frame, clamp this Push/Pull's force
                float forceWanted = ExternalCommand - thisFrameMaximumNetForce.magnitude;
                if (forceWanted > 0) {
                    allomanticForce = Vector3.ClampMagnitude(allomanticForce, forceWanted);
                } else {
                    allomanticForce = Vector3.zero;
                }
            } else {
                // Make the AF proportional to the burn percentage, if the force is not overridden
                allomanticForce *= burnPercentage;
            }

            thisFrameMaximumNetForce += allomanticForce;
            target.LastMaxPossibleAllomanticForce = allomanticForce;

            switch (SettingsMenu.settingsAllomancy.anchoredBoost) {
                case 0: { // Disabled
                        restitutionForceFromTarget = Vector3.zero;
                        restitutionForceFromAllomancer = Vector3.zero;
                        break;
                    }
                case 1: { // ANF
                        if (pulling && !IronPulling || !pulling && !SteelPushing) { // If not actively pushing/pulling on this target, let the APB = AF for both target and allomancer
                            restitutionForceFromTarget = allomanticForce;
                            restitutionForceFromAllomancer = -allomanticForce;
                        } else {
                            if (target.IsStatic || target.IsPerfectlyAnchored) { // If target is perfectly anchored, pushes are perfectly resisted. Its ANF = AF.
                                restitutionForceFromTarget = allomanticForce;
                            } else {
                                // Calculate Allomantic Normal Forces
                                // Target is partially anchored
                                //calculate changes from the last frame
                                Vector3 lastTargetAcceleration = (target.Velocity - target.LastVelocity) / Time.fixedDeltaTime;
                                Vector3 unaccountedForTargetAcceleration = lastTargetAcceleration - target.LastExpectedAcceleration;// + Physics.gravity;
                                restitutionForceFromTarget = Vector3.Project(unaccountedForTargetAcceleration * target.NetMass, direction);
                            }
                            Vector3 lastAllomancerAcceleration = (rb.velocity - LastAllomancerVelocity) / Time.fixedDeltaTime;
                            Vector3 unaccountedForAllomancerAcceleration = lastAllomancerAcceleration - lastExpectedAllomancerAcceleration;
                            restitutionForceFromAllomancer = Vector3.Project(unaccountedForAllomancerAcceleration * rb.mass, direction);
                        }
                        if (SettingsMenu.settingsAllomancy.normalForceMax == 1) {
                            restitutionForceFromTarget = Vector3.ClampMagnitude(restitutionForceFromTarget, allomanticForce.magnitude);
                            restitutionForceFromAllomancer = Vector3.ClampMagnitude(restitutionForceFromAllomancer, allomanticForce.magnitude);
                        }

                        // Prevents the ANF from being negative relative to the AF and prevents the ANF from ever decreasing the AF below its original value
                        switch (SettingsMenu.settingsAllomancy.normalForceMin) {
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
                        if (SettingsMenu.settingsAllomancy.normalForceEquality == 1) {
                            if (restitutionForceFromAllomancer.magnitude > restitutionForceFromTarget.magnitude) {
                                restitutionForceFromTarget = -restitutionForceFromAllomancer;
                            } else {
                                restitutionForceFromAllomancer = -restitutionForceFromTarget;
                            }
                        }

                        break;
                    }
                default: { // EWV
                           // Sometimes, the restitutionForceFromTarget is actually negative, rather than positive, unlike with the ANF. It contains the percentage of the AF that is subtracted from the AF to get the net AF.
                        Vector3 relativeVelocity = Vector3.Project(target.Velocity - rb.velocity, direction);
                        float velocityFactor = 1 - Mathf.Exp(-relativeVelocity.magnitude / SettingsMenu.settingsAllomancy.velocityConstant);
                        switch (SettingsMenu.settingsAllomancy.exponentialWithVelocitySignage) {
                            case 0: {
                                    // Do nothing
                                    break;
                                }
                            case 1: {
                                    if (Vector3.Dot(relativeVelocity, direction) < 0) { // Decreases when dot > 0 ==> Decreases when relativeVelocity and direction of Allomancer's Push is same
                                        velocityFactor *= 0;
                                    }
                                    break;
                                }
                            case 2: {
                                    if (Vector3.Dot(relativeVelocity, direction) > 0) { // Decreases when dot < 0 ==> Decreases when relativeVelocity and direction of Allomancer's Push is opposite
                                        velocityFactor *= 0;
                                    }
                                    break;
                                }
                            case 3: {
                                    if (Vector3.Dot(relativeVelocity, direction) > 0) { // is additive when dot > 0 ==> Additive when relativeVelocity and direction of Allomancer's Push is same
                                        velocityFactor *= -1;
                                    }
                                    break;
                                }
                        }

                        restitutionForceFromAllomancer = allomanticForce * velocityFactor;
                        restitutionForceFromTarget = allomanticForce * -velocityFactor;

                        break;
                    }
            }
        } else {
            // APB: Distributed Energy
            // Experimental and has much messier math than other APBs

            // Calculate the expected distribution of energy. This is affected by timeScale; if running at timeScale = .1, you are viewing the world 10x faster, and you have only 10% of Pres's power each unit time.
            // This is the energy that, without anchors, will be fully added to the system.
            // With anchors, only a portion of this energy will effectively be added to the system.
            // Next frame, that remaining unused energy will be added in the next section.
            //Vector3 maxEnergy = 100 * Time.timeScale * Time.timeScale * (target.CenterOfMass - CenterOfMass).normalized * (target.LastWasPulled ? 1 : -1);// Simulation physics
            Vector3 maxEnergy = CalculateAllomanticForce(target.CenterOfMass, effectiveCharge) * Time.fixedDeltaTime * Time.timeScale * (pulling ? 1 : -1);
            direction = maxEnergy.normalized;

            Vector3 allomancerExpectedVelocityChange = Mathf.Sqrt(2 * maxEnergy.magnitude / (Mass + Mass * Mass / target.MagneticMass)) * direction;
            Vector3 targetExpectedVelocityChange = Mathf.Sqrt(2 * maxEnergy.magnitude / (target.MagneticMass + target.MagneticMass * target.MagneticMass / Mass)) * -direction;
            float allomancerExpectedEnergyUsed = .5f * Mass * allomancerExpectedVelocityChange.sqrMagnitude;
            float targetExpectedEnergyUsed = .5f * target.MagneticMass * targetExpectedVelocityChange.sqrMagnitude;

            // Add the unused energy from the last frame
            Vector3 lastAllomancerVelocityChange = (rb.velocity - LastAllomancerVelocity);
            Vector3 lastTargetVelocityChange = (target.Velocity - target.LastVelocity);

            float allomancerEnergy = allomancerExpectedEnergyUsed;
            float targetEnergy = targetExpectedEnergyUsed;
            if (pulling && IronPulling || !pulling && SteelPushing) {
                if (Vector3.Dot(allomancerExpectedVelocityChange, lastAllomancerVelocityChange) > 0) {
                    targetEnergy += allomancerExpectedEnergyUsed * (1 - Mathf.Clamp01(lastAllomancerVelocityChange.magnitude / allomancerExpectedVelocityChange.magnitude));
                }
                if (Vector3.Dot(targetExpectedVelocityChange, lastTargetVelocityChange) > 0) {
                    allomancerEnergy += targetExpectedEnergyUsed * (1 - Mathf.Clamp01(lastTargetVelocityChange.magnitude / targetExpectedVelocityChange.magnitude));
                }
            }

            float forceOfAllomancerPower = Mass * Mathf.Sqrt(2 * allomancerEnergy / Mass) / Time.fixedDeltaTime;
            float forceOfTargetPower = target.MagneticMass * Mathf.Sqrt(2 * targetEnergy / target.MagneticMass) / Time.fixedDeltaTime;

            // Whichever change in energy corresponds to a greater force is set to be the force experienced by both
            Vector3 fullForce;
            if (forceOfAllomancerPower > forceOfTargetPower)
                fullForce = forceOfAllomancerPower * direction;
            else
                fullForce = forceOfTargetPower * direction;

            // allomantic force is inherently equal to target.MagneticMass * target...VelocityChange / ...
            // Restitution forces represent the components of the Improved Apparent force that come from the opposite's wasted energy
            //allomanticForce = (LastAllomanticForce + Mass * allomancerExpectedVelocityChange / Time.fixedDeltaTime) / 2;
            //restitutionForceFromTarget = (target.LastAnchoredPushBoostFromTarget + fullForce - allomanticForce) / 2;
            //restitutionForceFromAllomancer = (target.LastAnchoredPushBoostFromAllomancer - (fullForce - allomanticForce)) / 2;
            allomanticForce = Mass * allomancerExpectedVelocityChange / Time.fixedDeltaTime;
            restitutionForceFromTarget = fullForce - allomanticForce;
            restitutionForceFromAllomancer = -(fullForce - allomanticForce);
        }


        // If using an external force command, all that matters is that the calculated force is at least the desired force.
        // If so, make sure the AF and the ABP sum up to the external command.
        if (ExternalControl) {
            allomanticForce -= restitutionForceFromTarget;
        }

        if (target.LastWasPulled) {
            if (IronBurnPercentageTarget == 0 || ExternalControl) {
                thisFrameMaximumNetForce += restitutionForceFromTarget;
            } else {
                thisFrameMaximumNetForce += restitutionForceFromTarget / IronBurnPercentageTarget;
            }
        } else {
            if (SteelBurnPercentageTarget == 0 || ExternalControl) {
                thisFrameMaximumNetForce += restitutionForceFromTarget;
            } else {
                thisFrameMaximumNetForce += restitutionForceFromTarget / SteelBurnPercentageTarget;
            }
        }

        thisFrameAllomanticForce += allomanticForce;
        thisFrameAnchoredPushBoost += restitutionForceFromTarget;

        target.LastAllomanticForce = allomanticForce;
        target.LastAnchoredPushBoostFromAllomancer = restitutionForceFromAllomancer;
        target.LastAnchoredPushBoostFromTarget = restitutionForceFromTarget;
    }

    /// <summary>
    /// Applys the force that was calculated in CalculateForce to the target and player.
    /// This effectively executes the Push or Pull.
    /// </summary>
    /// <param name="target">The magnetic to add a force to</param>
    private void AddForce(Magnetic target) {

        target.AddForce(target.LastNetForceOnTarget, target.LastAllomanticForceOnTarget);
        rb.AddForce(target.LastNetForceOnAllomancer);

        // Debug
        //allomanticsForce = target.LastAllomanticForce.magnitude;
        //allomanticsForces = target.LastAllomanticForce;
        //netAllomancersForce = target.LastNetForceOnAllomancer.magnitude;
        //resititutionFromTargetsForce = target.LastAnchoredPushBoostFromTarget;
        //resititutionFromPlayersForce = target.LastAnchoredPushBoostFromAllomancer;
        //percentOfTargetForceReturned = resititutionFromTargetsForce.magnitude / allomanticsForce;
        //percentOfAllomancerForceReturned = resititutionFromPlayersForce.magnitude / allomanticsForce;
        //netTargetsForce = target.LastNetForceOnTarget.magnitude;
    }
    #endregion

    #region otherAllomancy
    /// <summary>
    /// Start burning iron or steel.
    /// Passively burn iron or steel, depending on startIron.
    /// </summary>
    /// <param name="startIron"></param>
    /// <returns>true if not already burning metals and successfully started burning, false otherwise</returns>
    public virtual bool StartBurning(bool startIron = iron) {
        if (IsBurning || startIron && !HasIron || !startIron && !HasSteel)
            return false;
        IsBurning = true;
        // Set burn percentages to a low burn
        IronBurnPercentageTarget = .1f;
        SteelBurnPercentageTarget = .1f;
        BubbleBurnPercentageTarget = 1;
        if (bubbleKeepOpen) {
            BubbleOpen(BubbleMetalStatus);
        }
        if (startIron)
            lastWasPulling = true;
        else
            lastWasPushing = true;
        return true;
    }

    /// <summary>
    /// Stops burning iron and steel.
    /// </summary>
    /// <param name="clearTargets">Also remove marked push/pull targets</param>
    public virtual void StopBurning(bool clearTargets = true) {
        if (IsBurning) {
            if (clearTargets) {
                PullTargets.Clear();
                PushTargets.Clear();
            }
            if (UsingBubble) {
                BubbleClose(clearTargets);
            }
            IronBurnPercentageTarget = 0;
            SteelBurnPercentageTarget = 0;
            BubbleBurnPercentageTarget = 0;
            IsBurning = false;
            lastWasPulling = false;
            lastWasPushing = false;
        }
    }

    /// <summary>
    /// Consume iron for a Pull
    /// </summary>
    /// <param name="force">the force to consume metal for</param>
    private void BurnIron(float force) {
        double burnedMass = gramsIronPerSecondPerNewton * force * Time.fixedDeltaTime;
        IronReserve.Mass -= burnedMass;
    }

    /// <summary>
    /// Consume steel for a Push
    /// </summary>
    /// <param name="force">the force to consume metal for</param>
    private void BurnSteel(float force) {
        double burnedMass = gramsSteelPerSecondPerNewton * force * Time.fixedDeltaTime;
        SteelReserve.Mass -= burnedMass;
    }

    /// <summary>
    /// Refreshes the Bubble that shows the range of selecting targets
    /// </summary>
    /// <param name="metal">true to open the iron Buble, false for steel</param>
    public void BubbleOpen(bool metal) {
        StartBurning(metal);
        // if cannot open that bubble because we're out of metal, don't open it at all
        if (metal == iron && !HasIron || metal == steel && !HasSteel) {
            BubbleClose();
        } else {
            if (!BubbleIsOpen) {
                BubbleIsOpen = true;
                bubbleRenderer.enabled = true;
            }
            if (BubbleIsOpen) {
                // set color
                if (metal == iron && BubbleMetalStatus != iron) {
                    bubbleRenderer.material.color = AllomechanicalGlower.ColorIronTransparent;
                    bubbleRenderer.material.SetInt("_Speed", bubbleSpeed);
                    BubbleMetalStatus = iron;
                } else if (metal == steel && BubbleMetalStatus != steel) {
                    bubbleRenderer.material.color = AllomechanicalGlower.ColorSteelTransparent;
                    bubbleRenderer.material.SetInt("_Speed", -bubbleSpeed);
                    BubbleMetalStatus = steel;
                }
            }
        }
    }
    protected void InitBubble() {
        BubbleMetalStatus = steel;
        bubbleRenderer.material.color = AllomechanicalGlower.ColorSteelTransparent;
        bubbleRenderer.material.SetInt("_Speed", -bubbleSpeed);
        BubbleMetalStatus = steel;
        BubbleClose();
    }
    /// <summary>
    /// Opens the bubble with the most recent polarity.
    /// </summary>
    public void BubbleOpen() {
        BubbleOpen(BubbleMetalStatus);
    }

    /// <summary>
    /// Closes the Bubble.
    /// </summary>
    /// <param name="clearTargets">if true, will invoke each removed target's Clear()</param>
    public void BubbleClose(bool clearTargets = true) {
        if (BubbleIsOpen) {
            BubbleIsOpen = false;
            bubbleRenderer.enabled = false;
            if (clearTargets)
                BubbleTargets.Clear();
        }
    }

    /// <summary>
    /// Sets the VISUAL radius of the bubble.
    /// </summary>
    /// <param name="visualRadius">the radius of the bubble</param>
    protected void BubbleSetVisualSize(float visualRadius) {
        // set size
        float scale = visualRadius * 2;
        bubbleRenderer.transform.localScale = new Vector3(scale, scale, scale);
    }


    #endregion

    #region targetManipulation
    /// <summary>
    /// Add a target to the PullTargets.
    /// </summary>
    /// <param name="target">the metal to add</param>
    /// <param name="allowInBothArrays">if it's a pushTarget, remove it from pushTargets and move it to pullTargets</param>
    /// <param name="vacuous">vacuous adding: remove as soon as another target is added</param>
    public void AddPullTarget(Magnetic target, bool allowInBothArrays = false, bool vacuous = false) {
        StartBurning(true);
        if (HasIron) {
            if (!allowInBothArrays && PushTargets.IsTarget(target)) {
                PushTargets.RemoveTarget(target, false);
            }
            if (target != null) {
                if (PullTargets.AddTarget(target, vacuous))
                    CalculateForce(target, PullTargets.NetCharge(), PullTargets.SumOfCharges(), iron, IronBurnPercentageTarget);
            }
        }
    }

    /// <summary>
    /// Add a target to the PushTargets.
    /// </summary>
    /// <param name="target">the metal to add</param>
    /// <param name="allowInBothArrays">if it's a pullTarget, remove it from pullTargets and move it to pushTargets</param>
    /// <param name="vacuous">vacuous adding: remove as soon as another target is added</param>
    public void AddPushTarget(Magnetic target, bool allowInBothArrays = false, bool vacuous = false) {
        StartBurning(false);
        if (HasSteel) {
            if (!allowInBothArrays && PullTargets.IsTarget(target)) {
                PullTargets.RemoveTarget(target, false);
            }
            if (target != null) {
                if (PushTargets.AddTarget(target, vacuous))
                    CalculateForce(target, PushTargets.NetCharge(), PushTargets.SumOfCharges(), steel, SteelBurnPercentageTarget);
            }
        }
    }

    /// <summary>
    /// Remove a target from Push or Pull targets.
    /// </summary>
    /// <param name="target">the Magnetic to remove</param>
    /// <param name="startWithPullTargets">true to start searching for Pull targets, false for Push targets</param>
    /// <returns></returns>
    public bool RemoveTarget(Magnetic target, bool startWithPullTargets = false) {
        // Try to remove target from both arrays, if necessary
        if (startWithPullTargets) {
            if (PullTargets.RemoveTarget(target)) {
                return true;
            } else return PushTargets.RemoveTarget(target);
        } else {
            if (PushTargets.RemoveTarget(target)) {
                return true;
            } else return PullTargets.RemoveTarget(target);
        }
    }

    /// <summary>
    /// Removes a Pull target.
    /// </summary>
    /// <param name="target">the metal to removed</param>
    /// <returns>true if it was found and removed</returns>
    public bool RemovePullTarget(Magnetic target) {
        return PullTargets.RemoveTarget(target);
    }

    /// <summary>
    /// Removes a Push target.
    /// </summary>
    /// <param name="target">the metal to removed</param>
    /// <returns>true if it was found and removed</returns>
    public bool RemovePushTarget(Magnetic target) {
        return PushTargets.RemoveTarget(target);
    }

    /// <summary>
    /// Removes the target at the given index
    /// </summary>
    /// <param name="index">the index to remove</param>
    public void RemovePullTargetAt(int index) {
        PullTargets.RemoveTargetAt(index);
    }

    /// <summary>
    /// Removes the target at the given index
    /// </summary>
    /// <param name="index">the index to remove</param>
    public void RemovePushTargetAt(int index) {
        PushTargets.RemoveTargetAt(index);
    }

    /// <summary>
    /// Removes the target from the Bubble selection
    /// </summary>
    /// <param name="target">the target to remove</param>
    public void RemoveBubbleTarget(Magnetic target) {
        BubbleTargets.RemoveTarget(target);
    }
    #endregion

}