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
    private const float targetFocusFalloffConstant = 128;       // Determines how quickly blue lines blend from in-focus to out-of-focus
    private const float targetFocusLowerBound = .5f;            // Determines the luminosity of blue lines that are out of foucus
    private const float targetFocusOffScreenBound = .3f;      // Determines the luminosity of blue lines that are off-screen
    private const float targetLowTransition = .06f;
    private const float targetLowCurvePosition = .02f;

    // Other Constants
    private const float burnPercentageLerpConstant = .30f;
    private const int blueLineLayer = 10;
    private const float metalLinesLerpConstant = .30f;

    // Button held-down times
    private float timeToStopBurning = 0f;
    private float timeToSwapBurning = 0f;

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

    public override void Clear(bool clearTargets = true) {
        DisableRenderingBlueLines();
        base.Clear(clearTargets);
    }

    public void SoftClear() {
        if (HasHighlightedTarget)
            HighlightedTarget.RemoveTargetGlow();
        IronPulling = false;
        SteelPushing = false;
        RemoveAllTargets();
    }

    /*
     * Read inputs for selecting targets.
     * Update burn percentages.
     * Update blue lines pointing from player to metal.
     */
    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            // Start and Stop Burning metals
            if (IsBurning) {
                // Stop burning
                if (Keybinds.Negate()) {
                    timeToStopBurning += Time.deltaTime;
                    if (Keybinds.Select() && Keybinds.SelectAlternate() && timeToStopBurning > timeToHoldDown) {
                        StopBurning();
                        timeToStopBurning = 0;
                    }
                } else {
                    timeToStopBurning = 0;
                }
            } else {
                // Start burning
                if (!Keybinds.Negate()) {
                    if (Keybinds.SelectDown())
                        StartBurning(true);
                    else if (Keybinds.SelectAlternateDown())
                        StartBurning(false);
                }
            }

            // Could have stopped burning above. Check if the Allomancer is still burning.
            if (IsBurning) {

                // Change Burn Percentage Targets, Number of Targets
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
                if (scrollValue > 0) {
                    IncrementNumberOfTargets();
                }
                if (scrollValue < 0) {
                    DecrementNumberOfTargets();
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
                        } else {
                            if (Keybinds.NegateDown()) {
                                timeToSwapBurning = Time.time + timeDoubleTapWindow;
                            }
                        }

                        // Search for Metals

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

                        // Add/Remove Targets

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
        UpdateBurnRateMeter();
        HUD.BurnPercentageMeter.SetMetalLineCountText(PullTargets.Size.ToString());
        if (SettingsMenu.settingsData.renderblueLines == 1)
            EnableRenderingBlueLines();
        ironBurnPercentageLerp = 1;
        steelBurnPercentageLerp = 1;
        forceMagnitudeTarget = 600;

        SearchForMetals(); // first frame of blue lines

        return true;
    }

    public override void StopBurning() {
        base.StopBurning();
        if (HasHighlightedTarget) {
            HighlightedTarget.RemoveTargetGlow();
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

    /*
     * Searches all Magnetics in the scene for those that are within detection range of the player.
     * Shows metal lines drawing from them to the player.
     * Returns the Magnetic "closest" of these to the center of the screen.
     * 
     * If targetedLineColors is false, then push/pullTargets will not have specially colored lines (i.e. red, green, light blue)
     * 
     * Rules for the metal lines:
     *  - The WIDTH of the line is dependant on the MASS of the target
     *  - The BRIGHTNESS of the line is dependent on the DISTANCE from the player
     *  - The LIGHT SABER FACTOR is dependent on the FORCE acting on the target. If the metal is not a target, it is 1.
     */
    public Magnetic SearchForMetals(bool targetedLineColors = true) {
        float smallestDistanceFromCenter = 1f;
        Magnetic centerObject = null;
        bool mustCalculateCenter = true;

        // If the player is directly looking at a magnetic's collider
        //if (Physics.SphereCast(CameraController.ActiveCamera.transform.position, .5f, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 500, GameManager.Layer_IgnorePlayer)) {
        if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 500, GameManager.Layer_IgnorePlayer)) {
            Magnetic target = hit.collider.GetComponentInParent<Magnetic>();
            if (target) {
                centerObject = target;
                mustCalculateCenter = false;
            }
        }
        
        // If the player is not directly looking at a magnetic, select the one closest to the center of the screen

        for (int i = 0; i < GameManager.MagneticsInScene.Count; i++) {
            Magnetic target = GameManager.MagneticsInScene[i];
            if(target.isActiveAndEnabled && target != Player.PlayerMagnetic) {
                if (mustCalculateCenter) { // If player is not directly looking at magnetic, calculate which is closest
                    float weightedDistanceFromCenter = SetLineProperties(target);

                    // If looking for the object at the center of the screen
                    // If the Magnetic could be targeted
                    if (targetedLineColors && weightedDistanceFromCenter < 1) {
                        // IF the new Magnetic is closer to the center of the screen than the previous most-center Magnetic
                        if (weightedDistanceFromCenter < smallestDistanceFromCenter) {
                            smallestDistanceFromCenter = weightedDistanceFromCenter;
                            centerObject = target;
                        }
                    }
                } else { // If player is directly looking at magnetic, just set line properties
                    SetLineProperties(target);
                }
            }
        }
        if (centerObject) {
            // Make the blue line to the center object brighter
            centerObject.BrightenLine();
        }


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
        return centerObject;
    }

    /*
     * Checks several factors and sets the properties of the blue line pointing to target.
     * These factors are described in the above function.
     */
    private float SetLineProperties(Magnetic target) {
        Vector3 allomanticForceVector = CalculateAllomanticForce(target);
        float allomanticForce = allomanticForceVector.magnitude;
        // If using Percentage force mode, burn percentage affects your range for burning
        if (SettingsMenu.settingsData.pushControlStyle == 0)
            allomanticForce *= GreaterPassiveBurn;
        
        allomanticForce -= SettingsMenu.settingsData.metalDetectionThreshold; // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold

        if (allomanticForce <= 0) {
            // Magnetic is out of range
            target.DisableBlueLine();
            return 1;
        }

        // Set line properties
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
        if (SettingsMenu.settingsData.renderblueLines == 1) {
            //float closeness = Mathf.Exp(-blueLineChangeFactor * Mathf.Pow(1 / allomanticForce, blueLineBrightnessFactor));
            //float closeness = .125f * Mathf.Pow(allomanticForce, .25f);
            float closeness = blueLineBrightnessFactor * Mathf.Pow(allomanticForce, blueLineChangeFactor);
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
                target.Charge * (SettingsMenu.settingsData.cameraFirstPerson == 0 ? blueLineThirdPersonWidth : blueLineFirstPersonWidth),
                1,
                new Color(0, closeness * lowLineColor, closeness * highLineColor, 1)
                );
        }
        return distance;
    }

    public void RemoveAllTargets() {
        PullTargets.Clear();
        PushTargets.Clear();

        HUD.TargetOverlayController.Clear();
    }

    private void IncrementNumberOfTargets() {
        PullTargets.IncrementSize();
        PushTargets.IncrementSize();

        HUD.BurnPercentageMeter.SetMetalLineCountText(PullTargets.Size.ToString());
    }

    private void DecrementNumberOfTargets() {
        PullTargets.DecrementSize();
        PushTargets.DecrementSize();

        HUD.BurnPercentageMeter.SetMetalLineCountText(PullTargets.Size.ToString());
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
                    if(IronPulling) {
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
        RefreshHUDColorsOnly();
        HUD.TargetOverlayController.HardRefresh();
        HUD.BurnPercentageMeter.HardRefresh();
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
}