using UnityEngine;
/*
 * Controls of Ironpulling and Steelpushing.
 * Should be attached to any Allomancer.
 */
public class AllomanticIronSteel : MonoBehaviour {

    public const float chargePower = 1f / 8f;
    public const int maxNumberOfTargets = 10;
    // Simple metal booleans for passing to methods
    private const bool steel = false;
    private const bool iron = true;


    // Pull and Push Target members
    public TargetArray PullTargets { get; private set; }
    public TargetArray PushTargets { get; private set; }
    //public int AvailableNumberOfTargets { get; private set; } = 1;

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
    //private Vector3 thisFrameNetForceOnAllomancer = Vector3.zero;
    public Vector3 LastNormalForce { get; private set; } = Vector3.zero;
    private Vector3 thisFrameNormalForce = Vector3.zero;
    public Vector3 LastAllomanticForce { get; private set; } = Vector3.zero;
    private Vector3 thisFrameAllomanticForce = Vector3.zero;

    //private Vector3 thisFrameMaximumAllomanticForce = Vector3.zero;
    //private Vector3 lastMaximumAllomanticForce = Vector3.zero;
    private Vector3 thisFrameMaximumNetForce = Vector3.zero;
    public Vector3 LastMaximumNetForce { get; private set; } = Vector3.zero;
    //private Vector3 thisFrameMaximumNormalForce = Vector3.zero;
    //private Vector3 lastMaximumNormalForce = Vector3.zero;


    // Metal burn rates
    public float IronBurnRate { get; set; }
    public float SteelBurnRate { get; set; }

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
        IronBurnRate = 0;
        SteelBurnRate = 0;
        PullTargets.Clear(true, clearTargets);
        PushTargets.Clear(true, clearTargets);
        lastExpectedAllomancerAcceleration = Vector3.zero;
        LastNetForceOnAllomancer = Vector3.zero;
        LastAllomanticForce = Vector3.zero;
        LastNormalForce = Vector3.zero;
        LastMaximumNetForce = Vector3.zero;
    }

    private void FixedUpdate() {
        if (!PauseMenu.IsPaused) {
            if (IsBurningIronSteel) {
                // Remove all targets that are out of pushing range
                PullTargets.RemoveAllOutOfRange();
                PushTargets.RemoveAllOutOfRange();

                CalculatePullForces();
                CalculatePushForces();

                ExecutePushesAndPulls();

                lastAllomancerVelocity = rb.velocity;

                LastAllomanticForce = thisFrameAllomanticForce;
                thisFrameAllomanticForce = Vector3.zero;
                LastNormalForce = thisFrameNormalForce;
                thisFrameNormalForce = Vector3.zero;
                //lastMaximumAllomanticForce = thisFrameMaximumAllomanticForce;
                //lastMaximumNormalForce = thisFrameMaximumNormalForce;
                LastMaximumNetForce = thisFrameMaximumNetForce;
                thisFrameMaximumNetForce = Vector3.zero;
                LastNetForceOnAllomancer = LastAllomanticForce + LastNormalForce;
                lastExpectedAllomancerAcceleration = LastNetForceOnAllomancer / rb.mass;
                //thisFrameNetForceOnAllomancer = Vector3.zero;
                //thisFrameMaximumAllomanticForce = Vector3.zero;
                //thisFrameMaximumNormalForce = Vector3.zero;
            }
        }
    }

    private void CalculatePullForces() {
        for (int i = 0; i < PullTargets.Count; i++) {
            CalculateForce(PullTargets[i], iron);
        }
    }

    private void CalculatePushForces() {
        for (int i = 0; i < PushTargets.Count; i++) {
            CalculateForce(PushTargets[i], steel);
        }
    }

    private void ExecutePushesAndPulls() {
        if (PullingOnPullTargets) {
            PullOnTargets(iron);
        } else if (PushingOnPullTargets) {
            PushOnTargets(iron);
        }

        if (PullingOnPushTargets) {
            PullOnTargets(steel);
        } else if (PushingOnPushTargets) {
            PushOnTargets(steel);
        }
    }

    private void PullOnTargets(bool usingIronTargets) {
        if (usingIronTargets) {
            for (int i = 0; i < PullTargets.Count; i++) {
                AddForce(PullTargets[i], iron);
            }
        } else {
            for (int i = 0; i < PushTargets.Count; i++) {
                AddForce(PushTargets[i], iron);
            }
        }
    }

    private void PushOnTargets(bool usingIronTargets) {
        if (usingIronTargets) {
            for (int i = 0; i < PullTargets.Count; i++) {
                AddForce(PullTargets[i], steel);
            }
        } else {
            for (int i = 0; i < PushTargets.Count; i++) {
                AddForce(PushTargets[i], steel);
            }
        }
    }

    //private void ResetPullStatus(bool usingIronTargets) {
    //    if (usingIronTargets) {
    //        for (int i = 0; i < PullCount; i++) {
    //            PullTargets[i].LastWasPulled = true;
    //        }
    //    } else {
    //        for (int i = 0; i < PushCount; i++) {
    //            PushTargets[i].LastWasPulled = false;
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
    /*
     * Calculates the unaltered, maximum possible Allomantic Force between the allomancer and target.
     */
    public static Vector3 CalculateAllomanticForce(Magnetic target, AllomanticIronSteel allomancer) {
        Vector3 distanceFactor;
        Vector3 positionDifference = target.CenterOfMass - allomancer.CenterOfMass;

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

        return SettingsMenu.settingsData.allomanticConstant * target.Charge * allomancer.Charge * distanceFactor;
    }

    /* 
     * Calculates the Allomantic Force and Allomantic Normal Force that would affect this target, if it were Pushed or Pulled.
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
    private void CalculateForce(Magnetic target, bool usingIronTargets) {
        Vector3 allomanticForce = CalculateAllomanticForce(target, this) * (target.LastWasPulled ? 1 : -1) /* / (usingIronTargets ? PullCount : PushCount) */;
        Vector3 direction = allomanticForce.normalized;

        thisFrameMaximumNetForce += allomanticForce;
        target.LastMaxPossibleAllomanticForce = allomanticForce;

        // Make the AF proportional to the burn rate
        allomanticForce *= (target.LastWasPulled ? IronBurnRate : SteelBurnRate);

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
        thisFrameNormalForce += restitutionForceFromTarget;
        if(target.LastWasPulled) {
            if(IronBurnRate == 0) {
                thisFrameMaximumNetForce += restitutionForceFromTarget;
            } else {
                thisFrameMaximumNetForce += restitutionForceFromTarget / IronBurnRate;
            }
        } else {
            if(SteelBurnRate == 0) {
                thisFrameMaximumNetForce += restitutionForceFromTarget;
            } else {
                thisFrameMaximumNetForce += restitutionForceFromTarget / SteelBurnRate;
            }
        }
        target.LastAllomanticForce = allomanticForce;
        target.LastAllomanticNormalForceFromAllomancer = restitutionForceFromAllomancer;
        target.LastAllomanticNormalForceFromTarget = restitutionForceFromTarget;
    }

    /*
     * Applys the force that was calculated in CalculateForce to the target and player.
     * This effectively executes the Push or Pull.
     */
    private void AddForce(Magnetic target, bool pulling) {
        target.LastWasPulled = pulling;

        target.AddForce(target.LastNetAllomanticForceOnTarget);
        rb.AddForce(target.LastNetAllomanticForceOnAllomancer, ForceMode.Force);

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

    public void StartBurning() {
        if (!IsBurningIronSteel) {
            IsBurningIronSteel = true;

            // If this component belongs to the player
            if (tag == "Player")
                GetComponent<PlayerPushPullController>().StartBurningIronSteel();
        }

    }

    public void StopBurning() {
        if (IsBurningIronSteel) {
            PullTargets.Clear();
            PushTargets.Clear();
            IronBurnRate = 0;
            SteelBurnRate = 0;
            IsBurningIronSteel = false;

            // If this component belongs to the player
            if (tag == "Player")
                GetComponent<PlayerPushPullController>().StopBurningIronSteel();
        }
    }

    //private void StartPushPullingOnTargets(bool pulling) {
    //    if ((HasPullTarget && pulling) || !HasPushTarget) {
    //        for (int i = 0; i < PullCount; i++) {
    //            PullTargets[i].StartBeingPullPushed(pulling);
    //        }
    //    }
    //    if (HasPushTarget) {
    //        for (int i = 0; i < PushCount; i++) {
    //            PushTargets[i].StartBeingPullPushed(pulling);
    //        }
    //    }
    //}

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