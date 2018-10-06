using UnityEngine;
/*
 * Controls Ironpulling and Steelpushing.
 * Should be attached to any Allomancer.
 */
public class AllomanticIronSteel : MonoBehaviour {

    public const float chargePower = 1f / 8f;
    public const int maxNumberOfTargets = 10;

    private const float slowBurn = .1f;
    // Simple metal boolean constants for passing to methods
    private const bool steel = false;
    private const bool iron = true;
    
    // Pull and Push Target members
    public TargetArray PullTargets { get; private set; }
    public TargetArray PushTargets { get; private set; }

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

    // Used for calculating the acceleration over the last frame for pushing/pulling
    private Vector3 lastAllomancerVelocity = Vector3.zero;
    private Vector3 lastExpectedAllomancerAcceleration = Vector3.zero;

    public Vector3 LastNetForceOnAllomancer { get; private set; } = Vector3.zero;
    public Vector3 LastAnchoredPushBoost { get; private set; } = Vector3.zero;
    private Vector3 thisFrameAnchoredPushBoost = Vector3.zero;
    public Vector3 LastAllomanticForce { get; private set; } = Vector3.zero;
    private Vector3 thisFrameAllomanticForce = Vector3.zero;
    
    private Vector3 thisFrameMaximumNetForce = Vector3.zero;
    public Vector3 LastMaximumNetForce { get; private set; } = Vector3.zero;
    
    // Debug variables for viewing values in the Unity editor
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

    public float GreaterBurnRate {
        get {
            return Mathf.Max(IronBurnRateTarget, SteelBurnRateTarget);
        }
    }

    private Transform centerOfMass;
    private Rigidbody rb;
    public bool IronPulling { get; set; } = false;
    public bool SteelPushing { get; set; } = false;
    public bool IsBurningIronSteel { get; set; } = false;
    // Allomantic Charge
    public float Charge { get; private set; }
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

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        centerOfMass = transform.Find("Body/Center of Mass");
        centerOfMass.localPosition = Vector3.zero;
        Charge = Mathf.Pow(Mass, chargePower);
        PullTargets = new TargetArray(maxNumberOfTargets);
        PushTargets = new TargetArray(maxNumberOfTargets);
    }

    public void Clear(bool clearTargets = true) {
        IsBurningIronSteel = false;
        IronPulling = false;
        SteelPushing = false;
        IronBurnRateTarget = 0;
        SteelBurnRateTarget = 0;
        PullTargets.Clear(true, clearTargets);
        PushTargets.Clear(true, clearTargets);
        lastExpectedAllomancerAcceleration = Vector3.zero;
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
                PullTargets.RemoveAllOutOfRange(IronBurnRateTarget);
                PushTargets.RemoveAllOutOfRange(SteelBurnRateTarget);
                
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
                    }
                } else if (PushingOnPullTargets) {
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, steel);
                        AddForce(PullTargets[i]);
                    }
                } else if(HasPullTarget) {
                    for (int i = 0; i < PullTargets.Count; i++) {
                        CalculateForce(PullTargets[i], netPullTargetsCharge, sumPullTargetsCharge, iron);
                    }
                }

                if (PullingOnPushTargets) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, iron);
                        AddForce(PushTargets[i]);
                    }
                } else if (PushingOnPushTargets) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, steel);
                        AddForce(PushTargets[i]);
                    }
                } else if (HasPushTarget) {
                    for (int i = 0; i < PushTargets.Count; i++) {
                        CalculateForce(PushTargets[i], netPushTargetsCharge, sumPushTargetsCharge, steel);
                    }
                }

                // Update variables for calculating APBs and the like for next frame
                lastAllomancerVelocity = rb.velocity;
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
     * Calculates the unaltered, maximum possible Allomantic Force between the allomancer and target.
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
        return SettingsMenu.settingsData.allomanticConstant * targetCharge * Charge * distanceFactor;
    }

    /* 
     * Calculates the Allomantic Force and Anchored Push Boost that would affect this target, if it were Pushed or Pulled.
     * 
     * 
     * Pushing and Pulling
     * 
     * Formula subject to change:
     * F =  C * Burn rate * sixteenth or eighth root (depending on my mood) of (Allomancer mass * target mass) 
     *      / squared distance between the two
     *      / (depending on my mood) number of targets currently being pushed on
     *              (i.e. push on one target => 100% of the push going to them, push on three targets => 33% of each push going to each, etc. Not thought out very in-depth.)
     *  C: the Allomantic Constant.
     */
    private void CalculateForce(Magnetic target, float netMagneticCharge, float sumOfCharges, bool pulling) {
        target.LastWasPulled = pulling;

        // When pushing on multiple targets, you are effectively pushing on a mass equal to the sum of the masses of each target.
        // This effective mass is netMagneticMass, and is used here to calculate the effective charge against which the allomancer can push for this specific target
        //      SUCH THAT the sum of the pushes on each target will equal a single push on an imaginary target with a mass equal to the sum of each target's mass.

        float effectiveCharge = target.Charge / sumOfCharges * netMagneticCharge;

        Vector3 allomanticForce = CalculateAllomanticForce(target.CenterOfMass, effectiveCharge) * (target.LastWasPulled ? 1 : -1) /* / (usingIronTargets ? PullCount : PushCount) */;
        Vector3 direction = allomanticForce.normalized;

        thisFrameMaximumNetForce += allomanticForce;
        target.LastMaxPossibleAllomanticForce = allomanticForce;

        // Make the AF proportional to the burn rate
        allomanticForce *= (target.LastWasPulled ? IronBurnRateTarget : SteelBurnRateTarget);

        Vector3 restitutionForceFromTarget;
        Vector3 restitutionForceFromAllomancer;

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
                            restitutionForceFromTarget = Vector3.Project(unaccountedForTargetAcceleration * target.NetMass, direction);
                        }
                    }

                    Vector3 lastAllomancerAcceleration = (rb.velocity - lastAllomancerVelocity) / Time.fixedDeltaTime;
                    Vector3 unaccountedForAllomancerAcceleration = lastAllomancerAcceleration - lastExpectedAllomancerAcceleration;
                    restitutionForceFromAllomancer = Vector3.Project(unaccountedForAllomancerAcceleration * rb.mass, direction);

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
                        velocityFactorTarget = 1 - Mathf.Exp(-(Vector3.Project(target.Velocity, direction).magnitude / SettingsMenu.settingsData.velocityConstant));
                        velocityFactorAllomancer = 1 - Mathf.Exp(-(Vector3.Project(rb.velocity, direction).magnitude / SettingsMenu.settingsData.velocityConstant));
                    } else {
                        velocityFactorTarget = 1 - Mathf.Exp(-(Vector3.Project(rb.velocity - target.Velocity, direction).magnitude / SettingsMenu.settingsData.velocityConstant));
                        velocityFactorAllomancer = 1 - Mathf.Exp(-(Vector3.Project(target.Velocity - rb.velocity, direction).magnitude / SettingsMenu.settingsData.velocityConstant));
                    }
                    switch (SettingsMenu.settingsData.exponentialWithVelocitySignage) {
                        case 0: {
                                // Do nothing
                                break;
                            }
                        case 1: {
                                if (Vector3.Dot(rb.velocity, direction) > 0) {
                                    velocityFactorTarget *= 0;
                                }
                                if (Vector3.Dot(target.Velocity, direction) < 0) {
                                    velocityFactorAllomancer *= 0;
                                }
                                break;
                            }
                        default: {
                                if (Vector3.Dot(rb.velocity, direction) > 0) {
                                    velocityFactorTarget *= -1;
                                }
                                if (Vector3.Dot(target.Velocity, direction) < 0) {
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
        thisFrameAllomanticForce += allomanticForce;
        thisFrameAnchoredPushBoost += restitutionForceFromTarget;
        if(target.LastWasPulled) {
            if(IronBurnRateTarget == 0) {
                thisFrameMaximumNetForce += restitutionForceFromTarget;
            } else {
                thisFrameMaximumNetForce += restitutionForceFromTarget / IronBurnRateTarget;
            }
        } else {
            if(SteelBurnRateTarget == 0) {
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

        target.AddForce(target.LastNetAllomanticForceOnTarget);
        rb.AddForce(target.LastNetAllomanticForceOnAllomancer);

        // Debug
        allomanticsForce = target.LastAllomanticForce.magnitude;
        allomanticsForces = target.LastAllomanticForce;
        netAllomancersForce = target.LastNetAllomanticForceOnAllomancer.magnitude;
        resititutionFromTargetsForce = target.LastAnchoredPushBoostFromTarget;
        resititutionFromPlayersForce = target.LastAnchoredPushBoostFromAllomancer;
        percentOfTargetForceReturned = resititutionFromTargetsForce.magnitude / allomanticsForce;
        percentOfAllomancerForceReturned = resititutionFromPlayersForce.magnitude / allomanticsForce;
        netTargetsForce = target.LastNetAllomanticForceOnTarget.magnitude;
    }

    public void StartBurning() {
        if (!IsBurningIronSteel) {
            IsBurningIronSteel = true;
            // Set burn rates to slow burn, to start
            IronBurnRateTarget = slowBurn;
            SteelBurnRateTarget = slowBurn;

            // If this component belongs to the player
            if (tag == "Player")
                GetComponent<PlayerPullPushController>().StartBurningIronSteel();
        }

    }

    public void StopBurning() {
        if (IsBurningIronSteel) {
            PullTargets.Clear();
            PushTargets.Clear();
            IronBurnRateTarget = 0;
            SteelBurnRateTarget = 0;
            IsBurningIronSteel = false;

            // If this component belongs to the player
            if (tag == "Player")
                GetComponent<PlayerPullPushController>().StopBurningIronSteel();
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

    /*
     * Add a target
     * If it's a pushTarget, remove it from pushTargets and move it to pullTargets
     */
    public void AddPullTarget(Magnetic target) {
        if (!IsBurningIronSteel)
            StartBurning();
        if (PushTargets.IsTarget(target)) {
            PushTargets.RemoveTarget(target, false);
        }
        PullTargets.AddTarget(target, this);
    }

    /*
     * Add a target
     * If it's a pullTarget, remove it from pullTargets and move it to pushTargets
     */
    public void AddPushTarget(Magnetic target) {
        if (!IsBurningIronSteel)
            StartBurning();
        if (PullTargets.IsTarget(target)) {
            PullTargets.RemoveTarget(target, false);
        }
        PushTargets.AddTarget(target, this);
    }

    /*
     * Remove a target, regardless of it being a pull or push target.
     */
    public bool RemoveTarget(Magnetic target, bool startWithPullTargets) {
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