using UnityEngine;

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
    private const float targetFocusFalloffConstant = 128;       // Determines how quickly blue lines blend from in-focus to out-of-focus
    private const float targetFocusLowerBound = .2f;            // Determines the luminosity of blue lines that are out of foucus
    private const float targetFocusOffScreenBound = .15f;      // Determines the luminosity of blue lines that are off-screen
    private const float targetLowTransition = .06f;
    private const float targetLowCurvePosition = .02f;
    private const float lowLineColor = .1f;
    private const float highLineColor = .85f;
    private const float blueLineWidthBaseFactor = .04f;
    private const float blueLineStartupFactor = 2f;
    private const float blueLineBrightnessFactor = 1;
    // Other Constants
    private const float burnRateLerpConstant = .30f;
    private const int blueLineLayer = 10;

    // Button held-down times
    private float timeToStopBurning = 0f;
    private float timeToSwapBurning = 0f;

    // Lerp goals for burn rate targets
    // These are displayed in the Burn Rate Meter
    private float ironBurnRateLerp = 0;
    private float steelBurnRateLerp = 0;
    // for Magnitude control style
    private float forceMagnitudeTarget = 600;

    // Currently hovered-over Magnetic
    public Magnetic HighlightedTarget { get; private set; } = null;
    public bool HasHighlightedTarget {
        get {
            return HighlightedTarget != null;
        }
    }

    private void Update() {
        if (!PauseMenu.IsPaused) {
            if (Player.CanControlPlayer) {
                // Start burning
                if (!Keybinds.Negate()) {
                    if (Keybinds.SelectDown() && HasIron)
                        StartBurning(true);
                    else if (Keybinds.SelectAlternateDown() && HasSteel)
                        StartBurning(false);
                }

                if (IsBurningIronSteel) {
                    // Swap pull- and push- targets
                    if (Keybinds.NegateDown() && timeToSwapBurning > Time.time) {
                        // Double-tapped, Swap targets
                        PullTargets.SwapContents(PushTargets);
                    } else {
                        if (Keybinds.NegateDown()) {
                            timeToSwapBurning = Time.time + timeDoubleTapWindow;
                        }
                    }

                    // Check scrollwheel for changing the max number of targets and burn rate, or DPad if using gamepad
                    float scrollValue = 0;
                    if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) { // Gamepad
                        scrollValue = Keybinds.DPadYAxis();
                        if (SettingsMenu.settingsData.pushControlStyle == 1) {
                            ChangeTargetForceMagnitude(Keybinds.DPadXAxis());
                        }
                    } else { // Mouse and keyboard
                        if (Keybinds.ScrollWheelButton()) {
                            scrollValue = Keybinds.ScrollWheelAxis();
                        } else {
                            if (SettingsMenu.settingsData.pushControlStyle == 0) {
                                ChangeBurnRateTarget(Keybinds.ScrollWheelAxis());
                            } else {
                                ChangeTargetForceMagnitude(Keybinds.ScrollWheelAxis());
                            }
                        }
                    }
                    if (scrollValue > 0) {
                        IncrementNumberOfTargets();
                    }
                    if (scrollValue < 0) {
                        DecrementNumberOfTargets();
                    }

                    // Assign Burn rate targets based on the previously changed burn rate/target magnitudes
                    if (SettingsMenu.settingsData.pushControlStyle == 0) { // Percentage
                        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) { // Gamepad
                            SetPullRateTarget(Keybinds.RightBurnRate());
                            SetPushRateTarget(Keybinds.LeftBurnRate());
                        }
                    } else { // Magnitude
                        if (HasPullTarget || HasPushTarget) {

                            //Debug.Log(player.LastMaximumNetForce);

                            float maxNetForce = (LastMaximumNetForce).magnitude;
                            SetPullRateTarget(forceMagnitudeTarget / maxNetForce);
                            SetPushRateTarget(forceMagnitudeTarget / maxNetForce);
                        } else {
                            SetPullRateTarget(0);
                            SetPushRateTarget(0);
                        }
                    }

                    // Stop burning altogether, hide metal lines
                    if (Keybinds.Negate()) {
                        timeToStopBurning += Time.deltaTime;
                        if (Keybinds.Select() && Keybinds.SelectAlternate() && timeToStopBurning > timeToHoldDown) {
                            StopBurning();
                            timeToStopBurning = 0;
                        }
                    } else {
                        timeToStopBurning = 0;
                    }

                    LerpToBurnRates();
                    UpdateBurnRateMeter();


                    // Could have stopped burning above. Check if the Allomancer is still burning.
                    if (IsBurningIronSteel) {
                        bool pulling = Keybinds.IronPulling() && HasIron;
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
                        bool selecting = (Keybinds.Select() || Keybinds.SelectAlternate()) && !Keybinds.Negate();
                        Magnetic target = SearchForMetals();

                        if (target != null) {
                            // highlight the potential target you would select, if you targeted it
                            if (target != HighlightedTarget) {
                                if (HasHighlightedTarget)
                                    HighlightedTarget.RemoveTargetGlow();
                                target.AddTargetGlow();
                                HighlightedTarget = target;
                            }
                        } else {
                            // no target near center of screen; remove highlighted target
                            if (HasHighlightedTarget)
                                HighlightedTarget.RemoveTargetGlow();
                            HighlightedTarget = null;
                        }

                        if (Keybinds.Select() || Keybinds.SelectAlternate()) {
                            // Select or Deselect pullTarget and/or pushTarget
                            if (Keybinds.Select() && HasIron) { // Selecting pull target
                                if (selecting) {
                                    AddPullTarget(target);
                                } else {
                                    if (RemovePullTarget(target)) {// If the player is hovering over a pullTarget, instantly remove that one. Keep it highlighted.
                                        target.AddTargetGlow();
                                    } else if (Keybinds.SelectDown() && !RemovePullTarget(target)) { // If the highlighted Magnetic is not a pullTarget, remove the oldest pullTarget instead
                                        RemovePullTargetAt(0);
                                    }
                                }
                            }
                            if (Keybinds.SelectAlternate() && HasSteel) {
                                if (selecting) {
                                    AddPushTarget(target);
                                } else {
                                    if (RemovePushTarget(target)) {
                                        target.AddTargetGlow();
                                    } else if (Keybinds.SelectAlternateDown() && !RemovePushTarget(target)) {
                                        RemovePushTargetAt(0);
                                    }
                                }
                            }
                        }
                        RefreshHUD();
                    }
                }
            } else { // If the player is not in control, but still burning metals, show blue lines to metals.
                if (IsBurningIronSteel) {
                    OnlyUpdateBlueLines();
                }
            }

        }
    }

    public override void Clear(bool clearTargets = true) {
        IronReserve.SetMass(150);
        SteelReserve.SetMass(150);
        base.Clear(clearTargets);
    }

    protected override void StartBurning(bool startIron) {
        base.StartBurning(startIron);
        GamepadController.Shake(.1f, .1f, .3f);
        UpdateBurnRateMeter();
        HUD.BurnRateMeter.SetMetalLineCountText(PullTargets.Size.ToString());
        if (SettingsMenu.settingsData.renderblueLines == 1)
            EnableRenderingBlueLines();
        ironBurnRateLerp = 1;
        steelBurnRateLerp = 1;
        forceMagnitudeTarget = 600;

        SearchForMetals(); // first frame of blue lines
    }
    
    public override void StopBurning() {
        base.StopBurning();
        if (HasHighlightedTarget) {
            HighlightedTarget.RemoveTargetGlow();
            HighlightedTarget = null;
        }
        steelBurnRateLerp = 0;
        ironBurnRateLerp = 0;
        forceMagnitudeTarget = 0;
        GamepadController.SetRumble(0, 0);
        DisableRenderingBlueLines();
        RefreshHUD();
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
    public Magnetic SearchForMetals() {
        float smallestDistanceFromCenter = 1f;
        Magnetic centerObject = null;

        for (int i = 0; i < GameManager.MagneticsInScene.Count; i++) {
            Magnetic target = GameManager.MagneticsInScene[i];

            float weightedDistanceFromCenter = SetLineProperties(target, true);
            // If the Magnetic could be targeted
            if (weightedDistanceFromCenter < 1) {
                // IF the new Magnetic is closer to the center of the screen than the previous most-center Magnetic
                if (weightedDistanceFromCenter < smallestDistanceFromCenter) {
                    smallestDistanceFromCenter = weightedDistanceFromCenter;
                    centerObject = target;
                }
            }
        }

        // Update metal lines for Pull/PushTargets
        if (PullingOnPullTargets) {
            PullTargets.UpdateBlueLines(true, IronBurnRateTarget);
        } else if (PushingOnPullTargets) {
            PullTargets.UpdateBlueLines(true, SteelBurnRateTarget);
        } else if (HasPullTarget) {
            PullTargets.UpdateBlueLines(true, 0);
        }
        if (PullingOnPushTargets) {
            PushTargets.UpdateBlueLines(false, IronBurnRateTarget);
        } else if (PushingOnPushTargets) {
            PushTargets.UpdateBlueLines(false, SteelBurnRateTarget);
        } else if (HasPushTarget) {
            PushTargets.UpdateBlueLines(false, 0);
        }

        return centerObject;
    }

    /*
     * Like SearchForMetals, this shows the blue lines to nearby metals, but does not do anything related to target selection.
     * It only updates the visual effect of these blue lines.
     */
    private void OnlyUpdateBlueLines() {
        for (int i = 0; i < GameManager.MagneticsInScene.Count; i++) {
            Magnetic target = GameManager.MagneticsInScene[i];
            SetLineProperties(target, false);
        }
        if (HasHighlightedTarget) {
            HighlightedTarget.RemoveTargetGlow();
        }
    }

    private float SetLineProperties(Magnetic target, bool focus) {
        float allomanticForce = CalculateAllomanticForce(target, this).magnitude;
        // If using Percentage force mode, burn rate affects your range for burning
        if (SettingsMenu.settingsData.pushControlStyle == 0)
            allomanticForce *= GreaterPassiveBurn;

        allomanticForce -= SettingsMenu.settingsData.metalDetectionThreshold; // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold
        if (allomanticForce > 0) {
            // Set line properties
            if (SettingsMenu.settingsData.renderblueLines == 1) {
                Vector3 screenPosition = CameraController.ActiveCamera.WorldToViewportPoint(target.transform.position);

                float centerX = Mathf.Abs(screenPosition.x - .5f);
                float centerY = Mathf.Abs(screenPosition.y - .5f);

                // Calculate the distance from the center for deciding which blue lines are "in-focus"
                float distance = Mathf.Sqrt(
                    (centerX) * (centerX)
                    + (centerY) * (centerY) * importanceRatio
                );

                if (screenPosition.z < 0 || centerX > horizontalImportanceFactor || centerY > verticalImportanceFactor) { // not focusing, or, the target is behind the player, off-screen; Do not highlight this target
                    distance = 1;
                }
                float closeness = Mathf.Exp(-blueLineStartupFactor * Mathf.Pow(1 / allomanticForce, blueLineBrightnessFactor));
                // Make lines in-focus if near the center of the screen
                // If nearly off-screen, instead make lines dimmer
                if (screenPosition.z < 0) { // behind player
                    closeness *= targetFocusOffScreenBound * targetFocusOffScreenBound;
                } else {
                    if (centerX < .44f) {
                        closeness *= targetFocusLowerBound + (1 - targetFocusLowerBound) * Mathf.Exp(-Mathf.Pow(centerX + 1 - horizontalImportanceFactor, targetFocusFalloffConstant));
                    } else {
                        closeness *= targetFocusOffScreenBound + (targetFocusLowerBound - targetFocusOffScreenBound) * Mathf.Exp(-Mathf.Pow(-centerX - .5f - targetLowCurvePosition, targetFocusFalloffConstant));
                    }

                    if (centerY < .44f) {
                        closeness *= targetFocusLowerBound + (1 - targetFocusLowerBound) * Mathf.Exp(-Mathf.Pow(centerY + 1 - verticalImportanceFactor, targetFocusFalloffConstant));
                    } else {
                        closeness *= targetFocusOffScreenBound + (targetFocusLowerBound - targetFocusOffScreenBound) * Mathf.Exp(-Mathf.Pow(-centerY - .5f - targetLowCurvePosition, targetFocusFalloffConstant));
                    }
                }

                target.SetBlueLine(
                    CenterOfMass,
                    blueLineWidthBaseFactor * target.Charge,
                    1,
                    new Color(0, closeness * lowLineColor, closeness * highLineColor, 1)
                    );

                return distance;
            }

        } else { // Magnetic is out of range
            target.DisableBlueLine();
        }
        return 1;
    }

    private void RemoveAllTargets() {
        PullTargets.Clear();
        PushTargets.Clear();

        HUD.TargetOverlayController.Clear();
    }

    private void IncrementNumberOfTargets() {
        PullTargets.IncrementSize();
        PushTargets.IncrementSize();

        HUD.BurnRateMeter.SetMetalLineCountText(PullTargets.Size.ToString());
    }

    private void DecrementNumberOfTargets() {
        PullTargets.DecrementSize();
        PushTargets.DecrementSize();

        HUD.BurnRateMeter.SetMetalLineCountText(PullTargets.Size.ToString());
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
            StopBurning();
    }

    // Increments or decrements the current burn rate
    private void ChangeBurnRateTarget(float change) {
        if (change > 0) {
            change = .10f;
            if (ironBurnRateLerp < .09f || steelBurnRateLerp < .09f) {
                change /= 10f;
            }
        } else if (change < 0) {
            change = -.10f;
            if (ironBurnRateLerp <= .10f || steelBurnRateLerp <= .10f) {
                change /= 10f;
            }
        }
        SetPullRateTarget(Mathf.Clamp(ironBurnRateLerp + change, 0, 1));
        SetPushRateTarget(Mathf.Clamp(steelBurnRateLerp + change, 0, 1));
    }

    // Sets the lerp goal for iron burn rate
    private void SetPullRateTarget(float rate) {
        if (rate > .005f) {
            ironBurnRateLerp = Mathf.Min(1, rate);
        } else {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                ironBurnRateLerp = 1;
            else
                ironBurnRateLerp = 0;
        }
    }
    // Sets the lerp goal for steel burn rate
    private void SetPushRateTarget(float rate) {
        if (rate > .005f) {
            steelBurnRateLerp = Mathf.Min(1, rate);
        } else {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                steelBurnRateLerp = 1;
            else
                steelBurnRateLerp = 0;
        }
    }

    /*
     * Lerps from the current burn rate to the burn rate target for both metals.
     * If the player is not Pulling or Pushing, that burn rate is instead set to 0.
     *      Set, not lerped - there's more precision there.
     */
    private void LerpToBurnRates() {
        IronBurnRateTarget = Mathf.Lerp(IronBurnRateTarget, ironBurnRateLerp, burnRateLerpConstant);
        SteelBurnRateTarget = Mathf.Lerp(SteelBurnRateTarget, steelBurnRateLerp, burnRateLerpConstant);
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
            IronPassiveBurn = 1;
            SteelPassiveBurn = 1;
        } else {
            IronPassiveBurn = IronBurnRateTarget;
            SteelPassiveBurn = SteelBurnRateTarget;
        }

        // Gamepad rumble
        if (HasPullTarget || HasPushTarget) {
            if (IronPulling) {
                GamepadController.SetRumbleRight(IronBurnRateTarget * GamepadController.rumbleFactor);
            } else {
                GamepadController.SetRumbleRight(0);
            }
            if (SteelPushing) {
                GamepadController.SetRumbleLeft(SteelBurnRateTarget * GamepadController.rumbleFactor);
            } else {
                GamepadController.SetRumbleLeft(0);
            }
        } else {
            GamepadController.SetRumble(0, 0);
        }
        // If using the Percentage control scheme and the target burn rate is 0 (and not using a gamepad, which will very often be 0)
        //      Then stop burning metals
        if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != SettingsData.Gamepad && (IronBurnRateTarget < .001f && SteelBurnRateTarget < .001f)) {
            StopBurning();
        }
    }

    private void UpdateBurnRateMeter() {
        if (IsBurningIronSteel) {
            if (SettingsMenu.settingsData.pushControlStyle == 1)
                HUD.BurnRateMeter.SetBurnRateMeterForceMagnitude(LastAllomanticForce, LastAnchoredPushBoost, GreaterBurnRate, forceMagnitudeTarget);
            else if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) {
                if (SteelPushing) {
                    if (IronPulling) {
                        HUD.BurnRateMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                            GreaterBurnRate);
                    } else {
                        HUD.BurnRateMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                            SteelBurnRateTarget);
                    }
                } else {
                    HUD.BurnRateMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                        IronBurnRateTarget);
                }
            } else {
                HUD.BurnRateMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                    GreaterBurnRate);
            }
        } else {
            HUD.BurnRateMeter.Clear();
        }
    }

    // Refreshes all elements of the hud relevent to pushing and pulling
    private void RefreshHUD() {
        RefreshHUDColorsOnly();
        HUD.TargetOverlayController.HardRefresh();
        HUD.BurnRateMeter.HardRefresh();
    }

    // Refreshes the colors of the text of target labels and the burn rate meter.
    private void RefreshHUDColorsOnly() {

        if (PullingOnPullTargets || PushingOnPullTargets) {
            HUD.TargetOverlayController.SetPullTextColorStrong();
            HUD.BurnRateMeter.SetForceTextColorStrong();
        } else {
            HUD.TargetOverlayController.SetPullTextColorWeak();
        }
        if (PullingOnPushTargets || PushingOnPushTargets) {
            HUD.TargetOverlayController.SetPushTextColorStrong();
            HUD.BurnRateMeter.SetForceTextColorStrong();
        } else {
            HUD.TargetOverlayController.SetPushTextColorWeak();
        }

        if (IronPulling || SteelPushing) {
            HUD.BurnRateMeter.SetForceTextColorStrong();
        } else {
            HUD.BurnRateMeter.SetForceTextColorWeak();
        }
    }

    public void EnableRenderingBlueLines() {
        CameraController.ActiveCamera.cullingMask = ~0;
    }

    public void DisableRenderingBlueLines() {
        CameraController.ActiveCamera.cullingMask = ~(1 << blueLineLayer);
    }
}