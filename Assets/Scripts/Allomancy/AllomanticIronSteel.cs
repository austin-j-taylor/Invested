using UnityEngine;
/*
 * Controls Ironpulling and Steelpushing.
 * Should be attached to any Allomancer.
 */
[RequireComponent(typeof(Rigidbody))]
public class AllomanticIronSteel : Allomancer {

    public const int maxNumberOfTargets = 10;
    // Force calculation constants
    public const float chargePower = 1f / 8f;
    public const float lineOfSightFactor = 3/4f; // If a target is blocked by a wall, pushes are at 75% strength
    // Actual "Burn Rates" of iron and steel
    private const double gramsIronPerSecondPerNewton = .001f;
    private const double gramsSteelPerSecondPerNewton = gramsIronPerSecondPerNewton;
    public const double gramsPerSecondPassiveBurn = -.005f; // 5 mg/s for passively burning iron or steel to see metal lines
    // Blue metal line constants
    public const float lowLineColor = .1f;
    public const float highLineColor = .85f;
    public const float blueLineFirstPersonWidth = .02f;
    public const float blueLineThirdPersonWidth = .04f;
    protected const float blueLineChangeFactor = 1 / 4f;
    protected const float blueLineBrightnessFactor = 1 / 6f;
    // Simple metal boolean constants for passing to methods
    private const bool steel = false;
    private const bool iron = true;

    protected Rigidbody rb;

    public MetalReserve IronReserve { get; private set; }
    public MetalReserve SteelReserve { get; private set; }

    // Pull and Push Target members
    public TargetArray PullTargets { get; protected set; }
    public TargetArray PushTargets { get; protected set; }

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
    // Maximum possible Net Force on allomancer, regardless of burn rate
    public Vector3 LastMaximumNetForce { get; private set; } = Vector3.zero;
    private Vector3 thisFrameMaximumNetForce = Vector3.zero;

    //// Debug variables for viewing values in the Unity editor
    public float allomanticsForce;
    public float netAllomancersForce;
    public float netTargetsForce;
    public Vector3 allomanticsForces;
    public Vector3 resititutionFromTargetsForce;
    public Vector3 resititutionFromPlayersForce;
    public float percentOfTargetForceReturned;
    public float percentOfAllomancerForceReturned;
    // Metal burn rates
    // Used when burning metals, but not necessarily immediately Pushing or Pulling. Hence, they are "targets" and not the actual burn rate of the Allomancer.
    public float IronBurnRateTarget { get; set; }
    public float SteelBurnRateTarget { get; set; }
    // The passive burn rate of the allomancer.
    protected float IronPassiveBurn { get; set; }
    protected float SteelPassiveBurn { get; set; }
    //public float GreaterBurnRate {
    //    get {
    //        return Mathf.Max(IronBurnRateTarget, SteelBurnRateTarget);
    //    }
    //}
    public float GreaterPassiveBurn {
        get {
            return Mathf.Max(IronPassiveBurn, SteelPassiveBurn);
        }
    }

    private bool lastWasPulling = false;
    private bool lastWasPushing = false;
    private bool ironPulling = false;
    private bool steelPushing = false;
    public bool IronPulling {
        get {
            return ironPulling;
        }
        set {
            if (!value) {
                if (ironPulling) {
                    if (HasPullTarget)
                        StopOnPullTargets();
                    else
                        StopOnPushTargets();
                }
            }
            ironPulling = value;
        }
    }
    public bool SteelPushing {
        get {
            return steelPushing;
        }
        set {
            if (!value) {
                if (steelPushing) {
                    if (HasPushTarget)
                        StopOnPushTargets();
                    else
                        StopOnPullTargets();
                }
            }
            steelPushing = value;
        }
    }
    public bool IsBurningIronSteel { get; protected set; } = false;
    public float Strength { get; set; } = 1; // Allomantic Strength
    public float Charge { get; private set; } // Allomantic Charge
    public Vector3 CenterOfMass {
        get {
            return transform.TransformPoint(rb.centerOfMass);
        }
    }
    public float Mass {
        get {
            return rb.mass;
        }
    }

    protected virtual void Awake() {
        rb = GetComponent<Rigidbody>();
        Charge = Mathf.Pow(Mass, chargePower);
        IronReserve = gameObject.AddComponent<MetalReserve>();
        SteelReserve = gameObject.AddComponent<MetalReserve>();
        PullTargets = new TargetArray(maxNumberOfTargets);
        PushTargets = new TargetArray(maxNumberOfTargets);
        GameManager.AddAllomancer(this);
    }

    public virtual void Clear(bool clearTargets = true) {
        IsBurningIronSteel = false;

        if (clearTargets) {
            IronPulling = false;
            SteelPushing = false;
        } else { // prevents calling of StopOn____Targets
            ironPulling = false;
            steelPushing = false;
        }
        IronBurnRateTarget = 0;
        SteelBurnRateTarget = 0;
        IronPassiveBurn = 0;
        SteelPassiveBurn = 0;
        PullTargets.Clear(true, clearTargets);
        PushTargets.Clear(true, clearTargets);
        lastExpectedAllomancerAcceleration = Vector3.zero;
        //lastExpectedAllomancerVelocityChange = Vector3.zero;
        //LastExpectedAllomancerEnergyUsed = 0;
        LastAllomancerVelocity = Vector3.zero;
        LastNetForceOnAllomancer = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastAnchoredPushBoost = Vector3.zero;
        LastMaximumNetForce = Vector3.zero;
    }

    private void FixedUpdate() {
        if (!PauseMenu.IsPaused) {
            if (IsBurningIronSteel) {
                // Remove all targets that are out of pushing range
                // For Mouse/Keyboard, Iron and Steel burn rates are equal, so it's somewhat redundant to specify
                PullTargets.RemoveAllOutOfRange(IronBurnRateTarget, this);
                PushTargets.RemoveAllOutOfRange(SteelBurnRateTarget, this);

                // Calculate net charges to know how pushes will be distributed amongst targets
                float netPullTargetsCharge = PullTargets.NetCharge();
                float netPushTargetsCharge = PushTargets.NetCharge();
                float sumPullTargetsCharge = PullTargets.SumOfCharges();
                float sumPushTargetsCharge = PushTargets.SumOfCharges();

                // Calculate Allomantic Forces and APBs
                // Execute AFs and APBs on target and Allomancer
                if (PullingOnPullTargets) {
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, iron);
                        AddForce(PullTargets[i]);
                        BurnIron(PullTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (PushingOnPullTargets) {
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, steel);
                        AddForce(PullTargets[i]);
                        BurnSteel(PullTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (HasPullTarget) {
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, iron);
                    }
                }

                if (PullingOnPushTargets) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, iron);
                        AddForce(PushTargets[i]);
                        BurnIron(PushTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (PushingOnPushTargets) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, steel);
                        AddForce(PushTargets[i]);
                        BurnSteel(PushTargets[i].LastNetForceOnAllomancer.magnitude);
                    }
                } else if (HasPushTarget) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, steel);
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
                    if (!HasSteel)
                        StopBurning();
                }
                if (!HasSteel)
                    if (HasPushTarget)
                        PushTargets.Clear();


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

    /*
     * Calculates the maximum possible Allomantic Force between the allomancer and target.
     * Does not account for ABPs or burn rate.
     * Accounts for if a wall blocks line-of-sight with the target.
     */
    public static Vector3 CalculateAllomanticForce(Magnetic target, AllomanticIronSteel allomancer) {
        return allomancer.CalculateAllomanticForce(target.CenterOfMass, target.Charge); ;
    }

    private Vector3 CalculateAllomanticForce(Vector3 targetCenterOfMass, float targetCharge) {
        Vector3 distanceFactor;
        Vector3 positionDifference = targetCenterOfMass - CenterOfMass;

        switch (SettingsMenu.settingsData.forceDistanceRelationship) {
            case 0: {
                    distanceFactor = positionDifference.normalized * (1 - positionDifference.magnitude / SettingsMenu.settingsData.maxPushRange);
                    break;
                }
            case 1: {
                    distanceFactor = (positionDifference / positionDifference.sqrMagnitude);
                    break;
                }
            default: {
                    distanceFactor = positionDifference.normalized * Mathf.Exp(-positionDifference.magnitude / SettingsMenu.settingsData.distanceConstant);
                    break;
                }
        }
        // If the target is blocked by a wall, reduce the force.
        // Do the final calculation

        return SettingsMenu.settingsData.allomanticConstant * Strength * Charge * targetCharge * distanceFactor
                * ((Physics.Raycast(targetCenterOfMass, -positionDifference, out RaycastHit hit, (targetCenterOfMass - CenterOfMass).magnitude, GameManager.Layer_IgnoreCamera) && hit.transform != transform) ?
                lineOfSightFactor : 1); // If there is something blocking line-of-sight, the force is reduced.
    }

    /* 
     * Calculates the Allomantic Force and Anchored Push Boost that would affect this target, if it were Pushed or Pulled.
     * 
     * Pushing and Pulling
     * 
     * Formula subject to change:
     * F =  C * Burn rate * sixteenth or eighth root of (Allomancer mass * target mass) 
     *      / squared distance between the two
     *      / (depending on my mood) number of targets currently being pushed on
     *              (i.e. push on one target => 100% of the push going to them, push on three targets => 33% of each push going to each, etc. Not thought out very in-depth.)
     *  C: the Allomantic Constant.
     *  
     *  With the ANF, a push against a perfectly anchored metal structure is exactly twice as powerful as a push against a completely unanchored, freely-moving metal structure
     */
    private void CalculateForce(Magnetic target, float netMagneticCharge, float sumOfCharges, bool pulling) {
        target.LastWasPulled = pulling;

        // When pushing on multiple targets, you are effectively pushing on a mass equal to the sum of the masses of each target.
        // This effective mass is netMagneticMass, and is used here to calculate the effective charge against which the allomancer can push for this specific target
        //      SUCH THAT the sum of the pushes on each target will equal a single push on an imaginary target with a mass equal to the sum of each target's mass.

        float effectiveCharge = target.Charge / sumOfCharges * netMagneticCharge;
        Vector3 allomanticForce;
        Vector3 direction;

        Vector3 restitutionForceFromTarget;
        Vector3 restitutionForceFromAllomancer;
        
        if (SettingsMenu.settingsData.anchoredBoost == 3) {
            // Calculate the expected distribution of energy. This is affected by timeScale; if running at timeScale = .1, you are viewing the world 10x faster, and you have only 10% of Pres's power each unit time.
            // This is the energy that, without anchors, will be fully added to the system.
            // With anchors, only a portion of this energy will effectively be added to the system.
            // Next frame, that remaining unused energy will be added in the next section.
            //Vector3 maxEnergy = 100 * Time.timeScale * Time.timeScale * (target.CenterOfMass - CenterOfMass).normalized * (target.LastWasPulled ? 1 : -1);// Simulation physics
            Vector3 maxEnergy = CalculateAllomanticForce(target.CenterOfMass, effectiveCharge) * Time.fixedDeltaTime * Time.timeScale * (target.LastWasPulled ? 1 : -1);
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

        } else {
            allomanticForce = CalculateAllomanticForce(target.CenterOfMass, effectiveCharge) * (target.LastWasPulled ? 1 : -1) /* / (usingIronTargets ? PullCount : PushCount) */;
            direction = allomanticForce.normalized;

            thisFrameMaximumNetForce += allomanticForce;
            target.LastMaxPossibleAllomanticForce = allomanticForce;

            // Make the AF proportional to the burn rate
            allomanticForce *= (target.LastWasPulled ? IronBurnRateTarget : SteelBurnRateTarget);
            
            switch (SettingsMenu.settingsData.anchoredBoost) {
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
                           // Sometimes, the restitutionForceFromTarget is actually negative, rather than positive, unlike with the ANF. It contains the percentage of the AF that is subtracted from the AF to get the net AF.
                        Vector3 relativeVelocity = Vector3.Project(target.Velocity - rb.velocity, direction);
                        float velocityFactor = 1 - Mathf.Exp(-relativeVelocity.magnitude / SettingsMenu.settingsData.velocityConstant);
                        switch (SettingsMenu.settingsData.exponentialWithVelocitySignage) {
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
        }



        thisFrameAllomanticForce += allomanticForce;
        thisFrameAnchoredPushBoost += restitutionForceFromTarget;
        if (target.LastWasPulled) {
            if (IronBurnRateTarget == 0) {
                thisFrameMaximumNetForce += restitutionForceFromTarget;
            } else {
                thisFrameMaximumNetForce += restitutionForceFromTarget / IronBurnRateTarget;
            }
        } else {
            if (SteelBurnRateTarget == 0) {
                thisFrameMaximumNetForce += restitutionForceFromTarget;
            } else {
                thisFrameMaximumNetForce += restitutionForceFromTarget / SteelBurnRateTarget;
            }
        }
        target.LastAllomanticForce = allomanticForce;
        target.LastAnchoredPushBoostFromAllomancer = restitutionForceFromAllomancer;
        target.LastAnchoredPushBoostFromTarget = restitutionForceFromTarget;
    }

    /*
     * Applys the force that was calculated in CalculateForce to the target and player.
     * This effectively executes the Push or Pull.
     */
    private void AddForce(Magnetic target) {

        target.AddForce(target.LastNetForceOnTarget);
        rb.AddForce(target.LastNetForceOnAllomancer);

        //// Debug
        allomanticsForce = target.LastAllomanticForce.magnitude;
        allomanticsForces = target.LastAllomanticForce;
        netAllomancersForce = target.LastNetForceOnAllomancer.magnitude;
        resititutionFromTargetsForce = target.LastAnchoredPushBoostFromTarget;
        resititutionFromPlayersForce = target.LastAnchoredPushBoostFromAllomancer;
        percentOfTargetForceReturned = resititutionFromTargetsForce.magnitude / allomanticsForce;
        percentOfAllomancerForceReturned = resititutionFromPlayersForce.magnitude / allomanticsForce;
        netTargetsForce = target.LastNetForceOnTarget.magnitude;
    }

    /*
     * Start burning iron or steel. Passively burn iron or steel, depending on startIron.
     * Return true if successfully began burning metals, false otherwise.
     */
    protected virtual bool StartBurning(bool startIron) {
        if (IsBurningIronSteel || startIron && !HasIron || !startIron && !HasSteel)
            return false;
        IsBurningIronSteel = true;
        // Set burn rates to a low burn
        IronBurnRateTarget = .1f;
        SteelBurnRateTarget = .1f;
        if(startIron)
            lastWasPulling = true;
        else
            lastWasPushing = true;
        return true;
    }

    public virtual void StopBurning() {
        if (IsBurningIronSteel) {
            PullTargets.Clear();
            PushTargets.Clear();
            IronBurnRateTarget = 0;
            SteelBurnRateTarget = 0;
            IsBurningIronSteel = false;
            lastWasPulling = false;
            lastWasPushing = false;
        }
    }

    //Stop pushing or pulling on Pull targets
    public void StopOnPullTargets() {
        for (int i = 0; i < PullTargets.Count; i++) {
            PullTargets[i].StopBeingPullPushed();
        }
    }

    // Stop pushing or pulling on Push targets
    public void StopOnPushTargets() {
        for (int i = 0; i < PushTargets.Count; i++) {
            PushTargets[i].StopBeingPullPushed();
        }
    }

    // Consume iron for pull
    private void BurnIron(float force) {
        double burnedMass = gramsIronPerSecondPerNewton * force * Time.fixedDeltaTime;
        IronReserve.Mass -= burnedMass;
    }

    // Consume steel for push
    private void BurnSteel(float force) {
        double burnedMass = gramsSteelPerSecondPerNewton * force * Time.fixedDeltaTime;
        SteelReserve.Mass -= burnedMass;
    }

    /*
     * Add a target
     * If it's a pushTarget, remove it from pushTargets and move it to pullTargets
     */
    public void AddPullTarget(Magnetic target) {
        StartBurning(true);
        if (HasIron) {
            if (PushTargets.IsTarget(target)) {
                PushTargets.RemoveTarget(target, false);
            }
            if (target != null) {
                if (PullTargets.AddTarget(target, this))
                    CalculateForce(target, PullTargets.NetCharge(), PullTargets.SumOfCharges(), iron);
            }
        }
    }

    /*
     * Add a target
     * If it's a pullTarget, remove it from pullTargets and move it to pushTargets
     */
    public void AddPushTarget(Magnetic target) {
        StartBurning(false);
        if (HasSteel) {
            if (PullTargets.IsTarget(target)) {
                PullTargets.RemoveTarget(target, false);
            }
            if (target != null) {
                if (PushTargets.AddTarget(target, this))
                    CalculateForce(target, PushTargets.NetCharge(), PushTargets.SumOfCharges(), steel);
            }
        }
    }

    /*
     * Remove a target, regardless of it being a pull or push target.
     */
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

    public bool RemovePullTarget(Magnetic target) {
        return PullTargets.RemoveTarget(target);
    }

    public bool RemovePushTarget(Magnetic target) {
        return PushTargets.RemoveTarget(target);
    }

    public void RemovePullTargetAt(int index) {
        PullTargets.RemoveTargetAt(index);
    }

    public void RemovePushTargetAt(int index) {
        PushTargets.RemoveTargetAt(index);
    }
}