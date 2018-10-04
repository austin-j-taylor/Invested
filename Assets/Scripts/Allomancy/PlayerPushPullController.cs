using UnityEngine;

/*
 * Controls the blue metal lines that point from the player to nearby metals.
 * Controls the means through which the player selects Pull/PushTargets.
 */
public class PlayerPushPullController : MonoBehaviour {

    // Button-press time constants
    private const float timeToHoldDown = .5f;
    private const float timeDoubleTapWindow = .5f;
    // Blue Line constants
    private const float horizontalMin = .45f;
    private const float horizontalMax = .55f;
    private const float verticalMin = .2f;
    private const float verticalMax = .8f;
    private const float targetFocusRadius = .05f;               // Determines the range around the center of the screen within which blue lines are in focus.
    private const float verticalImportanceFactor = 1 / 20f;     // Determines how elliptical the range around the center of the screen is.
    private const float targetFocusFalloffConstant = 128;       // Determines how quickly blue lines blend from in-focus to out-of-focus
    private const float targetFocusLowerBound = .2f;            // Determines the luminosity of blue lines that are out of foucus
    private const float targetFocusOffScreenBound = .035f;      // Determines the luminosity of blue lines that are off-screen
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

    // Determines if the player just toggled between pushing/pulling and not pushing/pulling
    private bool lastWasPulling = false;
    private bool lastWasPushing = false;

    // burn rate targets
    // When burning metals but not immediately Pushing or Pulling, these will not be set to 0.
    // These are displayed in the Burn Rate Meter
    private float ironBurnRateTarget = 0;
    private float steelBurnRateTarget = 0;
    // Lerp goals for burn rate targets
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

    AllomanticIronSteel player;

    private void Start() {
        player = GetComponent<AllomanticIronSteel>();
    }

    public void Clear() {
        forceMagnitudeTarget = 600;
        HighlightedTarget = null;
    }

    private void Update() {
        if (!PauseMenu.IsPaused) {
            if (Player.CanControlPlayer) {
                // Start burning
                if ((Keybinds.SelectDown() || Keybinds.SelectAlternateDown()) && !Keybinds.Negate()) {
                    player.StartBurning();
                }

                if (player.IsBurningIronSteel) {


                    // Swap pull- and push- targets
                    if (Keybinds.NegateDown() && timeToSwapBurning > Time.time) {
                        // Double-tapped, Swap targets
                        player.PullTargets.SwapContents(player.PushTargets);
                    } else {
                        if (Keybinds.NegateDown()) {
                            timeToSwapBurning = Time.time + timeDoubleTapWindow;
                        }
                    }

                    // Check scrollwheel for changing the max number of targets and burn rate, or DPad if using gamepad
                    float scrollValue = 0;
                    if (SettingsMenu.settingsData.controlScheme == 2) { // Gamepad
                        scrollValue = Keybinds.DPadXAxis();
                        if (SettingsMenu.settingsData.pushControlStyle == 1) {
                            ChangeTargetForceMagnitude(Keybinds.DPadYAxis());
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
                        if (SettingsMenu.settingsData.controlScheme == 2) {
                            SetPullRateTarget(Keybinds.RightBurnRate());
                            SetPushRateTarget(Keybinds.LeftBurnRate());
                        }
                    } else { // Magnitude
                        if (player.HasPullTarget || player.HasPushTarget) {

                            //Debug.Log(player.LastMaximumNetForce);

                            float maxNetForce = (player.LastMaximumNetForce).magnitude;
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
                            player.StopBurning();
                            timeToStopBurning = 0;
                        }
                    } else {
                        timeToStopBurning = 0;
                    }

                    LerpToBurnRates();
                    UpdateBurnRateMeter();
                }

                // Could have stopped burning above. Check if the Allomancer is still burning.
                if (player.IsBurningIronSteel) {

                    player.IronPulling = Keybinds.IronPulling();
                    player.SteelPushing = Keybinds.SteelPushing();

                    // If you are trying to push and pull and only have pullTargets, only push. And vice versa
                    if (!player.HasPushTarget && player.HasPullTarget) {
                        if (player.IronPulling)
                            player.SteelPushing = false;
                    } else
                    if (!player.HasPullTarget && player.HasPushTarget) {
                        if (player.SteelPushing)
                            player.IronPulling = false;
                    }

                    // Change colors of target labels when toggling pushing/pulling
                    if (player.IronPulling) {
                        if (!lastWasPulling) { // first frame of pulling
                            RefreshHUDColorsOnly();
                            lastWasPulling = true;
                            //StartPushPullingOnTargets(iron);
                        }
                    } else {
                        if (lastWasPulling) { // first frame of NOT pulling
                            RefreshHUDColorsOnly();
                            lastWasPulling = false;
                            if (player.HasPullTarget) {
                                player.StopOnPullTargets();
                            } else {
                                player.StopOnPushTargets();
                            }
                        }
                    }
                    if (player.SteelPushing) {
                        if (!lastWasPushing) { // first frame of pushing
                            RefreshHUDColorsOnly();
                            lastWasPushing = true;
                            //StartPushPullingOnTargets(steel);
                        }
                    } else {
                        if (lastWasPushing) { // first frame of NOT pushing
                            RefreshHUDColorsOnly();
                            lastWasPushing = false;
                            if (player.HasPushTarget) {
                                player.StopOnPushTargets();
                            } else {
                                player.StopOnPullTargets();
                            }
                        }
                    }

                    // Check input for target selection
                    bool selecting = (Keybinds.Select() || Keybinds.SelectAlternate()) && !Keybinds.Negate();
                    Magnetic target = SearchForMetals();

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
                        if (Keybinds.Select()) { // Selecting pull target
                            if (selecting) {
                                player.AddPullTarget(target);
                            } else {
                                if (player.RemovePullTarget(target)) {// If the player is hovering over a pullTarget, instantly remove that one. Keep it highlighted.
                                    target.AddTargetGlow();
                                } else if (Keybinds.SelectDown() && !player.RemovePullTarget(target)) { // If the highlighted Magnetic is not a pullTarget, remove the oldest pullTarget instead
                                    player.RemovePullTargetAt(0);
                                }
                            }
                        }
                        if (Keybinds.SelectAlternate()) {
                            if (selecting) {
                                player.AddPushTarget(target);
                            } else {
                                if (player.RemovePushTarget(target)) {
                                    target.AddTargetGlow();
                                } else if (Keybinds.SelectAlternateDown() && !player.RemovePushTarget(target)) {
                                    player.RemovePushTargetAt(0);
                                }
                            }
                        }
                    }

                    RefreshHUD();
                }
            } else { // If the player is not in control, but still burning metals, show blue lines to metals.
                if (player.IsBurningIronSteel) {
                    OnlyUpdateBlueLines();
                }
            }
        }
    }

    /*
     * Called only by AllomanticIronSteel.StartBurning()
     */
    public void StartBurningIronSteel() {
        GamepadController.Shake(.1f, .1f, .3f);
        UpdateBurnRateMeter();
        HUD.BurnRateMeter.SetMetalLineCountText(player.PullTargets.Size.ToString());
        if(SettingsMenu.settingsData.renderblueLines == 1)
            EnableRenderingBlueLines();
        ironBurnRateLerp = 1;
        steelBurnRateLerp = 1;
        forceMagnitudeTarget = 600;
    }

    /*
     * Called only by AllomanticIronSteel.StopBurning()
     */
    public void StopBurningIronSteel() {
        if (HasHighlightedTarget) {
            HighlightedTarget.RemoveTargetGlow();
            HighlightedTarget = null;
        }
        steelBurnRateTarget = 0;
        ironBurnRateTarget = 0;
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

            float allomanticForce = AllomanticIronSteel.CalculateAllomanticForce(target, player).magnitude;
            // If using Percentage force mode, burn rate affects your range for burning
            if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != 2)
                allomanticForce *= Mathf.Max(ironBurnRateTarget, steelBurnRateTarget);

            allomanticForce -= SettingsMenu.settingsData.metalDetectionThreshold; // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold
            if (allomanticForce > 0) {
                Vector3 screenPosition = CameraController.ActiveCamera.WorldToViewportPoint(target.transform.position);

                // Calculate the distance from the center for deciding which blue lines are "in-focus"
                float weightedDistanceFromCenter = Mathf.Sqrt(Mathf.Pow(screenPosition.x - .5f, 2) + verticalImportanceFactor * Mathf.Pow(screenPosition.y - .5f, 2));
                if (screenPosition.z < 0) { // the target is behind the player, off-screen; Do not highlight this target
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
                    float closeness = Mathf.Exp(-blueLineStartupFactor * Mathf.Pow(1 / allomanticForce, blueLineBrightnessFactor));
                    // Make lines in-focus if near the center of the screen
                    if (screenPosition.z < 0)
                        closeness *= targetFocusOffScreenBound;
                    else
                        closeness *= targetFocusLowerBound + (1 - targetFocusLowerBound) * Mathf.Exp(-Mathf.Pow(weightedDistanceFromCenter + 1 - targetFocusRadius, targetFocusFalloffConstant));
                    target.SetBlueLine(
                        player.CenterOfMass,
                        blueLineWidthBaseFactor * target.Charge,
                        1,
                        new Color(0, closeness * lowLineColor, closeness * highLineColor, 1)
                        );
                }
            } else { // Magnetic is out of range
                target.DisableBlueLine();
            }
        }

        // Update metal lines for Pull/PushTargets
        if(player.HasPullTarget) {
            if(player.HasPushTarget) {
                player.PullTargets.UpdateBlueLines(true, player.IronBurnRate);
                player.PushTargets.UpdateBlueLines(false, player.SteelBurnRate);
            } else {
                player.PullTargets.UpdateBlueLines(true, Mathf.Max(player.IronBurnRate, player.SteelBurnRate));
            }
        } else {
            if(player.HasPushTarget) {
                player.PushTargets.UpdateBlueLines(false, Mathf.Max(player.IronBurnRate, player.SteelBurnRate));
            }
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

            float allomanticForce = AllomanticIronSteel.CalculateAllomanticForce(target, player).magnitude;
            // If using Percentage force mode, burn rate affects your range for burning
            if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != 2)
                allomanticForce *= Mathf.Max(ironBurnRateTarget, steelBurnRateTarget);

            allomanticForce -= SettingsMenu.settingsData.metalDetectionThreshold; // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold
            if (allomanticForce > 0) {
                bool isOffScreen =  CameraController.ActiveCamera.WorldToViewportPoint(target.transform.position).z < 0;

                // Set line properties
                if (SettingsMenu.settingsData.renderblueLines == 1) {
                    float closeness = Mathf.Exp(-blueLineStartupFactor * Mathf.Pow(1 / allomanticForce, blueLineBrightnessFactor));
                    // Make lines in-focus if near the center of the screen
                    if (isOffScreen)
                        closeness *= targetFocusOffScreenBound;
                    else
                        closeness *= targetFocusLowerBound + (1 - targetFocusLowerBound) * Mathf.Exp(-Mathf.Pow(1 - targetFocusRadius, targetFocusFalloffConstant));
                    target.SetBlueLine(
                        player.CenterOfMass,
                        blueLineWidthBaseFactor * target.Charge,
                        1,
                        new Color(0, closeness * lowLineColor, closeness * highLineColor, 1)
                        );
                }
            } else { // Magnetic is out of range
                target.DisableBlueLine();
            }
        }
        if(HasHighlightedTarget) {
            HighlightedTarget.RemoveTargetGlow();
        }
    }

    private void RemoveAllTargets() {
        player.PullTargets.Clear();
        player.PushTargets.Clear();

        HUD.TargetOverlayController.Clear();
    }

    private void IncrementNumberOfTargets() {
        player.PullTargets.IncrementSize();
        player.PushTargets.IncrementSize();

        HUD.BurnRateMeter.SetMetalLineCountText(player.PullTargets.Size.ToString());
    }

    private void DecrementNumberOfTargets() {
        player.PullTargets.DecrementSize();
        player.PushTargets.DecrementSize();

        HUD.BurnRateMeter.SetMetalLineCountText(player.PullTargets.Size.ToString());
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
            player.StopBurning();
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
        if (rate > .001f) {
            ironBurnRateLerp = Mathf.Min(1, rate);
        } else {
            ironBurnRateLerp = 0;
        }
    }
    // Sets the lerp goal for steel burn rate
    private void SetPushRateTarget(float rate) {
        if (rate > .001f) {
            steelBurnRateLerp = Mathf.Min(1, rate);
        } else {
            steelBurnRateLerp = 0;
        }
    }

    /*
     * Lerps from the current burn rate to the burn rate target for both metals.
     * If the player is not Pulling or Pushing, that burn rate is instead set to 0.
     *      Set, not lerped - there's more precision there.
     */
    private void LerpToBurnRates() {
        ironBurnRateTarget = Mathf.Lerp(ironBurnRateTarget, ironBurnRateLerp, burnRateLerpConstant);
        steelBurnRateTarget = Mathf.Lerp(steelBurnRateTarget, steelBurnRateLerp, burnRateLerpConstant);
        if (player.IronPulling) {
            player.IronBurnRate = ironBurnRateTarget;
        } else {
            player.IronBurnRate = 0;
        }
        if(player.SteelPushing) {
            player.SteelBurnRate = steelBurnRateTarget;
        } else {
            player.SteelBurnRate = 0;
        }

        // Gamepad rumble
        if (player.HasPullTarget || player.HasPushTarget) {
            if (player.IronPulling) {
                GamepadController.SetRumbleRight(player.IronBurnRate * GamepadController.rumbleFactor);
            } else {
                GamepadController.SetRumbleRight(0);
            }
            if (player.SteelPushing) {
                GamepadController.SetRumbleLeft(player.SteelBurnRate * GamepadController.rumbleFactor);
            } else {
                GamepadController.SetRumbleLeft(0);
            }
        } else {
            GamepadController.SetRumble(0, 0);
        }
        // If using the Percentage control scheme and the target burn rate is 0 (and not using a gamepad, which will very often be 0)
        //      Then stop burning metals
        if (SettingsMenu.settingsData.pushControlStyle == 0 && SettingsMenu.settingsData.controlScheme != 2 && (ironBurnRateTarget < .001f && steelBurnRateTarget < .001f)) {
            player.StopBurning();
        }
    }

    private void UpdateBurnRateMeter() {
        if (player.IsBurningIronSteel) {
            if (SettingsMenu.settingsData.pushControlStyle == 1)
                HUD.BurnRateMeter.SetBurnRateMeterForceMagnitude(player.LastAllomanticForce, player.LastNormalForce, Mathf.Max(ironBurnRateTarget, steelBurnRateTarget), forceMagnitudeTarget);
            else
                HUD.BurnRateMeter.SetBurnRateMeterPercentage(player.LastAllomanticForce, player.LastNormalForce, Mathf.Max(ironBurnRateTarget, steelBurnRateTarget));
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

        if (player.PullingOnPullTargets || player.PushingOnPullTargets) {
            HUD.TargetOverlayController.SetPullTextColorStrong();
            HUD.BurnRateMeter.SetForceTextColorStrong();
        } else {
            HUD.TargetOverlayController.SetPullTextColorWeak();
        }
        if (player.PullingOnPushTargets || player.PushingOnPushTargets) {
            HUD.TargetOverlayController.SetPushTextColorStrong();
            HUD.BurnRateMeter.SetForceTextColorStrong();
        } else {
            HUD.TargetOverlayController.SetPushTextColorWeak();
        }

        if (player.IronPulling || player.SteelPushing) {
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