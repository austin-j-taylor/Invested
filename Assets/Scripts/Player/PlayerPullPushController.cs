using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using Unity.Jobs;
using Unity.Collections;

/// <summary>
/// The AllomanticIronSteel specific for the Player.
/// Controls the blue metal lines that point from the player to nearby metals.
/// Controls the means through which the player selects Pull/PushTargets.
/// </summary>
public class PlayerPullPushController : AllomanticIronSteel {

    #region constants
    // Button-press time constants
    private const float timeToHoldDown = .5f;
    private const float timeDoubleTapWindow = .5f;
    // Blue Line constants
    private const float lineWeightThreshold = 1;
    private const float targetFocusOffScreenBound = .2f;      // Determines the luminosity of blue lines that are off-screen
    public const float firstPersonWidthFactor = .1f;
    public const float blueLineWidthFactor = .02f;
    public const float blueLineChangeFactor = 1 / 4f;
    public const float blueLineBrightnessFactor = .15f;
    private readonly Vector3 firstPersonCenterOfMassOffset = new Vector3(0, -0.20f, 0);

    // Control Mode constants
    private const float minAreaRadius = .0251f;
    public const float maxAreaRadius = .25f;
    private const float areaRadiusIncrement = .025f;
    private const int minBubbleRadius = 1;
    private const int maxBubbleRadius = 10;
    private const int bubbleRadiusIncrement = 1;
    private const float bubbleIntensityMin = 0.25f;
    private const float bubbleIntensityMax = 0.75f;
    // Other Constants
    private const float burnPercentageLerpConstant = .30f;
    private const float areaLerpConstant = .4f;
    private const int blueLineLayer = 10;
    private const float metalLinesLerpConstant = .30f;
    private const int defaultCharge = 1;
    #endregion
    public enum ControlMode { Manual, Area, Bubble, Coinshot };

    public ControlMode Mode { get; private set; }
    public Vector3 CenterOfBlueLines => CameraController.IsFirstPerson ? CenterOfMass + firstPersonCenterOfMassOffset : CenterOfMass; // The position (in world space) where the blue lines appear from

    private Magnetic removedTarget; // When a target was just removed, keep track of it so we don't immediately select it again
    // radius for Area
    private float selectionAreaRadius = maxAreaRadius / 2;
    // Lerp goals for burn percentage targets
    // These are displayed in the Burn Rate Meter
    private float ironBurnPercentageLerp = 0;
    private float steelBurnPercentageLerp = 0;
    private float bubbleBurnPercentageLerp = 0;
    // Lerp goal for bubble/area radius
    private float bubbleRadiusLerp = 0;
    private float areaRadiusLerp = 0;
    // for Magnitude control style
    private float forceMagnitudeTarget = 600;

    #region clearing
    public override void Clear() {
        StopBurning();
        Strength = 1;
        removedTarget = null;
        base.Clear();
    }

    public void SoftClear() {
        IronPulling = false;
        SteelPushing = false;
        RemoveAllTargets();
    }

    protected override void Awake() {
        base.Awake();

        BubbleMetalStatus = iron;
        Mode = ControlMode.Manual;
        PullTargets.Size = TargetArray.smallArrayCapacity;
        PushTargets.Size = TargetArray.smallArrayCapacity;
        bubbleRenderer = transform.Find("BubbleRange").GetComponent<Renderer>();
        BubbleTargets = new TargetArray(TargetArray.largeArrayCapacity);
    }
    protected override void InitArrays() {
        PullTargets = new TargetArray(TargetArray.largeArrayCapacity);
        PushTargets = new TargetArray(TargetArray.largeArrayCapacity);
    }
    public void RemoveAllTargets() {
        PullTargets.Clear();
        PushTargets.Clear();

        HUD.TargetOverlayController.Clear();
    }
    #endregion

    #region updatePushingAndPulling
    /// <summary>
    /// Update Pushing/Pulling status and other control properties
    /// Update blue lines pointing to nearby metals.
    /// Read inputs for marking targets.
    /// </summary>
    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            if (IsBurning) {
                if (!ExternalControl && Player.CanControl) {

                    UpdateBurnPercentagesAndRadius();
                    LerpToBurnPercentages();
                    LerpToBubbleSize();
                    UpdateBurnRateMeter();
                    // Could have stopped burning above. Check if the Allomancer is still burning.
                    if (IsBurning) {
                        UpdatePushingPulling();
                        SetTargetedLineProperties();
                        RefreshHUD();
                    }
                } else { // If the player is not in control, but still burning metals, show blue lines to metals.
                    if (IsBurning) {
                        LerpToBurnPercentages();
                        LerpToBubbleSize();
                        UpdateBlueLines();
                        UpdateBurnRateMeter();
                        RefreshHUD();
                    }
                }
            }

            // Start and Stop Burning metals
            if (Player.CanControl && !ExternalControl) {
                if (IsBurning) {
                    // Stop burning
                    if (Keybinds.StopBurning()) {
                        StopBurning();
                    }
                } else {
                    // Start burning (as long as the Control Wheel isn't open to misinterprent your mouse clicks, etc)
                    if (Keybinds.StopBurning()) {
                        StartBurning();
                    } else if (!HUD.ControlWheelController.IsOpen) {
                        if (Keybinds.SelectDown() || Keybinds.PullDown()) {
                            StartBurning(true);
                        } else if (Keybinds.SelectAlternateDown() || Keybinds.PushDown()) {
                            StartBurning(false);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Changes burn percentage targets and bubble/area radius depending on player input
    /// </summary>
    private void UpdateBurnPercentagesAndRadius() {

        // Change Burn Percentage Targets
        // Check scrollwheel for changing burn percentage and bubble/area radii, or DPad if using gamepad
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) { // Gamepad
            // Bubble/area radii determined by the DPad axes
            ChangeRadius(Keybinds.DPadYAxis());
            if (SettingsMenu.settingsAllomancy.pushControlStyle == 1) {
                ChangeTargetForceMagnitude(Keybinds.DPadXAxis());
            }
        } else { // Mouse and keyboard
            // Burn percentages and radii are determined by scroll wheel and whether the Control Wheel is open
            if (Keybinds.ControlWheel() && (Mode == ControlMode.Area || Mode == ControlMode.Bubble)) {
                // Control Wheel open? Change the bubble/area radii.
                ChangeRadius(Keybinds.ScrollWheelAxis());
            } else {
                if (SettingsMenu.settingsAllomancy.pushControlStyle == 0) {
                    // Control Wheel closed, and we're in Percentage? Change that.
                    if (Mode == ControlMode.Bubble)
                        ChangeBurnPercentageTargetBubble(Keybinds.ScrollWheelAxis());
                    else
                        ChangeBurnPercentageTargetManual(Keybinds.ScrollWheelAxis());
                } else {
                    // In Magnitude? Change that.
                    ChangeTargetForceMagnitude(Keybinds.ScrollWheelAxis());
                }
            }
        }

        // Assign Burn percentage targets based on the previously changed burn percentage/target magnitudes
        if (SettingsMenu.settingsAllomancy.pushControlStyle == 0) { // Percentage
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) { // Gamepad
                                                                                                // If throwing a coin, the Pushing burn target is 1.
                if (Mode == ControlMode.Bubble) {
                    if (Keybinds.WithdrawCoin()) {
                        SteelBurnPercentageTarget = 1;
                        steelBurnPercentageLerp = 1;
                    }
                    SetBubblePercentageTarget(Mathf.Max(Keybinds.LeftBurnPercentage(), Keybinds.RightBurnPercentage()));
                } else {
                    if (Keybinds.WithdrawCoin()) {
                        SteelBurnPercentageTarget = 1;
                        steelBurnPercentageLerp = 1;
                    } else {
                        SetPushPercentageTarget(Keybinds.LeftBurnPercentage());
                    }
                    SetPullPercentageTarget(Keybinds.RightBurnPercentage());
                }
            }
        } else { // Magnitude
            if (HasPullTarget || HasPushTarget) {

                float maxNetForce = (LastMaximumNetForce).magnitude;
                SetPullPercentageTarget(forceMagnitudeTarget / maxNetForce);
                SetPushPercentageTarget(forceMagnitudeTarget / maxNetForce);
            } else {
                SetPullPercentageTarget(0);
                SetPushPercentageTarget(0);
            }
        }
    }

    /// <summary>
    /// Updates the status of Pushing and Pulling from player input.
    /// </summary>
    private void UpdatePushingPulling() {
        bool pulling, pushing;
        bool keybindPulling = Keybinds.IronPulling();
        bool keybindPushing = Keybinds.SteelPushing() || Keybinds.WithdrawCoin(); // for Pushing on coins after tossing them
        bool keybindPullingDown = Keybinds.PullDown();
        bool keybindPushingDown = Keybinds.PushDown();

        // Coinshot mode: 
        // If do not have pull-targets, pulling is also Pushing
        // If you were pressing the button to Pull, it's now to Push
        if (Mode == ControlMode.Coinshot) {
            pulling = keybindPulling && HasIron && HasPullTarget;
            pushing = keybindPushing && HasSteel;
            // if LMB while has a push target, Push and don't Pull.
            if (keybindPulling && !HasPullTarget) {
                pushing = true;
                pulling = false;
                keybindPulling = false;
                keybindPushing = true;
                keybindPullingDown = false;
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

        UpdateTargetSelection(keybindPullingDown, keybindPulling, keybindPushingDown, keybindPushing);
    }

    /// <summary>
    /// Searches for and Marks new targets depending on player input.
    /// </summary>
    private void UpdateTargetSelection(bool keybindPullingDown, bool keybindPulling, bool keybindPushingDown, bool keybindPushing) {

        // Check input for target selection
        bool markForPull = Keybinds.Select() && HasIron;
        bool markForPullDown = Keybinds.SelectDown() && HasIron;
        bool markForPush = Keybinds.SelectAlternate() && HasSteel;
        bool markForPushDown = Keybinds.SelectAlternateDown() && HasSteel;

        // Search for Metals
        Magnetic bullseyeTarget;
        List<Magnetic> newTargetsArea;
        List<Magnetic> newTargetsBubble;
        (bullseyeTarget, newTargetsArea, newTargetsBubble) = IronSteelSight();
        if (Keybinds.SelectUp() || Keybinds.SelectAlternateUp()) {
            // No longer need to worry about remarking what we just marked
            removedTarget = null;
        }
        // Assign targets depending on the control mode.
        switch (Mode) {
            case ControlMode.Manual:
            // fall through, same as coinshot
            case ControlMode.Coinshot:
                // set the bullseye target to be brighter
                if (bullseyeTarget)
                    bullseyeTarget.BrightenLine();
                if (markForPullDown) {
                    if (Keybinds.MultipleMarks()) {
                        if (bullseyeTarget != null) {
                            if (PullTargets.IsTarget(bullseyeTarget)) {
                                // If the bullseye target is already selected, we want to remove it instead.
                                RemovePullTarget(bullseyeTarget);
                                removedTarget = bullseyeTarget;
                            } else {
                                // It's not a target. Add new one.
                                AddPullTarget(bullseyeTarget);
                                removedTarget = null;
                            }
                        }
                    } else {
                        // If there is no bullseye target, deselect all targets.
                        if (bullseyeTarget == null) {
                            PullTargets.Clear();
                        } else if (PullTargets.IsTarget(bullseyeTarget)) {
                            // If the bullseye target is already selected, we want to remove it instead.
                            PullTargets.Clear();
                            removedTarget = bullseyeTarget;
                        } else {
                            // It's not a target. Remove old target and add new one.
                            PullTargets.Clear();
                            AddPullTarget(bullseyeTarget);
                            removedTarget = null;
                        }
                    }
                } else if (markForPull) {
                    if (bullseyeTarget != null) {
                        if (PullTargets.IsTarget(bullseyeTarget)) {
                            if (!Keybinds.MultipleMarks()) {
                                PullTargets.Clear();
                                if (removedTarget != bullseyeTarget) {
                                    AddPullTarget(bullseyeTarget);
                                    removedTarget = null;
                                }
                            }
                        } else {
                            if (removedTarget != bullseyeTarget) {
                                // It's not a target. Add it. If we're multitargeting, preserve the old target.
                                if (!Keybinds.MultipleMarks())
                                    PullTargets.Clear();
                                AddPullTarget(bullseyeTarget);
                                removedTarget = null;
                            }
                        }
                    }
                } else {
                    // Not holding down Negate nor Select this round. Consider vacuous pulling.
                    if (keybindPullingDown) {
                        if (!HasMarkedPullTarget) {
                            AddPullTarget(bullseyeTarget, true, true);
                        }
                    } else {
                        if (!keybindPulling && PullTargets.VacuousCount > 0) {
                            // If we were vacuously pulling but we just released the Pull, remove those targets.
                            PullTargets.RemoveAllVacuousTargets();
                        }
                    }
                }

                if (markForPushDown) {
                    if (Keybinds.MultipleMarks()) {
                        if (bullseyeTarget != null) {
                            if (PushTargets.IsTarget(bullseyeTarget)) {
                                // If the bullseye target is already selected, we want to remove it instead.
                                RemovePushTarget(bullseyeTarget);
                                removedTarget = bullseyeTarget;
                            } else {
                                // It's not a target. Add new one.
                                AddPushTarget(bullseyeTarget);
                                removedTarget = null;
                            }
                        }
                    } else {
                        // If there is no bullseye target, deselect all targets.
                        if (bullseyeTarget == null) {
                            PushTargets.Clear();
                        } else if (PushTargets.IsTarget(bullseyeTarget)) {
                            // If the bullseye target is already selected, we want to remove it instead.
                            PushTargets.Clear();
                            removedTarget = bullseyeTarget;
                        } else {
                            // It's not a target. Remove old target and add new one.
                            PushTargets.Clear();
                            AddPushTarget(bullseyeTarget);
                            removedTarget = null;
                        }
                    }
                } else if (markForPush) {
                    if (bullseyeTarget != null) {
                        if (PushTargets.IsTarget(bullseyeTarget)) {
                            if (!Keybinds.MultipleMarks()) {
                                PushTargets.Clear();
                                if (removedTarget != bullseyeTarget) {
                                    AddPushTarget(bullseyeTarget);
                                    removedTarget = null;
                                }
                            }
                        } else {
                            if (removedTarget != bullseyeTarget) {
                                // It's not a target. Add it. If we're multitargeting, preserve the old target.
                                if (!Keybinds.MultipleMarks())
                                    PushTargets.Clear();
                                AddPushTarget(bullseyeTarget);
                                removedTarget = null;
                            }
                        }
                    }
                } else {
                    // Not holding down Negate nor Select this round. Consider vacuous pulling.
                    if (keybindPushingDown) {
                        if (!HasMarkedPushTarget) {
                            AddPushTarget(bullseyeTarget, true, true);
                        }
                    } else {
                        if (!keybindPushing && PushTargets.VacuousCount > 0) {
                            // If we were vacuously Pushing but we just released the Push, remove those targets.
                            PushTargets.RemoveAllVacuousTargets();
                        }
                    }
                }
                break;
            case ControlMode.Area:
                // Crosshair: lerp to the correct size
                LerpToAreaSize(selectionAreaRadius);
                // set the targets in the area to be brighter
                for (int i = 0; i < newTargetsArea.Count; i++) {
                    newTargetsArea[i].BrightenLine();
                }
                if (HasIron) {
                    if (markForPullDown) {
                        if (Keybinds.MultipleMarks()) {
                            if (bullseyeTarget != null) {
                                //if (PullTargets.IsTarget(bullseyeTarget)) {
                                //    // If the bullseye target is already selected, we want to remove targets instead.
                                //    PullTargets.RemoveTargets(newTargetsArea);
                                //    //removingTargets = PullTargets.VacuousCount == 0;
                                //} else {
                                // They're not targets. Add them.
                                // O(n^2)ish
                                for (int i = 0; i < newTargetsArea.Count; i++) {
                                    PullTargets.AddTarget(newTargetsArea[i], false);
                                }
                                //}
                            }
                        } else {
                            //if (PullTargets.IsTarget(bullseyeTarget)) {
                            //    // If the bullseye target is already selected, we want to remove targets instead.
                            //    PullTargets.RemoveTargets(newTargetsArea);
                            //    removingTargets = PullTargets.VacuousCount == 0;
                            //} else {
                            // It's not a target. Remove old targets and add new one.
                            PullTargets.ReplaceContents(newTargetsArea, false);
                            //}
                        }
                    } else if (markForPull) {
                        if (Keybinds.MultipleMarks()) {
                            //if (removingTargets) {
                            //    PullTargets.RemoveTargets(newTargetsArea);
                            //} else {
                            for (int i = 0; i < newTargetsArea.Count; i++) {
                                PullTargets.AddTarget(newTargetsArea[i], false);
                            }
                            //}
                        } else {
                            //if (removingTargets) {
                            //    PullTargets.RemoveTargets(newTargetsArea);
                            //} else {
                            PullTargets.ReplaceContents(newTargetsArea, false);
                            //}
                        }
                    } else {
                        // Not holding down Negate nor Select this round. Consider vacuous pulling.
                        if (!HasPullTarget) {
                            if (keybindPullingDown) {
                                PullTargets.ReplaceContents(newTargetsArea, true);
                            }
                        } else if (!keybindPulling && PullTargets.VacuousCount > 0) {
                            PullTargets.RemoveAllVacuousTargets();
                        }
                    }
                }
                if (HasSteel) {
                    if (markForPushDown) {
                        if (Keybinds.MultipleMarks()) {
                            if (bullseyeTarget != null) {
                                //if (PushTargets.IsTarget(bullseyeTarget)) {
                                //    // If the bullseye target is already selected, we want to remove targets instead.
                                //    PushTargets.RemoveTargets(newTargetsArea);
                                //    //removingTargets = PushTargets.VacuousCount == 0;
                                //} else {
                                // They're not targets. Add them.
                                // O(n^2)ish
                                for (int i = 0; i < newTargetsArea.Count; i++) {
                                    PushTargets.AddTarget(newTargetsArea[i], false);
                                }
                                //}
                            }
                        } else {
                            //if (PushTargets.IsTarget(bullseyeTarget)) {
                            //    // If the bullseye target is already selected, we want to remove targets instead.
                            //    PushTargets.RemoveTargets(newTargetsArea);
                            //    removingTargets = PushTargets.VacuousCount == 0;
                            //} else {
                            // It's not a target. Remove old targets and add new one.
                            PushTargets.ReplaceContents(newTargetsArea, false);
                            //}
                        }
                    } else if (markForPush) {
                        if (Keybinds.MultipleMarks()) {
                            //if (removingTargets) {
                            //    PushTargets.RemoveTargets(newTargetsArea);
                            //} else {
                            for (int i = 0; i < newTargetsArea.Count; i++) {
                                PushTargets.AddTarget(newTargetsArea[i], false);
                            }
                            //}
                        } else {
                            //if (removingTargets) {
                            //    PushTargets.RemoveTargets(newTargetsArea);
                            //} else {
                            PushTargets.ReplaceContents(newTargetsArea, false);
                            //}
                        }
                    } else {
                        // Not holding down Negate nor Select this round. Consider vacuous Pushing.
                        if (!HasPushTarget) {
                            if (keybindPushingDown) {
                                PushTargets.ReplaceContents(newTargetsArea, true);
                            }
                        } else if (!keybindPushing && PushTargets.VacuousCount > 0) {
                            PushTargets.RemoveAllVacuousTargets();
                        }
                    }
                }
                break;
            case ControlMode.Bubble:
                // The Bubble crosshair also uses the area crosshair settings
                LerpToAreaSize(SelectionBubbleRadius / maxBubbleRadius * maxAreaRadius);
                // If in bubble mode, LMB and RMB will open/close the bubble and Q/E/MB4/MB5 toggle it
                if (BubbleIsOpen) {
                    // Toggle the bubble being persistently open
                    if (markForPullDown) {
                        if (bubbleKeepOpen) {
                            if (BubbleMetalStatus == iron) {
                                bubbleKeepOpen = false;
                            } else {
                                BubbleOpen(iron);
                            }
                        } else {
                            bubbleKeepOpen = true;
                            BubbleOpen(iron);
                        }
                    } else if (markForPushDown) {
                        if (bubbleKeepOpen) {
                            if (BubbleMetalStatus == steel) {
                                bubbleKeepOpen = false;
                            } else {
                                BubbleOpen(steel);
                            }
                        } else {
                            bubbleKeepOpen = true;
                            BubbleOpen(steel);
                        }
                    }
                } else {
                    if (markForPullDown) {
                        BubbleOpen(iron);
                        bubbleKeepOpen = true;
                    } else if (markForPushDown) {
                        BubbleOpen(steel);
                        bubbleKeepOpen = true;
                    }
                }

                if (keybindPulling) {
                    BubbleOpen(iron);
                } else if (keybindPushing) {
                    BubbleOpen(steel);
                } else if (!bubbleKeepOpen) {
                    BubbleClose();
                }
                break;
        }

        // Handle targets when bubble is open, regardless of mode
        if (BubbleIsOpen) {
            // remove out-of-range targets
            BubbleTargets.RemoveAllOutOfBubble(SelectionBubbleRadius, this);
            BubbleTargets.Size = newTargetsBubble.Count;
            // add new in-range targets
            for (int i = 0; i < newTargetsBubble.Count; i++) {
                BubbleTargets.AddTarget(newTargetsBubble[i], false);
            }
        }
    }
    #endregion

    #region burningMetals
    /// <summary>
    /// Starts burning metals, turning on ironsight.
    /// </summary>
    /// <param name="startIron">Start passively burning iron (true) or steel (false)</param>
    /// <returns></returns>
    public override bool StartBurning(bool startIron = true) {
        if (!base.StartBurning(startIron))
            return false;
        GamepadController.Shake(.1f, .1f, .3f);
        ironBurnPercentageLerp = 1;
        steelBurnPercentageLerp = 1;
        bubbleRadiusLerp = SelectionBubbleRadius;
        if (bubbleBurnPercentageLerp < 0.001f)
            bubbleBurnPercentageLerp = 1;

        switch (Mode) {
            case ControlMode.Manual:
                HUD.Crosshair.SetManual();
                break;
            case ControlMode.Coinshot:
                HUD.Crosshair.SetCoinshot();
                break;
            case ControlMode.Area:
                areaRadiusLerp = selectionAreaRadius;
                HUD.Crosshair.SetCircleRadius(areaRadiusLerp);
                HUD.Crosshair.SetArea();
                break;
            case ControlMode.Bubble:
                areaRadiusLerp = SelectionBubbleRadius / maxBubbleRadius * maxAreaRadius;
                HUD.Crosshair.SetCircleRadius(areaRadiusLerp);
                HUD.Crosshair.SetBubble();
                break;
        }
        forceMagnitudeTarget = 600;
        if (SettingsMenu.settingsGraphics.renderblueLines == 1)
            EnableRenderingBlueLines();
        UpdateBlueLines();

        return true;
    }

    /// <summary>
    /// Stops burning iron and steel.
    /// </summary>
    /// <param name="clearTargets">Also remove marked push/pull targets</param>
    public override void StopBurning(bool clearTargets = true) {
        base.StopBurning(clearTargets);
        steelBurnPercentageLerp = 0;
        ironBurnPercentageLerp = 0;
        IronPassiveBurn = 0;
        SteelPassiveBurn = 0;
        forceMagnitudeTarget = 0;
        GamepadController.SetRumble(0, 0);
        GetComponentInChildren<AllomechanicalGlower>().RemoveAllEmissions();
        DisableRenderingBlueLines();

        HUD.Crosshair.SetManual();
        RefreshHUD();
    }
    #endregion

    #region metalLines
    /*
     * STEEL/IRONSIGHT MANAGEMENT: Render blue lines pointing to nearby metals
     * 
     *  - Searches all Magnetics in the scene that are within a range of the player.
     *  - Shows metal lines drawing from them to the player.
     *  - Returns the Magnetics that might be selected, depending on the mode:
     *      Manual/Coinshot: the closest metal to the center of the screen (the bullseye target)
     *      Area: All metals in the circle close to the center of the screen
     *      Bubble: All metals in a sphere around the player
     * 
     * Rules for the metal lines:
     *  - The WIDTH of the line is dependant on the MASS of the target
     *  - The BRIGHTNESS of the line is dependent on the FORCE that would result from the Push
     *  - The "LIGHT SABER" FACTOR (the white core of the line) is dependent on the FORCE actually acting on the target.
     *  
     *  This algorithm could be more efficient.
     */
    /// <summary>
    /// Find nearby metals for targeting and render blue lines pointing to them.
    /// </summary>
    /// <returns>The bullseye target, the list of targets in the area, the list of targets in the bubble</returns>
    private (Magnetic, List<Magnetic>, List<Magnetic>) IronSteelSight() {
        Magnetic targetBullseye = null;
        List<Magnetic> newTargetsArea = new List<Magnetic>();
        List<Magnetic> newTargetsBubble = new List<Magnetic>();

        // To determine the range of detection, find the force that would act on a supermassive metal.
        // That way, we can ignore metals out of that range for efficiency.
        float bigCharge = 5;
        float distanceThresholdSqr;
        switch (SettingsMenu.settingsAllomancy.forceDistanceRelationship) {
            case 0: {
                    distanceThresholdSqr = SettingsMenu.settingsAllomancy.maxPushRange;
                    break;
                }
            case 1: {
                    float lhs = SettingsMenu.settingsAllomancy.metalDetectionThreshold / (SettingsMenu.settingsAllomancy.allomanticConstant * Strength * Charge * bigCharge);
                    if (SettingsMenu.settingsAllomancy.pushControlStyle == 0)
                        lhs /= GreaterPassiveBurn;
                    distanceThresholdSqr = 1 / lhs;
                    break;
                }
            default: {
                    float lhs = SettingsMenu.settingsAllomancy.metalDetectionThreshold / (SettingsMenu.settingsAllomancy.allomanticConstant * Strength * Charge * bigCharge);
                    if (SettingsMenu.settingsAllomancy.pushControlStyle == 0)
                        lhs /= GreaterPassiveBurn;
                    distanceThresholdSqr = -(float)System.Math.Log(lhs) * SettingsMenu.settingsAllomancy.distanceConstant;
                    break;
                }
        }
        distanceThresholdSqr *= distanceThresholdSqr;

        // Do this in a thread


        // Go through every metal in the scene and update the blue lines pointing to them.
        // Add every metal near the center of the screen to the Lists of Magnetics that are in range for Area and Bubble selection.
        float bullseyeWeight = 0; // weight of the Magnetic currently "closest" to the bullseye

        //Action<float, ConcurrentBag<Magnetic>, ConcurrentBag<Magnetic>, Magnetic, Magnetic> action = (float distanceThresholdSqr, ConcurrentBag<Magnetic> newTargetsArea, ConcurrentBag<Magnetic> newTargetsBubble, Magnetic target, Magnetic targetBullseye) => {
        //Action<ConcurrentBag<Magnetic>, ConcurrentBag<Magnetic>, Magnetic, Magnetic> action = (ConcurrentBag<Magnetic> targetsArea, ConcurrentBag<Magnetic> targetsBubble, Magnetic bullseye, Magnetic target) => {

        //};
        int numJobs = GameManager.MagneticsInScene.Count;

        foreach (Magnetic target in GameManager.MagneticsInScene) {

            if (target.isActiveAndEnabled && target != Player.PlayerMagnetic) {
                // skip this target completely if it is too far away
                if ((target.CenterOfMass - transform.position).sqrMagnitude > distanceThresholdSqr) {
                    target.DisableBlueLine();
                } else {
                    float weight = SetLineProperties(target, out float radialDistance, out float linearDistance);
                    // If the Magnetic is on the screen
                    if (weight > 0) {
                        // IF we're not looking directly at a metal's collider
                        // and IF that metal is reasonably close to the center of the screen
                        // and IF the new Magnetic is closer to the center of the screen than the previous most-center Magnetic
                        if (weight > lineWeightThreshold && weight > bullseyeWeight) {
                            bullseyeWeight = weight;
                            targetBullseye = target;
                        }
                        if (radialDistance < selectionAreaRadius) {
                            newTargetsArea.Add(target);
                        }
                    }
                    if (linearDistance < SelectionBubbleRadius) {
                        newTargetsBubble.Add(target);
                    }
                }
            }
        }
        // If the player is directly looking at a magnetic's collider, use that for the bullseye isntead
        if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 500, GameManager.Layer_IgnorePlayer)) {
            Magnetic target = hit.collider.GetComponentInParent<Magnetic>();
            if (target && target.isActiveAndEnabled && target.IsInRange(this, GreaterPassiveBurn)) {
                targetBullseye = target;
                if (!newTargetsArea.Contains(targetBullseye))
                    newTargetsArea.Add(targetBullseye);
            }
        }

        return (targetBullseye, newTargetsArea, newTargetsBubble);
    }

    /*
     * Checks several factors and sets the properties of the blue line pointing to target.
     * These factors are described in the above function.
     * Returns the "weight" of the target, which increases within closeness to the player and the center of the screen.
     */
    /// <summary>
    /// Calculates how good of a target a metal is and assigns its blue line properties.
    /// </summary>
    /// <param name="target">the metal in question</param>
    /// <param name="radialDistance">the distance from the center of the screen to the target</param>
    /// <param name="linearDistance">the distance from the player to the target</param>
    /// <returns>the weight of the target (-1 if invalid, 1 for very valid)</returns>
    private float SetLineProperties(Magnetic target, out float radialDistance, out float linearDistance) {
        Vector3 allomanticForceVector = CalculateAllomanticForce(target);
        float allomanticForce = allomanticForceVector.magnitude;
        // If using Percentage force mode, burn percentage affects your range for burning
        if (SettingsMenu.settingsAllomancy.pushControlStyle == 0)
            allomanticForce *= GreaterPassiveBurn;

        allomanticForce -= SettingsMenu.settingsAllomancy.metalDetectionThreshold; // blue metal lines will fade to a luminocity of 0 when the force is on the edge of the threshold

        if (allomanticForce <= 0) {
            // Magnetic is out of range
            target.DisableBlueLine();
            radialDistance = 1;
            linearDistance = float.PositiveInfinity;
            return -1;
        }
        // Set line properties
        Vector3 screenPosition = CameraController.ActiveCamera.WorldToViewportPoint(target.CenterOfMass);
        // make the center be 0
        screenPosition.x -= .5f;
        screenPosition.y -= .5f;
        // Pretend the screen is a square for radial distance. Scale down X.
        screenPosition.x = screenPosition.x * Screen.width / Screen.height;

        // Calculate the distance from the center for deciding which blue lines are "in-focus"
        // Radial distance: 
        radialDistance = Mathf.Sqrt(
            (screenPosition.x) * (screenPosition.x) +
            (screenPosition.y) * (screenPosition.y)
        );
        linearDistance = (transform.position - target.CenterOfMass).magnitude;

        float weight;
        if (screenPosition.z < 0) { // the target is behind the player, off-screen
            weight = -1;
        } else {
            // Assign weighting due to position
            weight = .1f / radialDistance - linearDistance / 500;
        }

        if (SettingsMenu.settingsGraphics.renderblueLines == 1) {
            float closeness = blueLineBrightnessFactor * Mathf.Pow(allomanticForce, blueLineChangeFactor);
            //if(SettingsMenu.settingsData.cameraFirstPerson == 1) {
            //    closeness *= perspectiveFactor;
            //}

            // If nearly off - screen, make lines dimmer
            if (screenPosition.z < 0) {
                closeness *= targetFocusOffScreenBound;
            }
            target.SetBlueLine(
                CenterOfBlueLines,
                target.Charge * blueLineWidthFactor * (CameraController.IsFirstPerson && GameManager.CameraState == GameManager.GameCameraState.Standard ? firstPersonWidthFactor : 1),
                1,
                closeness
            );
        }

        return weight;
    }

    /// <summary>
    /// Updates the blue lines pointed to metals without expecting
    /// </summary>
    public void UpdateBlueLines() {
        IronSteelSight();
        SetTargetedLineProperties();
    }

    /// <summary>
    /// Makes the lines pointing to marked targets be brighter, special colors, etc.
    /// </summary>
    private void SetTargetedLineProperties() {
        // Regardless of other factors, lines pointing to Push/Pull-targets have unique colors
        // Update metal lines for Pull/PushTargets
        if (PullingOnPullTargets) {
            PullTargets.UpdateBlueLines(iron, IronBurnPercentageTarget, CenterOfBlueLines);
        } else if (PushingOnPullTargets) {
            PullTargets.UpdateBlueLines(iron, SteelBurnPercentageTarget, CenterOfBlueLines);
        } else if (HasPullTarget) {
            PullTargets.UpdateBlueLines(iron, 0, CenterOfBlueLines);
        }
        if (PullingOnPushTargets) {
            PushTargets.UpdateBlueLines(steel, IronBurnPercentageTarget, CenterOfBlueLines);
        } else if (PushingOnPushTargets) {
            PushTargets.UpdateBlueLines(steel, SteelBurnPercentageTarget, CenterOfBlueLines);
        } else if (HasPushTarget) {
            PushTargets.UpdateBlueLines(steel, 0, CenterOfBlueLines);
        }

        BubbleTargets.UpdateBlueLines(BubbleMetalStatus, BubbleBurnPercentageTarget, CenterOfBlueLines);
    }

    public void EnableRenderingBlueLines() {
        if (IsBurning)
            CameraController.ActiveCamera.cullingMask = ~0;
    }

    public void DisableRenderingBlueLines() {
        CameraController.ActiveCamera.cullingMask = ~(1 << blueLineLayer);
    }
    #endregion

    #region controlsManupulation
    /// <summary>
    /// Changes the radius for bubble/area mode.
    /// </summary>
    private void ChangeRadius(float change) {
        if (change == 0)
            return;
        if (Mode == ControlMode.Area) {
            if (change > 0) {
                if (selectionAreaRadius < maxAreaRadius) {
                    selectionAreaRadius += areaRadiusIncrement;
                }
            } else if (selectionAreaRadius > minAreaRadius) {
                selectionAreaRadius -= areaRadiusIncrement;
            }

        } else if (Mode == ControlMode.Bubble) {
            if (change > 0) {
                if (SelectionBubbleRadius < maxBubbleRadius) {
                    SelectionBubbleRadius += bubbleRadiusIncrement;
                }
            } else if (SelectionBubbleRadius > minBubbleRadius) {
                SelectionBubbleRadius -= bubbleRadiusIncrement;
            }
        }
    }

    /// <summary>
    /// Sets the desired force magnitude for when the Push Control Mode is "Magnitude" 
    /// </summary>
    /// <param name="change">the delta by which the force should chagnge</param>
    private void ChangeTargetForceMagnitude(float change) {
        if (SettingsMenu.settingsInterface.forceUnits == 0) {
            if (change > 0) {
                change = -Physics.gravity.y / 10 * Mass;
                if (forceMagnitudeTarget < -Physics.gravity.y) {
                    change /= 10f;
                }
            } else if (change < 0) {
                change = Physics.gravity.y / 10 * Mass;
                if (forceMagnitudeTarget <= -Physics.gravity.y) {
                    change /= 10f;
                }
            }
        } else {
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
        }
        forceMagnitudeTarget = forceMagnitudeTarget + change;
        if (forceMagnitudeTarget <= 0.01f)
            StopBurning();
    }

    /// <summary>
    /// Changes the desired percentage for when the Push Control Mode is "Percent" and Control Mode is not Bubble
    /// </summary>
    /// <param name="change">the delta by which the percent should change</param>
    private void ChangeBurnPercentageTargetManual(float change) {
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
    /// <summary>
    /// Changes the desired percentage for when the Push Control Mode is "Percent" and Control Mode is "Bubble"
    /// </summary>
    /// <param name="change">the delta by which the percent should change</param>
    private void ChangeBurnPercentageTargetBubble(float change) {
        if (change > 0) {
            change = .10f;
            if (bubbleBurnPercentageLerp < .09f) {
                change /= 10f;
            }
        } else if (change < 0) {
            change = -.10f;
            if (bubbleBurnPercentageLerp <= .10f) {
                change /= 10f;
            }
        }
        SetBubblePercentageTarget(Mathf.Clamp(bubbleBurnPercentageLerp + change, 0, 1));
    }

    /// <summary>
    /// Sets the percentage target for Pulling to lerp to
    /// </summary>
    /// <param name="percentage">the desired percent</param>
    private void SetPullPercentageTarget(float percentage) {
        if (percentage > .005f) {
            ironBurnPercentageLerp = Mathf.Min(1, percentage);
        } else {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                ironBurnPercentageLerp = 1;
            else
                ironBurnPercentageLerp = 0;
        }
    }
    /// <summary>
    /// Sets the percentage target for Pushing to lerp to
    /// </summary>
    /// <param name="percentage">the desired percent</param>
    private void SetPushPercentageTarget(float percentage) {
        if (percentage > .005f) {
            steelBurnPercentageLerp = Mathf.Min(1, percentage);
        } else {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                steelBurnPercentageLerp = 1;
            else
                steelBurnPercentageLerp = 0;
        }
    }
    /// <summary>
    /// Sets the percentage target for the Bubble to lerp to
    /// </summary>
    /// <param name="percentage">the desired percent</param>
    private void SetBubblePercentageTarget(float percentage) {
        if (percentage > .005f) {
            bubbleBurnPercentageLerp = Mathf.Min(1, percentage);
        } else {
            if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad)
                bubbleBurnPercentageLerp = 1;
            else
                bubbleBurnPercentageLerp = 0;
        }
    }

    /// <summary>
    /// Lerps the current burn percentages to the desired target percentages.
    /// </summary>
    private void LerpToBurnPercentages() {
        IronBurnPercentageTarget = Mathf.Lerp(IronBurnPercentageTarget, ironBurnPercentageLerp, burnPercentageLerpConstant);
        SteelBurnPercentageTarget = Mathf.Lerp(SteelBurnPercentageTarget, steelBurnPercentageLerp, burnPercentageLerpConstant);
        BubbleBurnPercentageTarget = Mathf.Lerp(BubbleBurnPercentageTarget, bubbleBurnPercentageLerp, burnPercentageLerpConstant);
        if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad || SettingsMenu.settingsAllomancy.pushControlStyle == 1) {
            IronPassiveBurn = 1;
            SteelPassiveBurn = 1;
        } else {
            IronPassiveBurn = IronBurnPercentageTarget;
            SteelPassiveBurn = SteelBurnPercentageTarget;
        }

        // Gamepad rumble
        if (HasPullTarget || HasPushTarget && TimeController.CurrentTimeScale > 0) {
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
        if (SettingsMenu.settingsAllomancy.pushControlStyle == 0 && SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad && (IronBurnPercentageTarget < .001f && SteelBurnPercentageTarget < .001f)) {
            StopBurning();
        }
    }

    /// <summary>
    /// Lerps the current Bubble burn percentage to the desired target percentage.
    /// </summary>
    private void LerpToBubbleSize() {
        if (BubbleIsOpen) {
            float diff = SelectionBubbleRadius - bubbleRadiusLerp;
            if (diff < 0)
                diff = -diff;
            if (diff > .001f) {
                bubbleRadiusLerp = Mathf.Lerp(bubbleRadiusLerp, SelectionBubbleRadius, burnPercentageLerpConstant);
                BubbleSetVisualSize(bubbleRadiusLerp);
            }
        }
    }

    /// <summary>
    /// Lerps the Area radius towards the desired radius
    /// </summary>
    /// <param name="targetRadius">the desired radius</param>
    private void LerpToAreaSize(float targetRadius) {
        float diff = targetRadius - areaRadiusLerp;
        if (diff < 0)
            diff = -diff;
        if (diff > .001f) {
            areaRadiusLerp = Mathf.Lerp(areaRadiusLerp, targetRadius, areaLerpConstant);
            HUD.Crosshair.SetCircleRadius(areaRadiusLerp);
        }
    }

    /// <summary>
    /// Updates the burn rate meter and Bubble edge glow to reflect the current burn percentage
    /// </summary>
    private void UpdateBurnRateMeter() {
        if (IsBurning) {
            if (SettingsMenu.settingsAllomancy.pushControlStyle == 1) // Magnitude
                HUD.BurnPercentageMeter.SetBurnRateMeterForceMagnitude(LastAllomanticForce, LastAnchoredPushBoost, IronBurnPercentageTarget, SteelBurnPercentageTarget, forceMagnitudeTarget);
            else if (SettingsMenu.settingsGameplay.controlScheme == JSONSettings_Gameplay.Gamepad) {
                if (Mode == ControlMode.Bubble) {
                    HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                        BubbleBurnPercentageTarget, BubbleBurnPercentageTarget);
                } else {
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
                }
            } else {
                // keyboard/mouse, just set it normally
                if (Mode == ControlMode.Bubble) {
                    HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                        BubbleBurnPercentageTarget, BubbleBurnPercentageTarget);
                } else {
                    HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                        IronBurnPercentageTarget, SteelBurnPercentageTarget);
                }
            }

            // Set bubble edge glow
            bubbleRenderer.material.SetFloat("_Intensity", bubbleIntensityMin + bubbleIntensityMax * BubbleBurnPercentageTarget);
        } else {
            HUD.BurnPercentageMeter.Clear();
            HUD.Crosshair.Clear();
        }
    }

    /// <summary>
    /// Refreshes all elements of the hud relevent to pushing and pulling
    /// </summary>
    private void RefreshHUD() {
        if (IsBurning) {
            RefreshHUDColorsOnly();
            // number of targets
            switch (Mode) {
                case ControlMode.Area:
                    HUD.BurnPercentageMeter.SetMetalLineCountTextArea(selectionAreaRadius);
                    break;
                case ControlMode.Bubble:
                    HUD.BurnPercentageMeter.SetMetalLineCountTextBubble(SelectionBubbleRadius);
                    break;
            }
        } else {
            HUD.BurnPercentageMeter.Clear();
            HUD.Crosshair.Clear();
        }
        HUD.TargetOverlayController.Clear();
    }

    /// <summary>
    /// Refreshes the colors of the text of target labels and the burn rate meter.
    /// </summary>
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
    #endregion

    #region controlModes
    public void SetControlModeManual() {
        if (Mode == ControlMode.Manual) {
            StartBurning();
            return;
        }
        Mode = ControlMode.Manual;
        PullTargets.Size = TargetArray.smallArrayCapacity;
        PushTargets.Size = TargetArray.smallArrayCapacity;
        HUD.Crosshair.SetManual();
        HUD.BurnPercentageMeter.SetMetalLineCountTextManual();
        StartBurning();
    }
    public void SetControlModeArea() {
        if (Mode == ControlMode.Area) {
            StartBurning();
            return;
        }
        Mode = ControlMode.Area;
        PullTargets.Size = TargetArray.largeArrayCapacity;
        PushTargets.Size = TargetArray.largeArrayCapacity;
        //areaRadiusLerp = 0;
        HUD.Crosshair.SetArea();
        StartBurning();
    }
    public void SetControlModeBubble() {
        if (Mode == ControlMode.Bubble) {
            StartBurning();
            return;
        }
        Mode = ControlMode.Bubble;
        PullTargets.Size = 0;
        PushTargets.Size = 0;
        HUD.Crosshair.SetBubble();
        StartBurning();
    }
    public void SetControlModeCoinshot() {
        if (Mode == ControlMode.Coinshot) {
            StartBurning();
            return;
        }
        Mode = ControlMode.Coinshot;
        PullTargets.Size = TargetArray.smallArrayCapacity;
        PushTargets.Size = TargetArray.smallArrayCapacity;
        HUD.Crosshair.SetCoinshot();
        HUD.BurnPercentageMeter.SetMetalLineCountTextManual();
        StartBurning();
    }
    #endregion

}