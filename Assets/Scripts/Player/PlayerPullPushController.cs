using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using Unity.Jobs;
using Unity.Collections;

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
    private const float lineWeightThreshold = 1;
    public const float targetFocusHighlitFactor = 3;       // Targets that would be selected have brighter lines
    private const float targetFocusOffScreenBound = .2f;      // Determines the luminosity of blue lines that are off-screen
    private const float firstPersonWidthFactor = .25f;
    public const float blueLineWidthFactor = .02f;
    public const float blueLineChangeFactor = 1 / 4f;
    public const float blueLineBrightnessFactor = .15f;

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

    public enum ControlMode { Manual, Area, Bubble, Coinshot };

    // Button held-down times
    //private float timeToStopBurning = 0;
    //private float timeToSwapBurning = 0;

    public ControlMode Mode { get; private set; }
    private bool removingTargets; // needed for knowing to not mark a recently unmarked target
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

    public override void Clear() {
        StopBurning();
        Strength = 1;
        removingTargets = false;
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
    /*
     * Read inputs for selecting targets.
     * Update burn percentages.
     * Update blue lines pointing from player to metal.
     */
    private void LateUpdate() {
        if (!PauseMenu.IsPaused) {
            if (IsBurning) {
                if (!ExternalControl && Player.CanControl) {
                    // Change Burn Percentage Targets
                    // Check scrollwheel for changing the max number of targets and burn percentage, or DPad if using gamepad
                    float scrollValue = 0;
                    if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad) { // Gamepad
                        scrollValue = Keybinds.DPadYAxis();
                        if (SettingsMenu.settingsData.pushControlStyle == 1) {
                            ChangeTargetForceMagnitude(Keybinds.DPadXAxis());
                        }
                    } else { // Mouse and keyboard
                        if (Keybinds.ControlWheel() && (Mode == ControlMode.Area || Mode == ControlMode.Bubble)) {
                            scrollValue = Keybinds.ScrollWheelAxis();
                        } else {
                            if (SettingsMenu.settingsData.pushControlStyle == 0) {
                                ChangeBurnPercentageTarget(Keybinds.ScrollWheelAxis(), Mode == ControlMode.Bubble);
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

                            float maxNetForce = (LastMaximumNetForce).magnitude;
                            SetPullPercentageTarget(forceMagnitudeTarget / maxNetForce);
                            SetPushPercentageTarget(forceMagnitudeTarget / maxNetForce);
                        } else {
                            SetPullPercentageTarget(0);
                            SetPushPercentageTarget(0);
                        }
                    }

                    LerpToBurnPercentages();
                    LerpToBubbleSize();
                    UpdateBurnRateMeter();

                    // Could have stopped burning above. Check if the Allomancer is still burning.
                    if (IsBurning) {
                        //// Swap pull- and push- targets
                        //if (Keybinds.NegateDown() && timeToSwapBurning > Time.time) {
                        //    // swap bubble, if in that mode
                        //    if (Mode == ControlMode.Bubble && BubbleIsOpen) {
                        //        BubbleOpen(!BubbleMetalStatus);
                        //    }
                        //    // Double-tapped, Swap targets
                        //    PullTargets.SwapContents(PushTargets);
                        //} else {
                        //    if (Keybinds.NegateDown()) {
                        //        timeToSwapBurning = Time.time + timeDoubleTapWindow;
                        //    }
                        //}

                        // Changing status of Pushing and Pulling
                        // Coinshot mode: 
                        // If do not have pull-targets, pulling is also Pushing
                        // Player.cs handles coin throwing
                        bool pulling, pushing;
                        bool keybindPulling = Keybinds.IronPulling();
                        bool keybindPushing = Keybinds.SteelPushing() || Keybinds.WithdrawCoin(); // for Pushing on coins after tossing them
                        bool keybindPullingDown = Keybinds.PullDown();
                        bool keybindPushingDown = Keybinds.PushDown();
                        if (Mode == ControlMode.Coinshot) {
                            pulling = keybindPulling && HasIron && HasPullTarget;
                            pushing = keybindPushing && HasSteel;
                            // if LMB while has a push target, Push and don't Pull.
                            if (keybindPulling && !HasPullTarget) {
                                pushing = true;
                                keybindPushing = true;
                                keybindPushingDown = false;
                                pulling = false;
                                keybindPulling = false;
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

                        // Check input for target selection
                        bool markForPull = Keybinds.Select() && HasIron;
                        bool markForPullDown = Keybinds.SelectDown() && HasIron;
                        bool markForPush = Keybinds.SelectAlternate() && HasSteel;
                        bool markForPushDown = Keybinds.SelectAlternateDown() && HasSteel;
                        //bool removing = Keybinds.Negate();

                        // Search for Metals
                        Magnetic bullseyeTarget;
                        List<Magnetic> newTargetsArea;
                        List<Magnetic> newTargetsBubble;
                        (bullseyeTarget, newTargetsArea, newTargetsBubble) = IronSteelSight();
                        if (Keybinds.SelectUp() || Keybinds.SelectAlternateUp()) {
                            // When you just unmarked a target, you can't mark
                            removingTargets = false;
                        }
                        // Assign targets depending on the control mode.
                        switch (Mode) {
                            case ControlMode.Manual:
                            // fall through, same as coinshot
                            case ControlMode.Coinshot:
                                // set the bullseye target to be brighter
                                if (bullseyeTarget)
                                    bullseyeTarget.BrightenLine(targetFocusHighlitFactor);
                                if (markForPullDown) {
                                    if (Keybinds.MultipleMarks()) {
                                        if (bullseyeTarget != null) {
                                            if (PullTargets.IsTarget(bullseyeTarget)) {
                                                // If the bullseye target is already selected, we want to remove it instead.
                                                RemovePullTarget(bullseyeTarget);
                                                removingTargets = PullTargets.VacuousCount == 0; // start removing targets, unless they were vacuous
                                            } else {
                                                // It's not a target. Add new one.
                                                AddPullTarget(bullseyeTarget);
                                            }
                                        }
                                    } else {
                                        // If there is no bullseye target, deselect all targets.
                                        if (bullseyeTarget == null) {
                                            PullTargets.Clear();
                                        //} else if (PullTargets.IsTarget(bullseyeTarget)) {
                                        //    // If the bullseye target is already selected, we want to remove it instead.
                                        //    RemovePullTarget(bullseyeTarget);
                                        //    removingTargets = PullTargets.VacuousCount == 0;
                                        } else {
                                            // It's not a target. Remove old target and add new one.
                                            PullTargets.Clear();
                                            AddPullTarget(bullseyeTarget);
                                        }
                                    }
                                } else if (markForPull) {
                                    if (bullseyeTarget != null) {
                                        if (PullTargets.IsTarget(bullseyeTarget)) {
                                            if (removingTargets) {
                                                RemovePullTarget(bullseyeTarget);
                                            } else {
                                                if (!Keybinds.MultipleMarks())
                                                    PullTargets.Clear();
                                                AddPullTarget(bullseyeTarget);
                                            }
                                        } else {
                                            if (!removingTargets) {
                                                // It's not a target. Add it. If we're multitargeting, preserve the old target.
                                                if (!Keybinds.MultipleMarks())
                                                    PullTargets.Clear();
                                                AddPullTarget(bullseyeTarget);
                                            }
                                        }
                                    }
                                } else {
                                    // Not holding down Negate nor Select this round. Consider vacuous pulling.
                                    if (!HasPullTarget) {
                                        if (keybindPullingDown) {
                                            AddPullTarget(bullseyeTarget, true, true);
                                        }
                                    } else if (!keybindPulling && PullTargets.VacuousCount > 0) {
                                        // If we were vacuously pulling but we just released the Pull, remove those targets.
                                        PullTargets.RemoveAllVacuousTargets();
                                    }
                                }
                                if (markForPushDown) {
                                    if (Keybinds.MultipleMarks()) {
                                        if (bullseyeTarget != null) {
                                            if (PushTargets.IsTarget(bullseyeTarget)) {
                                                // If the bullseye target is already selected, we want to remove it instead.
                                                RemovePushTarget(bullseyeTarget);
                                                removingTargets = PushTargets.VacuousCount == 0;
                                            } else {
                                                // It's not a target. Add new one.
                                                AddPushTarget(bullseyeTarget);
                                            }
                                        }
                                    } else {
                                        // If there is no bullseye target, deselect all targets.
                                        if (bullseyeTarget == null) {
                                            PushTargets.Clear();
                                        //} else if (PushTargets.IsTarget(bullseyeTarget)) {
                                        //    // If the bullseye target is already selected, we want to remove it instead.
                                        //    RemovePushTarget(bullseyeTarget);
                                        //    removingTargets = PushTargets.VacuousCount == 0;
                                        } else {
                                            // It's not a target. Remove old target and add new one.
                                            PushTargets.Clear();
                                            AddPushTarget(bullseyeTarget);
                                        }
                                    }
                                } else if (markForPush) {
                                    if (bullseyeTarget != null) {
                                        if (PushTargets.IsTarget(bullseyeTarget)) {
                                            if (removingTargets) {
                                                RemovePushTarget(bullseyeTarget);
                                            } else {
                                                if (!Keybinds.MultipleMarks())
                                                    PushTargets.Clear();
                                                AddPushTarget(bullseyeTarget);
                                            }
                                        } else {
                                            if (!removingTargets) {
                                                // It's not a target. Add it. If we're multitargeting, preserve the old target.
                                                if (!Keybinds.MultipleMarks())
                                                    PushTargets.Clear();
                                                AddPushTarget(bullseyeTarget);
                                            }
                                        }
                                    }
                                } else {
                                    // Not holding down Negate nor markForPush this round. Consider vacuous pulling.
                                    if (!HasPushTarget) {
                                        if (keybindPushingDown) {
                                            AddPushTarget(bullseyeTarget, true, true);
                                        }
                                    } else if (!keybindPushing && PushTargets.VacuousCount > 0) {
                                        PushTargets.RemoveAllVacuousTargets();
                                    }
                                }
                                //}
                                break;
                            case ControlMode.Area:
                                // Crosshair: lerp to the correct size
                                LerpToAreaSize();
                                // set the targets in the area to be brighter
                                for (int i = 0; i < newTargetsArea.Count; i++) {
                                    newTargetsArea[i].BrightenLine(targetFocusHighlitFactor);
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
                        //timeToStopBurning = 0;
                    }
                    //else
                    //if (Keybinds.Negate()) {
                    //    timeToStopBurning += Time.deltaTime;
                    //    if (Keybinds.Select() && Keybinds.SelectAlternate() && timeToStopBurning > timeToHoldDown) {
                    //        //if (Keybinds.IronPulling() && Keybinds.SteelPushing() && timeToStopBurning > timeToHoldDown) {
                    //        StopBurning();
                    //        timeToStopBurning = 0;
                    //    }
                    //} else {
                    //    timeToStopBurning = 0;
                    //}
                } else {
                    // Start burning (as long as the Control Wheel isn't open to interfere)
                    if (Keybinds.StopBurning()) {
                        // toggle back on
                        StartBurning();
                    }
                    //else if (!Keybinds.Negate() && !HUD.ControlWheelController.IsOpen) {
                    else if (!HUD.ControlWheelController.IsOpen) {
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

    public override bool StartBurning(bool startIron = true) {
        if (!base.StartBurning(startIron))
            return false;
        GamepadController.Shake(.1f, .1f, .3f);
        ironBurnPercentageLerp = 1;
        steelBurnPercentageLerp = 1;
        bubbleRadiusLerp = SelectionBubbleRadius;
        if (bubbleBurnPercentageLerp < 0.001f)
            bubbleBurnPercentageLerp = 1;

        areaRadiusLerp = 0;
        if (Mode == ControlMode.Area) {
            HUD.Crosshair.SetArea();
        } else {
            HUD.Crosshair.SetManual();
        }

        forceMagnitudeTarget = 600;
        if (SettingsMenu.settingsData.renderblueLines == 1)
            EnableRenderingBlueLines();
        UpdateBlueLines();

        return true;
    }

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
    /*
     * STEEL/IRONSIGHT MANAGEMENT: blue lines pointing to nearby metals
     * 
     * Searches all Magnetics in the scene for those that are within detection range of the player.
     * Shows metal lines drawing from them to the player.
     * Outputs the Magnetics that should be selected. Depending on the Control Mode, this will be:
     *  - Manual/Coinshot: the "closest" metal to the center of the screen
     *  - Area: All metals in the circle close to the center of the screen
     *  - Bubble: All metals in a sphere around the player
     * 
     * Rules for the metal lines:
     *  - The WIDTH of the line is dependant on the MASS of the target
     *  - The BRIGHTNESS of the line is dependent on the FORCE that would result from the Push
     *  - The "LIGHT SABER" FACTOR is dependent on the FORCE acting on the target. If the metal is not a target, it is 1 (no light saber factor).
     *  
     *  This algorithm (specifically, setting the properties for the blue lines) gobbles up CPU usage. Once you have more than ~300 objects in range,
     *      each of which performs an Allomantic Force calculation, you are guaranteed to lose frames.
     */
    private (Magnetic, List<Magnetic>, List<Magnetic>) IronSteelSight() {
        Magnetic targetBullseye = null;
        bool centerWeightCheck = true;
        List<Magnetic> newTargetsArea = new List<Magnetic>();
        List<Magnetic> newTargetsBubble = new List<Magnetic>();

        // If the player is directly looking at a magnetic's collider, use that for the bullseye
        if (Physics.Raycast(CameraController.ActiveCamera.transform.position, CameraController.ActiveCamera.transform.forward, out RaycastHit hit, 500, GameManager.Layer_IgnorePlayer)) {
            Magnetic target = hit.collider.GetComponentInParent<Magnetic>();
            if (target && target.IsInRange(this, GreaterPassiveBurn)) {
                targetBullseye = target;
                centerWeightCheck = false;
            }
        }

        // To determine the range of detection, find the force that would act on a supermassive metal.
        // That way, we can ignore metals out of that range for efficiency.
        float bigCharge = 5;
        float distanceThresholdSqr;
        switch (SettingsMenu.settingsData.forceDistanceRelationship) {
            case 0: {
                    distanceThresholdSqr = SettingsMenu.settingsData.maxPushRange;
                    break;
                }
            case 1: {
                    float lhs = SettingsMenu.settingsData.metalDetectionThreshold / (SettingsMenu.settingsData.allomanticConstant * Strength * Charge * bigCharge);
                    if (SettingsMenu.settingsData.pushControlStyle == 0)
                        lhs /= GreaterPassiveBurn;
                    distanceThresholdSqr = 1 / lhs;
                    break;
                }
            default: {
                    float lhs = SettingsMenu.settingsData.metalDetectionThreshold / (SettingsMenu.settingsData.allomanticConstant * Strength * Charge * bigCharge);
                    if (SettingsMenu.settingsData.pushControlStyle == 0)
                        lhs /= GreaterPassiveBurn;
                    distanceThresholdSqr = -(float)System.Math.Log(lhs) * SettingsMenu.settingsData.distanceConstant;
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

            if (target != Player.PlayerMagnetic) {
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
                        if (centerWeightCheck && weight > lineWeightThreshold && weight > bullseyeWeight) {
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

        return (targetBullseye, newTargetsArea, newTargetsBubble);
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

        if (SettingsMenu.settingsData.renderblueLines == 1) {
            float closeness = blueLineBrightnessFactor * Mathf.Pow(allomanticForce, blueLineChangeFactor);
            //if(SettingsMenu.settingsData.cameraFirstPerson == 1) {
            //    closeness *= perspectiveFactor;
            //}

            // If nearly off - screen, make lines dimmer
            if (screenPosition.z < 0) {
                closeness *= targetFocusOffScreenBound;
            }
            target.SetBlueLine(
                CenterOfMass,
                target.Charge * blueLineWidthFactor * (SettingsMenu.settingsData.cameraFirstPerson == 1 ? firstPersonWidthFactor : 1),
                1,
                closeness
            );
        }

        return weight;
    }
    public void UpdateBlueLines() {
        IronSteelSight();
        SetTargetedLineProperties();
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

        BubbleTargets.UpdateBlueLines(BubbleMetalStatus, BubbleBurnPercentageTarget, CenterOfMass);
    }

    public void RemoveAllTargets() {
        PullTargets.Clear();
        PushTargets.Clear();

        HUD.TargetOverlayController.Clear();
    }

    private void IncrementTargets() {
        switch (Mode) {
            case ControlMode.Area:
                if (selectionAreaRadius < maxAreaRadius) {
                    selectionAreaRadius += areaRadiusIncrement;
                }
                break;
            case ControlMode.Bubble:
                if (SelectionBubbleRadius < maxBubbleRadius) {
                    SelectionBubbleRadius += bubbleRadiusIncrement;
                }
                break;
        }
    }

    private void DecrementTargets() {
        switch (Mode) {
            case ControlMode.Area:
                if (selectionAreaRadius > minAreaRadius) {
                    selectionAreaRadius -= areaRadiusIncrement;
                }
                break;
            case ControlMode.Bubble:
                if (SelectionBubbleRadius > minBubbleRadius) {
                    SelectionBubbleRadius -= bubbleRadiusIncrement;
                }
                break;
        }
    }

    private void ChangeTargetForceMagnitude(float change) {
        if (SettingsMenu.settingsData.forceUnits == 0) {
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

    // Increments or decrements the current burn percentage
    private void ChangeBurnPercentageTarget(float change, bool percentageForBubble = false) {
        if (percentageForBubble) {
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
        } else {
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
    // Sets the lerp goal for the bubble
    private void SetBubblePercentageTarget(float percentage) {
        if (percentage > .005f) {
            bubbleBurnPercentageLerp = Mathf.Min(1, percentage);
        } else {
            if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad)
                bubbleBurnPercentageLerp = 1;
            else
                bubbleBurnPercentageLerp = 0;
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
        BubbleBurnPercentageTarget = Mathf.Lerp(BubbleBurnPercentageTarget, bubbleBurnPercentageLerp, burnPercentageLerpConstant);
        if (SettingsMenu.settingsData.controlScheme == SettingsData.Gamepad || SettingsMenu.settingsData.pushControlStyle == 1) {
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

    // Smoothly changes the bubble renderer's scale to fit the target radius
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
    private void LerpToAreaSize() {
        float diff = selectionAreaRadius - areaRadiusLerp;
        if (diff < 0)
            diff = -diff;
        if (diff > .001f) {
            areaRadiusLerp = Mathf.Lerp(areaRadiusLerp, selectionAreaRadius, areaLerpConstant);
            HUD.Crosshair.SetCircleRadius(areaRadiusLerp);
        }
    }

    // Updates the burn rate meter with the current burn rates.
    // Also updates the bubble's edge intensity wit the bubble burn rate.
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
                // keyboard/mouse, just set it normally
                if (Mode == ControlMode.Bubble) {
                    HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                        BubbleBurnPercentageTarget, BubbleBurnPercentageTarget);
                } else {
                    HUD.BurnPercentageMeter.SetBurnRateMeterPercentage(LastAllomanticForce, LastAnchoredPushBoost,
                        IronBurnPercentageTarget, SteelBurnPercentageTarget);
                }
            }

            // bubble
            bubbleRenderer.material.SetFloat("_Intensity", bubbleIntensityMin + bubbleIntensityMax * BubbleBurnPercentageTarget);
        } else {
            HUD.BurnPercentageMeter.Clear();
            HUD.Crosshair.Clear();
        }
    }


    // Refreshes all elements of the hud relevent to pushing and pulling
    private void RefreshHUD() {
        if (IsBurning) {
            RefreshHUDColorsOnly();
            HUD.TargetOverlayController.HardRefresh();
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
        PullTargets.Size = TargetArray.smallArrayCapacity;
        PushTargets.Size = TargetArray.smallArrayCapacity;
        HUD.Crosshair.SetManual();
        HUD.BurnPercentageMeter.SetMetalLineCountTextManual();
    }
    public void SetControlModeArea() {
        Mode = ControlMode.Area;
        PullTargets.Size = TargetArray.largeArrayCapacity;
        PushTargets.Size = TargetArray.largeArrayCapacity;
        areaRadiusLerp = 0;
        HUD.Crosshair.SetArea();
    }
    public void SetControlModeBubble() {
        Mode = ControlMode.Bubble;
        PullTargets.Size = 0;
        PushTargets.Size = 0;
        HUD.Crosshair.SetManual();
    }
    public void SetControlModeCoinshot() {
        Mode = ControlMode.Coinshot;
        PullTargets.Size = TargetArray.smallArrayCapacity;
        PushTargets.Size = TargetArray.smallArrayCapacity;
        HUD.Crosshair.SetManual();
        HUD.BurnPercentageMeter.SetMetalLineCountTextManual();
    }
}