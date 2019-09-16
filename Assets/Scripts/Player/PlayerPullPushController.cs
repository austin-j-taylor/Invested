using System.Collections.Generic;
using UnityEngine;
using VolumetricLines;

/*
 * The AllomanticIronSteel specific for the Player.
 * Controls the blue metal lines that point from the player to nearby metals.
 * Controls the means through which the player selects Pull/PushTargets.
 */
public class PlayerPullPushController : AllomanticIronSteel {

    // Button-press time constants
    private const float timeToHoldDown = .5f;
    private const float timeDoubleTapWindow = .5f;
    // Blue Line constants
    private const float horizontalImportanceFactor = .1f;
    private const float verticalImportanceFactor = .35f;
    private const float importanceRatio = (horizontalImportanceFactor / verticalImportanceFactor) * (horizontalImportanceFactor / verticalImportanceFactor);
    private const float lineWeightThreshold = 1;
    private const float targetLateralConstant = .1f;
    private const float targetFocusFalloffConstant = 128;       // Determines how quickly blue lines blend from in-focus to out-of-focus
    private const float targetFocusLowerBound = .35f;            // Determines the luminosity of blue lines that are out of foucus
    private const float targetFocusOffScreenBound = .3f;      // Determines the luminosity of blue lines that are off-screen
    private const float targetLowTransition = .06f;
    private const float targetLowCurvePosition = .02f;

    // Control Mode constants
    private const float minAreaRadius = .025f;
    private const float maxAreaRadius = .25f;
    private const float areaRadiusIncrement = .025f;
    private const float minBubbleRadius = 1f;
    private const float maxBubbleRadius = 10f;
    private const float bubbleRadiusIncrement = 1f;
    // Other Constants
    private const float burnPercentageLerpConstant = .30f;
    private const int blueLineLayer = 10;
    private const float metalLinesLerpConstant = .30f;

    public enum ControlMode { Manual, Area, Bubble, Coinshot };

    // Button held-down times
    private float timeToStopBurning = 0;
    private float timeToSwapBurning = 0;

    public ControlMode Mode { get; private set; }
    // radius for Area and Bubble
    private float selectionAreaRadius = .025f;
    private float selectionBubbleRadius = 2f;

    // Lerp goals for burn percentage targets
    // These are displayed in the Burn Rate Meter
    private float ironBurnPercentageLerp = 0;
    private float steelBurnPercentageLerp = 0;
    // for Magnitude control style
    private float forceMagnitudeTarget = 600;

    // Currently hovered-over Magnetic
    private Magnetic highlightedTarget = null;
    public Magnetic HighlightedTarget {
        get {
            return highlightedTarget;
        }
        private set {
            if(highlightedTarget == null) {
                if(value != null) {
                    value.IsHighlighted = true;
                }
            } else {
                if(value == null || value != highlightedTarget) {
                    highlightedTarget.IsHighlighted = false;
                } else {
                    value.IsHighlighted = true;
                }
            }
            highlightedTarget = value;
        }
    }
    public bool HasHighlightedTarget {
        get {
            return HighlightedTarget != null;
        }
    }

    public override void Clear() {
        DisableRenderingBlueLines();
        base.Clear();
    }

    public void SoftClear() {
        HighlightedTarget = null;
        IronPulling = false;
        SteelPushing = false;
        VacuouslyPullTargeting = false;
        VacuouslyPushTargeting = false;
        RemoveAllTargets();
    }

    protected override void Awake() {
        base.Awake();

        Mode = ControlMode.Manual;
    }

    /*
     * Read inputs for selecting targets.
     * Update burn percentages.
     * Update blue lines pointing from player to metal.
     */
    private void Update() {
        if (!PauseMenu.IsPaused) {
            
            if (IsBurning) {

                // Change Burn Percentage Targets
                // Check scrollwheel for changing the max number of targets and burn percentage, or DPad if using gamepad
                float scrollValue = 0;
                if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) { // Gamepad
                    scrollValue = Keybinds.DPadYAxis();
                    if (SettingsMenu.settingsData.pushControlStyle == 1) {
                        ChangeTargetForceMagnitude(Keybinds.DPadXAxis());
                    }
                } else { // Mouse and keyboard
                    if (Keybinds.Negate()) {
                        scrollValue = Keybinds.ScrollWheelAxis();
                    } else {
                        if (SettingsMenu.settingsData.pushControlStyle == 0) {
                            ChangeBurnPercentageTarget(Keybinds.ScrollWheelAxis());
                        } else {
                            ChangeTargetForceMagnitude(Keybinds.ScrollWheelAxis());
                        }
                    }
                }
                // Change number of targets
                if (scrollValue > 0) {
                    IncrementTargets();
                }
                if (scrollValue < 0) {
                    DecrementTargets();
                }

                // Assign Burn percentage targets based on the previously changed burn percentage/target magnitudes
                if (SettingsMenu.settingsData.pushControlStyle == 0) { // Percentage
                    if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) { // Gamepad
                        SetPullPercentageTarget(Keybinds.RightBurnPercentage());
                        SetPushPercentageTarget(Keybinds.LeftBurnPercentage());
                    }
                } else { // Magnitude
                    if (HasPullTarget || HasPushTarget) {

                        //Debug.Log(player.LastMaximumNetForce);

                        float maxNetForce = (LastMaximumNetForce).magnitude;
                        SetPullPercentageTarget(forceMagnitudeTarget / maxNetForce);
                        SetPushPercentageTarget(forceMagnitudeTarget / maxNetForce);
                    } else {
                        SetPullPercentageTarget(0);
                        SetPushPercentageTarget(0);
                    }
                }

                LerpToBurnPercentages();
                UpdateBurnRateMeter();

                if (Player.CanControl) {
                    // Could have stopped burning above. Check if the Allomancer is still burning.
                    if (IsBurning) {
                        // Swap pull- and push- targets
                        if (Keybinds.NegateDown() && timeToSwapBurning > Time.time) {
                            // Double-tapped, Swap targets
                            PullTargets.SwapContents(PushTargets);
                            // If vacuously targeting, swap statuses of vacuous targets
                            SwapVacuousTargets();
                        } else {
                            if (Keybinds.NegateDown()) {
                                timeToSwapBurning = Time.time + timeDoubleTapWindow;
                            }
                        }

                        // Changing status of Pushing and Pulling
                        // Coinshot mode: 
                        // If do not have pull-targets, pulling is also Pushing
                        // Player.cs handles coin throwing
                        bool pulling, pushing;
                        bool keybindPulling = Keybinds.IronPulling();
                        bool keybindPushing = Keybinds.SteelPushing();
                        if (Mode == ControlMode.Coinshot) {
                            pulling = keybindPulling && HasIron && HasPullTarget;
                            pushing = keybindPushing && HasSteel;
                            // if LMB while has a push target, Push and don't Pull.
                            if(keybindPulling && !HasPullTarget) {
                                pulling = false;
                                keybindPulling = false;
                                pushing = true;
                                keybindPushing = true;
                            }
                        } else {
                            pulling = keybindPulling && HasIron;
                            pushing = keybindPushing && HasSteel;
                        }

                        // Cannot Push and Pull on the same targets
                        if (!HasPushTarget && HasPullTarget) {
                            if (pulling)
                                pushing = false;
                        } else
                        if (!HasPullTarget && HasPushTarget) {
                            if (pushing)
                                pulling = false;
                        }
                        IronPulling = pulling;
                        SteelPushing = pushing;

                        // Check input for target selection
                        bool addingTargets = (Keybinds.Select() || Keybinds.SelectAlternate()) && !Keybinds.Negate();

                        // Search for Metals
                        if (Mode == ControlMode.Manual || Mode == ControlMode.Coinshot) {
                            Magnetic target = SearchForMetalsManual();
                            TryToAddTarget(target, addingTargets, !HasPullTarget, !HasPushTarget, keybindPulling, keybindPushing);
                            // highlight the potential target you would select, if you targeted it
                            HighlightedTarget = target;
                        } else {
                            Magnetic[] targets = SearchForMetalsAreaOrBubble();

                            if(Keybinds.Negate()) {
                                // Removing targets
                                // For area/bubble targeting, Removing a target means to remove all targets.
                                if(Keybinds.Select()) {
                                    PullTargets.Clear();
                                    VacuouslyPullTargeting = false;
                                }
                                if (Keybinds.SelectAlternate()) {
                                    PushTargets.Clear();
                                    VacuouslyPushTargeting = false;
                                }
                                // Consider preserving vacuous targets (consider coins, consider a vacuous cone/bubble)
                            } else {
                                // Adding targets
                                // When targets change, remove all old targets and make space for the new ones
                                if (Keybinds.Select() || (Keybinds.PullDown() && !HasPullTarget)) {// || VacuouslyPullTargeting) {
                                    PullTargets.Size = targets.Length; // takes care of removing all targets if none are in sight
                                }
                                if (Keybinds.SelectAlternate() || (Keybinds.PushDown() && !HasPushTarget)) {// || VacuouslyPushTargeting) {
                                    PushTargets.Size = targets.Length;
                                }
                                bool nowVacuouslyPulling = !HasPullTarget;
                                bool nowVacuouslyPushing = !HasPushTarget;
                                if (targets.Length == 0) {
                                    // No metals are in the scope of the area/bubble, but you are trying to select.
                                    // Do nothing?
                                    // Deselect everything?
                                    // This at least handles vacuous targets
                                    TryToAddTarget(null, addingTargets, nowVacuouslyPulling, nowVacuouslyPushing, keybindPulling, keybindPushing);
                                } else {
                                    foreach (Magnetic target in targets) {
                                        TryToAddTarget(target, addingTargets, nowVacuouslyPulling, nowVacuouslyPushing, keybindPulling, keybindPushing);
                                    }
                                }
                            }
                        }
                        SetTargetedLineProperties();

                        RefreshHUD();
                    }
                } else { // If the player is not in control, but still burning metals, show blue lines to metals.
                    if (IsBurning) {

                        SearchForMetalsManual();
                        LerpToBurnPercentages();
                        UpdateBurnRateMeter();
                        RefreshHUD();
                    }
                }
            }

            // Start and Stop Burning metals
            if (Player.CanControl) {
                if (IsBurning) {
                    // Stop burning
                    if (Keybinds.StopBurning()) {
                        StopBurning();
                        timeToStopBurning = 0;
                    } else
                    if (Keybinds.Negate()) {
                        timeToStopBurning += Time.deltaTime;
                        if (Keybinds.Select() && Keybinds.SelectAlternate() && timeToStopBurning > timeToHoldDown) {
                            //if (Keybinds.IronPulling() && Keybinds.SteelPushing() && timeToStopBurning > timeToHoldDown) {
                            StopBurning();
                            timeToStopBurning = 0;
                        }
                    } else {
                        timeToStopBurning = 0;
                    }
                } else {
                    // Start burning (as long as the Control Wheel isn't open to interfere)
                    if (!Keybinds.Negate() && !HUD.ControlWheelController.IsOpen) {
                        if (Keybinds.SelectDown() || Keybinds.PullDown()) {
                            StartBurning(true);
                            SearchForMetalsManual();
                        }
                        else if (Keybinds.SelectAlternateDown() || Keybinds.PushDown()) {
                            StartBurning(false);
                            SearchForMetalsManual();
                        }
                    }
                }
            }
        }
    }

    protected override bool StartBurning(bool startIron) {
        if (!base.StartBurning(startIron))
            return false;
        GamepadController.Shake(.1f, .1f, .3f);
        if (SettingsMenu.settingsData.renderblueLines == 1)
            EnableRenderingBlueLines();
        ironBurnPercentageLerp = 1;
        steelBurnPercentageLerp = 1;
        forceMagnitudeTarget = 600;

        return true;
    }

    public override void StopBurning() {
        base.StopBurning();
        HighlightedTarget = null;
        steelBurnPercentageLerp = 0;
        ironBurnPercentageLerp = 0;
        forceMagnitudeTarget = 0;
        GamepadController.SetRumble(0, 0);
        GetComponentInChildren<AllomechanicalGlower>().RemoveAllEmissions();
        DisableRenderingBlueLines();
        RefreshHUD();
    }

    // Called on Magnetics that should be added or removed as Push/Pull-targets
    // This whoooole thing is bad coding practice
    private void TryToAddTarget(Magnetic target, bool addingTargets, bool nowVacuouslyPulling, bool nowVacuouslyPushing, bool keybindPulling, bool keybindPushing) {
        // Add/Remove Targets
        // Pulling
        if (Keybinds.Select()) {
            VacuouslyPullTargeting = false;
            // Modifying target arrays
            if (addingTargets) {
                AddPullTarget(target);
            } else {
                if (RemovePullTarget(target)) {// If the player is hovering over a pullTarget, instantly remove that one

                } else if (Keybinds.SelectDown() && !RemovePullTarget(target)) { // If the highlighted Magnetic is not a pullTarget, remove the oldest pullTarget instead
                    RemovePullTargetAt(0);
                }
            }
        } else {
            if (!nowVacuouslyPulling && VacuouslyPullTargeting) {
                if (!keybindPulling) { // should no longer be vacuously pulling
                    VacuouslyPullTargeting = false;
                } else { // continue to vacuously Pull

                }
            } else {
                if (nowVacuouslyPulling && Keybinds.PullDown() && Mode != ControlMode.Coinshot) { // should now be vacuously pulling on this target (ignored in Coinshot mode)
                    VacuouslyPullTargeting = true;
                    AddPullTarget(target, false);
                }
            }
        }
        // Pushing
        if (Keybinds.SelectAlternate()) {
            VacuouslyPushTargeting = false;
            // Modifying target arrays
            if (addingTargets) {
                AddPushTarget(target);
            } else {
                if (RemovePushTarget(target)) {// If the player is hovering over a pushTarget, instantly remove that one. Keep it highlighted.
                    if (Mode == ControlMode.Manual || Mode == ControlMode.Coinshot)
                        HighlightedTarget = target;
                } else if (Keybinds.SelectAlternateDown() && !RemovePushTarget(target)) { // If the highlighted Magnetic is not a pushTarget, remove the oldest pushTarget instead
                    RemovePushTargetAt(0);
                }
            }
        } else {
            if (!nowVacuouslyPushing && VacuouslyPushTargeting) {
                if (!keybindPushing) { // should no longer be vacuously pushing
                    VacuouslyPushTargeting = false;
                }
            } else {
                if (nowVacuouslyPushing && Keybinds.PushDown()) { // should now be vacuously pushing
                    VacuouslyPushTargeting = true;
                    AddPushTarget(target, false);
                }
            }
        }
    }

    /*
     * STEEL/IRONSIGHT MANAGEMENT: blue lines pointing to nearby metals
     * 
     * Searches all Magnetics in the scene for those that are within detection range of the player.
     * Shows metal lines drawing from them to the player.
     * Returns the Magnetics that should be selected. Depending on the Control Mode, this will be:
     *  - Manual/Coinshot: the "closest" metal to the center of the screen
     *  - Area: All metals in the circle close to the center of the screen
     *  - Bubble: All metals in a sphere around the player
     * 
     * Rules for the metal lines:
     *  - The WIDTH of the line is dependant on the MASS of the target
     *  - The BRIGHTNESS of the line is dependent on the FORCE that would result from the Push
     *  - The LIGHT SABER FACTOR is dependent on the FORCE acting on the target. If the metal is not a target, it is 1.
     */
    private Magnetic SearchForMetalsManual() {
        float greatestWeight = 0;
        Magnetic centerObject = null;
        bool mustCalculateCenter = true;

        // If the player is directly looking at a magnetic's collider
        if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 500, GameManager.Layer_IgnorePlayer)) {
            Magnetic target = hit.collider.GetComponentInParent<Magnetic>();
            if (target && target.IsInRange(this, GreaterPassiveBurn)) {
                centerObject = target;
                mustCalculateCenter = false;
            }
        }
        // Go through every metal in the scene and update the blue lines pointing to them.
        // If the player is not directly looking at a magnetic, select the one closest to the center of the screen
        foreach (Magnetic target in GameManager.MagneticsInScene) {
            if (target.isActiveAndEnabled && target != Player.PlayerMagnetic) {
                if (mustCalculateCenter) { // If player is not directly looking at magnetic, calculate which is closest
                    float weight = SetLineProperties(target);

                    // If the Magnetic could be targeted
                    if (weight > lineWeightThreshold) {
                        // IF the new Magnetic is closer to the center of the screen than the previous most-center Magnetic
                        // and IF the new Magnetic is in range
                        if (weight > greatestWeight) {
                            greatestWeight = weight;
                            centerObject = target;
                        }
                    }
                } else { // If player is directly looking at magnetic, just set line properties
                    SetLineProperties(target);
                }
            }
        }
        if (centerObject) {
            // Brighten the blue line to the object that would be targeted.
            centerObject.BrightenLine();
        }

        return centerObject;
    }
    private Magnetic[] SearchForMetalsAreaOrBubble() {
        float distance;
        List<Magnetic> targetsToSelect = new List<Magnetic>();
        // Go through every metal in the scene and update the blue lines pointing to them.
        // Add every metal near the center of the screen to targetsToSelect.
        foreach (Magnetic target in GameManager.MagneticsInScene) {
            if (target.isActiveAndEnabled && target != Player.PlayerMagnetic) {
                float weight;
                if (Mode == ControlMode.Area)
                    weight = SetLinePropertiesGetRadial(target, out distance);
                else
                    weight = SetLinePropertiesGetLinear(target, out distance);

                // If the Magnetic could be targeted and it is within the radius
                if (weight > 0 && distance < (Mode == ControlMode.Area ? selectionAreaRadius : selectionBubbleRadius)) {
                    targetsToSelect.Add(target);
                }
            }
        }
        return targetsToSelect.ToArray();
    }

    private void SetTargetedLineProperties() {
        // Regardless of other factors, lines pointing to Push/Pull-targets have unique colors
        // Update metal lines for Pull/PushTargets
        if (PullingOnPullTargets) {
            PullTargets.UpdateBlueLines(iron, IronBurnPercentageTarget, CenterOfMass);
        } else if (PushingOnPullTargets) {
            PullTargets.UpdateBlueLines(iron, SteelBurnPercentageTarget, CenterOfMass);
        } else if (HasPullTarget) {
            PullTargets.UpdateBlueLines(iron, 0, CenterOfMass);
        }
        if (PullingOnPushTargets) {
            PushTargets.UpdateBlueLines(steel, IronBurnPercentageTarget, CenterOfMass);
        } else if (PushingOnPushTargets) {
            PushTargets.UpdateBlueLines(steel, SteelBurnPercentageTarget, CenterOfMass);
        } else if (HasPushTarget) {
            PushTargets.UpdateBlueLines(steel, 0, CenterOfMass);
        }
    }
    
    /*
     * Checks several factors and sets the properties of the blue line pointing to target.
     * These factors are described in the above function.
     * Returns the "weight" of the target, which increases within closeness to the player and the center of the screen.
     */
    private float SetLineProperties(Magnetic target, out float radialDistance, out float linearDistance) {
        Vector3 allomanticForceVector = CalculateAllomanticForce(target);
        float allomanticForce = allomanticForceVector.magnitude;
        // If using Percentage force mode, burn percentage affects your range for burning
        if (SettingsMenu.settingsData.pushControlStyle == 0)
            allomanticForce *= GreaterPassiveBurn;

        allomanticForce -= SettingsMenu.settingsData.metalDetectionThreshold; // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold

        if (allomanticForce <= 0) {
            // Magnetic is out of range
            target.DisableBlueLine();
            radialDistance = 1;
            linearDistance = float.PositiveInfinity;
            return -1;
        }

        // Set line properties
        Vector3 screenPosition = CameraController.ActiveCamera.WorldToViewportPoint(target.transform.position);

        // Calculate the distance from the center for deciding which blue lines are "in-focus"
        radialDistance = Mathf.Sqrt(
            (screenPosition.x - .5f) * (screenPosition.x - .5f) +
            (screenPosition.y - .5f) * (screenPosition.y - .5f) * importanceRatio
        );
        linearDistance = (transform.position - target.transform.position).magnitude;

        float weight;
        if (screenPosition.z < 0) { // the target is behind the player, off-screen
            weight = -1;
        } else {
            // Assign weighting due to position
            weight = .1f / radialDistance - linearDistance / 500;
        }

        if (SettingsMenu.settingsData.renderblueLines == 1) {
            float closeness = blueLineBrightnessFactor * Mathf.Pow(allomanticForce, blueLineChangeFactor);

            // Make lines in-focus if near the center of the screen
            // If nearly off-screen, instead make lines dimmer
            if (screenPosition.z < 0) { // behind player
                closeness *= targetFocusOffScreenBound * targetFocusOffScreenBound;
            } else {
                if (weight < lineWeightThreshold) {
                    closeness *= targetFocusLowerBound;
                }
            }
            target.SetBlueLine(
                CenterOfMass,
                target.Charge * (SettingsMenu.settingsData.cameraFirstPerson == 0 ? blueLineThirdPersonWidth : blueLineFirstPersonWidth),
                1,
                closeness
            );
        }
        return weight;
    }
    private float SetLinePropertiesGetRadial(Magnetic target, out float radialDistance) {
        return SetLineProperties(target, out radialDistance, out _);
    }
    private float SetLinePropertiesGetLinear(Magnetic target, out float linearDistance) {
        return SetLineProperties(target, out _, out linearDistance);
    }
    private float SetLineProperties(Magnetic target) {
        return SetLineProperties(target, out _, out _);
    }
    public void UpdateBlueLines() {
        SearchForMetalsManual();
        SetTargetedLineProperties();
    }

    public void RemoveAllTargets() {
        PullTargets.Clear();
        PushTargets.Clear();

        HUD.TargetOverlayController.Clear();
    }

    private void IncrementTargets() {
        switch (Mode) {
            case ControlMode.Manual: // fall through
            case ControlMode.Coinshot:
                if (sizeOfTargetArrays < maxNumberOfTargets) {
                    sizeOfTargetArrays++;
                    PullTargets.Size = sizeOfTargetArrays;
                    PushTargets.Size = sizeOfTargetArrays;
                }
                break;
            case ControlMode.Area:
                if (selectionAreaRadius < maxAreaRadius) {
                    selectionAreaRadius += areaRadiusIncrement;
                }
                break;
            case ControlMode.Bubble:
                if (selectionBubbleRadius < maxBubbleRadius) {
                    selectionBubbleRadius += bubbleRadiusIncrement;
                }
                break;
        }
    }

    private void DecrementTargets() {
        switch (Mode) {
            case ControlMode.Manual: // fall through
            case ControlMode.Coinshot:
                if (sizeOfTargetArrays > minNumberOfTargets) {
                    sizeOfTargetArrays--;
                    PullTargets.Size = sizeOfTargetArrays;
                    PushTargets.Size = sizeOfTargetArrays;
                }
                break;
            case ControlMode.Area:
                if (selectionAreaRadius > minAreaRadius) {
                    selectionAreaRadius -= areaRadiusIncrement;
                }
                break;
            case ControlMode.Bubble:
                if (selectionBubbleRadius > minBubbleRadius) {
                    selectionBubbleRadius -= bubbleRadiusIncrement;
                }
                break;
        }
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
            StopBurning();
    }

    // Increments or decrements the current burn percentage
    private void ChangeBurnPercentageTarget(float change) {
        if (change > 0) {
            change = .10f;
            if (ironBurnPercentageLerp < .09f || steelBurnPercentageLerp < .09f) {
                change /= 10f;
            }
        } else if (change < 0) {
            change = -.10f;
            if (ironBurnPercentageLerp <= .10f || steelBurnPercentageLerp <= .10f) {
                change /= 10f;
            }
        }
        SetPullPercentageTarget(Mathf.Clamp(ironBurnPercentageLerp + change, 0, 1));
        SetPushPercentageTarget(Mathf.Clamp(steelBurnPercentageLerp + change, 0, 1));
    }

    // Sets the lerp goal for iron burn percentage
    private void SetPullPercentageTarget(float percentage) {
        if (percentage > .005f) {
            ironBurnPercentageLerp = Mathf.Min(1, percentage);
        } else {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                ironBurnPercentageLerp = 1;
            else
                ironBurnPercentageLerp = 0;
        }
    }
    // Sets the lerp goal for steel burn percentage
    private void SetPushPercentageTarget(float percentage) {
        if (percentage > .005f) {
            steelBurnPercentageLerp = Mathf.Min(1, percentage);
        } else {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                steelBurnPercentageLerp = 1;
            else
                steelBurnPercentageLerp = 0;
        }
    }

    /*
     * Lerps from the current burn percentage to the burn percentage target for both metals.
     * If the player is not Pulling or Pushing, that burn percentage is instead set to 0.
     *      Set, not lerped - there's more precision there.
     */
    private void LerpToBurnPercentages() {
        IronBurnPercentageTarget = Mathf.Lerp(IronBurnPercentageTarget, ironBurnPercentageLerp, burnPercentageLerpConstant);
        SteelBurnPercentageTarget = Mathf.Lerp(SteelBurnPercentageTarget, steelBurnPercentageLerp, burnPercentageLerpConstant);
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            IronPassiveBurn = 1;
            SteelPassiveBurn = 1;
        } else {
            IronPassiveBurn = IronBurnPercentageTarget;
            SteelPassiveBurn = SteelBurnPercentageTarget;
        }

        // Gamepad rumble
        if (HasPullTarget || HasPushTarget) {
            if (IronPulling) {
                GamepadController.SetRumbleRight(IronBurnPercentageTarget * GamepadController.rumbleFactor);
            } else {
                GamepadController.SetRumbleRight(0);
            }
            if (SteelPushing) {
                GamepadController.SetRumbleLeft(SteelBurnPercentageTarget * GamepadController.rumbleFactor);
            } else {
                GamepadController.SetRumbleLeft(0);
            }
        } else {
            GamepadController.SetRumble(0, 0);
        }
        // If using the Percentage control scheme and the target burn percentage is 0 (and not using a gamepad, which will very often be 0)
        //      Then stop burning metals
        if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad && (IronBurnPercentageTarget < .001f && SteelBurnPercentageTarget < .001f)) {
            StopBurning();
        }
    }

    private void UpdateBurnRateMeter() {
        if (IsBurning) {
            if (SettingsMenu.settingsData.pushControlStyle == 1) // Magnitude
                HUD.BurnPercentageMeter.SetBurnRateMeterForceMagnitude(LastAllomanticForce, LastAnchoredPushBoost, IronBurnPercentageTarget, SteelBurnPercentageTarget, forceMagnitudeTarget);
            else if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
                if (SteelPushing) {
                    if (IronPulling) {
                        HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                            IronBurnPercentageTarget, SteelBurnPercentageTarget);
                    } else {
                        HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                            0, SteelBurnPercentageTarget);
                    }
                } else {
                    if (IronPulling) {
                        HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                            IronBurnPercentageTarget, 0);
                    } else {
                        HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                            IronBurnPercentageTarget, SteelBurnPercentageTarget);
                    }
                }
            } else {
                HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                    IronBurnPercentageTarget, SteelBurnPercentageTarget);
            }
        } else {
            HUD.BurnPercentageMeter.Clear();
        }
    }

    // Refreshes all elements of the hud relevent to pushing and pulling
    private void RefreshHUD() {
        if (IsBurning) {
            RefreshHUDColorsOnly();
            HUD.TargetOverlayController.HardRefresh();
            // number of targets
            switch (Mode) {
                case ControlMode.Manual: // fall through
                case ControlMode.Coinshot:
                    HUD.BurnPercentageMeter.SetMetalLineCountTextManual(sizeOfTargetArrays);
                    break;
                case ControlMode.Area:
                    HUD.BurnPercentageMeter.SetMetalLineCountTextArea(selectionAreaRadius);
                    break;
                case ControlMode.Bubble:
                    HUD.BurnPercentageMeter.SetMetalLineCountTextBubble(selectionBubbleRadius);
                    break;
            }
        } else {
            HUD.BurnPercentageMeter.Clear();
        }
    }

    // Refreshes the colors of the text of target labels and the burn rate meter.
    private void RefreshHUDColorsOnly() {

        if (PullingOnPullTargets || PushingOnPullTargets) {
            HUD.TargetOverlayController.SetPullTextColorStrong();
            HUD.BurnPercentageMeter.SetForceTextColorStrong();
        } else {
            HUD.TargetOverlayController.SetPullTextColorWeak();
        }
        if (PullingOnPushTargets || PushingOnPushTargets) {
            HUD.TargetOverlayController.SetPushTextColorStrong();
            HUD.BurnPercentageMeter.SetForceTextColorStrong();
        } else {
            HUD.TargetOverlayController.SetPushTextColorWeak();
        }

        if (IronPulling || SteelPushing) {
            HUD.BurnPercentageMeter.SetForceTextColorStrong();
        } else {
            HUD.BurnPercentageMeter.SetForceTextColorWeak();
        }
    }

    public void EnableRenderingBlueLines() {
        if (IsBurning)
            CameraController.ActiveCamera.cullingMask = ~0;
    }

    public void DisableRenderingBlueLines() {
        CameraController.ActiveCamera.cullingMask = ~(1 << blueLineLayer);
    }

    public void SetControlModeManual() {
        Mode = ControlMode.Manual;
        PullTargets.Size = sizeOfTargetArrays;
    }
    public void SetControlModeArea() {
        Mode = ControlMode.Area;
        PullTargets.Size = 0;
        HighlightedTarget = null;
    }
    public void SetControlModeBubble() {
        Mode = ControlMode.Bubble;
        PullTargets.Size = 0;
        HighlightedTarget = null;
    }
    public void SetControlModeCoinshot() {
        Mode = ControlMode.Coinshot;
        PullTargets.Size = sizeOfTargetArrays;
    }

    public void AddVacuousPushTarget(Magnetic target, bool keepAdding = false) {
        if(keepAdding && PushTargets.IsFull()) {
            PushTargets.Size++;
        }
        VacuouslyPushTargeting = true;
        AddPushTarget(target, false);
    }
}