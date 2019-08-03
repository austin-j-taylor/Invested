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
    private const float targetFocusLowerBound = .3f;            // Determines the luminosity of blue lines that are out of foucus
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
    // Number of targets for Manual/Coinshot Control Mode; ignored for Area and Bubble
    protected int sizeOfTargetArrays = 1;
    // radius for Area and Bubble
    private float selectionAreaRadius = .1f;
    private float selectionBubbleRadius = 2f;

    // Lerp goals for burn percentage targets
    // These are displayed in the Burn Rate Meter
    private float ironBurnPercentageLerp = 0;
    private float steelBurnPercentageLerp = 0;
    // for Magnitude control style
    private float forceMagnitudeTarget = 600;

    // Currently hovered-over Magnetic
    public Magnetic HighlightedTarget { get; private set; } = null;
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
        if (HasHighlightedTarget)
            HighlightedTarget.IsHighlighted = false;
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
    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            if (Player.CanControlPlayer) {
                // Start and Stop Burning metals
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
                        if (Keybinds.SelectDown() || Keybinds.PullDown())
                            StartBurning(true);
                        else if (Keybinds.SelectAlternateDown() || Keybinds.PushDown())
                            StartBurning(false);
                    }
                }
            }

            // Could have changed burning status above. Check if the Allomancer is still burning.
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

                if (Player.CanControlPlayer) {
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
                        bool pulling = Keybinds.IronPulling() && HasIron && !(Mode == ControlMode.Coinshot && Keybinds.SteelPushing() && !Player.PlayerInstance.CoinHand.Pouch.IsEmpty);
                        bool pushing = Keybinds.SteelPushing() && HasSteel;
                        // If you are trying to push and pull and only have pullTargets, only push. And vice versa
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
                        Magnetic[] targets = SearchForMetals();
                        if (Mode == ControlMode.Area || Mode == ControlMode.Bubble) {
                            if (Keybinds.Select() || VacuouslyPullTargeting) {
                                PullTargets.Size = targets.Length;
                            }
                            if (Keybinds.SelectAlternate() || VacuouslyPushTargeting) {
                                PushTargets.Size = targets.Length;
                            }
                        }
                        foreach (Magnetic target in targets) {
                            TryToAddTarget(target, addingTargets);
                        }

                        RefreshHUD();
                    }
                } else { // If the player is not in control, but still burning metals, show blue lines to metals.
                    if (IsBurning) {

                        SearchForMetals(false);
                        LerpToBurnPercentages();
                        UpdateBurnRateMeter();
                        RefreshHUD();
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
        if (HasHighlightedTarget) {
            HighlightedTarget.IsHighlighted = false;
            HighlightedTarget = null;
        }
        steelBurnPercentageLerp = 0;
        ironBurnPercentageLerp = 0;
        forceMagnitudeTarget = 0;
        GamepadController.SetRumble(0, 0);
        GetComponentInChildren<AllomechanicalGlower>().RemoveAllEmissions();
        DisableRenderingBlueLines();
        RefreshHUD();
    }

    // Called on Magnetics that should be added as Push/Pull-targets
    private void TryToAddTarget(Magnetic target, bool addingTargets) {
        if (target != null) {
            // highlight the potential target you would select, if you targeted it
            if (target != HighlightedTarget) {
                // Remove old target
                if (HasHighlightedTarget)
                    HighlightedTarget.IsHighlighted = false;
                target.IsHighlighted = true;
                HighlightedTarget = target;
            }
        } else {
            // no target near center of screen; remove highlighted target
            if (HasHighlightedTarget) {
                HighlightedTarget.IsHighlighted = false;
            }
            HighlightedTarget = null;
        }

        // Add/Remove Targets
        // Pulling
        if (Keybinds.Select()) {
            if (VacuouslyPullTargeting) { // Was vacuously Pulling, but should no longer be
                VacuouslyPullTargeting = false;
            }
            // Modifying target arrays
            if (addingTargets) {
                AddPullTarget(target);
            } else {
                if (RemovePullTarget(target)) {// If the player is hovering over a pullTarget, instantly remove that one. Keep it highlighted.
                    target.IsHighlighted = true;
                } else if (Keybinds.SelectDown() && !RemovePullTarget(target)) { // If the highlighted Magnetic is not a pullTarget, remove the oldest pullTarget instead
                    RemovePullTargetAt(0);
                }
            }
        } else {
            if (VacuouslyPullTargeting) {
                if (!Keybinds.IronPulling()) { // should no longer be vacuously pulling
                    VacuouslyPullTargeting = false;
                } else { // continue to vacuously Pull

                }
            } else {
                if (!HasPullTarget && Keybinds.PullDown()) { // should now be vacuously pulling
                    VacuouslyPullTargeting = true;
                    AddPullTarget(target);
                }
            }
        }
        // Pushing
        if (Keybinds.SelectAlternate()) {
            if (VacuouslyPushTargeting) { // Was vacuously Pushing, but should no longer be
                VacuouslyPushTargeting = false;
            }
            // Modifying target arrays
            if (addingTargets) {
                AddPushTarget(target);
            } else {
                if (RemovePushTarget(target)) {// If the player is hovering over a pushTarget, instantly remove that one. Keep it highlighted.
                    target.IsHighlighted = true;
                } else if (Keybinds.SelectAlternateDown() && !RemovePushTarget(target)) { // If the highlighted Magnetic is not a pushTarget, remove the oldest pushTarget instead
                    RemovePushTargetAt(0);
                }
            }
        } else {
            if (VacuouslyPushTargeting) {
                if (!Keybinds.SteelPushing()) { // should no longer be vacuously pushing
                    VacuouslyPushTargeting = false;
                }
            } else {
                if (!HasPushTarget && Keybinds.PushDown()) { // should now be vacuously pushing
                    VacuouslyPushTargeting = true;
                    AddPushTarget(target);
                }
            }
        }
    }

    /*
     * Searches all Magnetics in the scene for those that are within detection range of the player.
     * Shows metal lines drawing from them to the player.
     * Returns the Magnetics that should be selected. Depending on the Control Mode, this will be:
     *  - Manual/Coinshot: the "closest" metal to the center of the screen
     *  - Area: All metals in the circle close to the center of the screen
     *  - Bubble: All metals in a sphere around the player
     * 
     * If targetedLineColors is false, then push/pullTargets will not have specially colored lines (i.e. red, green, light blue)
     * 
     * Rules for the metal lines:
     *  - The WIDTH of the line is dependant on the MASS of the target
     *  - The BRIGHTNESS of the line is dependent on the DISTANCE from the player
     *  - The LIGHT SABER FACTOR is dependent on the FORCE acting on the target. If the metal is not a target, it is 1.
     */
    public Magnetic[] SearchForMetals(bool targetedLineColors = true) {
        float greatestWeight = 0, radialDistance = 0, linearDistance = 0;
        Magnetic[] centerObjects;

        if (Mode == ControlMode.Manual || Mode == ControlMode.Coinshot) {

            bool mustCalculateCenter = true;
            centerObjects = new Magnetic[1];
            // If the player is directly looking at a magnetic's collider
            if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 500, GameManager.Layer_IgnorePlayer)) {
                Magnetic target = hit.collider.GetComponentInParent<Magnetic>();
                if (target && target.IsInRange(this, GreaterPassiveBurn)) {
                    centerObjects[0] = target;
                    mustCalculateCenter = false;
                }
            }
            // Go through every metal in the scene and update the blue lines pointing to them.
            // If the player is not directly looking at a magnetic, select the one closest to the center of the screen
            for (int i = 0; i < GameManager.MagneticsInScene.Count; i++) {
                Magnetic target = GameManager.MagneticsInScene[i];
                if (target.isActiveAndEnabled && target != Player.PlayerMagnetic) {
                    if (mustCalculateCenter) { // If player is not directly looking at magnetic, calculate which is closest
                        float weight = SetLineProperties(target, out radialDistance, out linearDistance);

                        // If looking for the object at the center of the screen
                        // If the Magnetic could be targeted
                        if (targetedLineColors && weight > lineWeightThreshold) {
                            // IF the new Magnetic is closer to the center of the screen than the previous most-center Magnetic
                            // and IF the new Magnetic is in range
                            if (weight > greatestWeight) {
                                greatestWeight = weight;
                                centerObjects[0] = target;
                            }
                        }
                    } else { // If player is directly looking at magnetic, just set line properties
                        SetLineProperties(target, out radialDistance, out linearDistance);
                    }
                }
            }
            if (centerObjects[0]) {
                // Brighten the blue line to the object that would be targeted.
                centerObjects[0].BrightenLine();
            }
        } else if (Mode == ControlMode.Area) {
            List<Magnetic> targetsToSelect = new List<Magnetic>();
            // Go through every metal in the scene and update the blue lines pointing to them.
            // Add every metal near the center of the screen to targetsToSelect.
            for (int i = 0; i < GameManager.MagneticsInScene.Count; i++) {
                Magnetic target = GameManager.MagneticsInScene[i];
                if (target.isActiveAndEnabled && target != Player.PlayerMagnetic) {
                    float weight = SetLineProperties(target, out radialDistance, out linearDistance);

                    // If the Magnetic could be targeted and it is within the radius
                    if (weight > 0 && radialDistance < selectionAreaRadius) {
                        targetsToSelect.Add(target);
                    }
                }
            }
            centerObjects = targetsToSelect.ToArray();

        } else { // Bubble
            List<Magnetic> targetsToSelect = new List<Magnetic>();
            // Go through every metal in the scene and update the blue lines pointing to them.
            // Add every metal near the player to targetsToSelect.
            for (int i = 0; i < GameManager.MagneticsInScene.Count; i++) {
                Magnetic target = GameManager.MagneticsInScene[i];
                if (target.isActiveAndEnabled && target != Player.PlayerMagnetic) {
                    float weight = SetLineProperties(target, out radialDistance, out linearDistance);

                    // If the Magnetic could be targeted and it is within the radius
                    if (weight > 0 && linearDistance < selectionBubbleRadius) {
                        targetsToSelect.Add(target);
                    }
                }
            }

            centerObjects = targetsToSelect.ToArray();
        }

        // Regardless of other factors, lines pointing to Push/Pull-targets have unique colors
        if (targetedLineColors) {
            // Update metal lines for Pull/PushTargets
            if (PullingOnPullTargets) {
                PullTargets.UpdateBlueLines(true, IronBurnPercentageTarget, CenterOfMass);
            } else if (PushingOnPullTargets) {
                PullTargets.UpdateBlueLines(true, SteelBurnPercentageTarget, CenterOfMass);
            } else if (HasPullTarget) {
                PullTargets.UpdateBlueLines(true, 0, CenterOfMass);
            }
            if (PullingOnPushTargets) {
                PushTargets.UpdateBlueLines(false, IronBurnPercentageTarget, CenterOfMass);
            } else if (PushingOnPushTargets) {
                PushTargets.UpdateBlueLines(false, SteelBurnPercentageTarget, CenterOfMass);
            } else if (HasPushTarget) {
                PushTargets.UpdateBlueLines(false, 0, CenterOfMass);
            }
        }
        return centerObjects;
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
    }
    public void SetControlModeBubble() {
        Mode = ControlMode.Bubble;
        PullTargets.Size = 0;
    }
    public void SetControlModeCoinshot() {
        Mode = ControlMode.Coinshot;
        PullTargets.Size = sizeOfTargetArrays;
    }
}