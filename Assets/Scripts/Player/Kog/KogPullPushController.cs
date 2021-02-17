using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using Unity.Jobs;
using Unity.Collections;

/// <summary>
/// The AllomanticIronSteel specific for Kog.
/// Controls the blue metal lines that point from the player to nearby metals.
/// Controls the means through which the player selects Pull/PushTargets.
/// </summary>
public class KogPullPushController : ActorPullPushController {

    #region constants
    private const float timeInActiveMax = 3;
    [SerializeField]
    private float timeInThrowingMax = 0.45f;
    private const float kogAllomanticStrength = 2;
    private const float maxSelectionRadius = 0.5f;
    #endregion

    public enum PullpushMode { Idle, Burning, Pullpushing, Active, Caught, Throwing }

    public PullpushMode State { get; private set; }
    public Magnetic MainTarget => HasPullTarget ? PullTargets[0] : HasPushTarget ? PushTargets[0] : null;
    public Magnetic lastBullseyeTarget;

    [SerializeField]
    private Transform boneCenterOfMass = null;
    [SerializeField]
    private Transform boneHand = null;

    private float timeInActive, timeInThrowing;

    protected override void Awake() {
        base.Awake();

        CustomCenterOfAllomancy = boneCenterOfMass;
        BaseStrength = kogAllomanticStrength;
        timeInActive = 0;
        timeInThrowing = 0;
        lastBullseyeTarget = null;
    }
    private void Start() {
        State_ToIdle();
    }

    protected override void Update() {
        base.Update();
        // Transitions
        switch (State) {
            case PullpushMode.Idle:
                if (IsBurning)
                    State_ToBurning();
                break;
            case PullpushMode.Burning:
                if (!IsBurning)
                    State_ToIdle();
                else if (PushingOrPullingOnTarget)
                    State_ToPullpushing();
                break;
            case PullpushMode.Pullpushing:
                if (Kog.HandController.State == KogHandController.GrabState.Grabbed) {
                    State_ToCaught();
                } else if (!PushingOrPullingOnTarget) {
                    State_ToActive();
                }
                break;
            case PullpushMode.Active:
                if (!IsBurning)
                    State_ToIdle();
                else if (PushingOrPullingOnTarget)
                    State_ToPullpushing();
                else {
                    timeInActive += Time.deltaTime;
                    if (timeInActive > timeInActiveMax)
                        State_ToBurning();
                }
                break;
            case PullpushMode.Caught:
                if (!IsBurning) {
                    Kog.HandController.Drop();
                    State_ToIdle();
                } else if (SteelPushing) {
                    PushTargets.Clear();
                    State_ToThrowing();
                }
                break;
            case PullpushMode.Throwing:
                if (!IsBurning) {
                    Kog.HandController.Release();
                    State_ToIdle();
                } else {
                    timeInThrowing += Time.deltaTime;
                    if (timeInThrowing > timeInThrowingMax) {
                        PushTargets.Clear();
                        PushTargets.AddTarget(Kog.HandController.Release(), true);
                        State_ToPullpushing();
                    }
                }
                break;
        }
        // Actions

        switch (State) {
            case PullpushMode.Idle:
                CustomCenterOfAllomancy = boneCenterOfMass;
                if (Kog.MovementController.Movement.sqrMagnitude > 0) {
                    Kog.KogAnimationController.SetHeadLookAtTarget(Kog.MovementController.Movement.normalized * 10, true);
                    Kog.MovementController.SetBodyLookAtDirection(Kog.MovementController.Movement);
                } else {
                    Kog.KogAnimationController.SetHeadLookAtTarget(transform.forward, true);
                }
                break;
            case PullpushMode.Burning:
                if (MainTarget != null) {
                    Kog.KogAnimationController.SetHeadLookAtTarget(MainTarget.transform.position, false);
                    Kog.MovementController.SetBodyLookAtPosition(MainTarget.transform.position);
                } else {
                    Kog.KogAnimationController.SetHeadLookAtTarget(CameraController.ActiveCamera.transform.forward * 10, true);
                    if (Kog.MovementController.Movement.sqrMagnitude > 0) {
                        Kog.MovementController.SetBodyLookAtDirection(Kog.MovementController.Movement);
                    }
                }
                break;
            case PullpushMode.Pullpushing:
                Kog.KogAnimationController.SetHeadLookAtTarget(MainTarget.transform.position, false);
                Kog.MovementController.SetBodyLookAtPosition(MainTarget.transform.position);
                break;
            case PullpushMode.Active:
                if (MainTarget != null) {
                    Kog.KogAnimationController.SetHeadLookAtTarget(MainTarget.transform.position, false);
                    Kog.MovementController.SetBodyLookAtPosition(MainTarget.transform.position);
                } else {
                    Kog.KogAnimationController.SetHeadLookAtTarget(CameraController.ActiveCamera.transform.forward * 10, true);
                    Kog.MovementController.SetBodyLookAtDirection(CameraController.ActiveCamera.transform.forward);
                }
                break;
            case PullpushMode.Caught:
                Kog.KogAnimationController.SetHeadLookAtTarget(CameraController.ActiveCamera.transform.forward * 10, true);
                Kog.MovementController.SetBodyLookAtDirection(CameraController.ActiveCamera.transform.forward);
                break;
            case PullpushMode.Throwing:
                Kog.KogAnimationController.SetHeadLookAtTarget(CameraController.ActiveCamera.transform.forward * 10, true);
                Kog.MovementController.SetBodyLookAtDirection(CameraController.ActiveCamera.transform.forward);
                break;
        }

        // Set body look-at target to be
        // in front of movement if not pushpulling

    }
    private void State_ToIdle() {
        State = PullpushMode.Idle;
        CustomCenterOfAllomancy = boneCenterOfMass;
    }
    private void State_ToBurning() {
        CustomCenterOfAllomancy = boneCenterOfMass;
        State = PullpushMode.Burning;
    }
    private void State_ToPullpushing() {
        State = PullpushMode.Pullpushing;
        CustomCenterOfAllomancy = boneHand;
    }
    private void State_ToActive() {
        State = PullpushMode.Active;
        CustomCenterOfAllomancy = boneHand;
        timeInActive = 0;
    }
    private void State_ToCaught() {
        State = PullpushMode.Caught;
        PullTargets.Clear();
        CustomCenterOfAllomancy = boneHand;
    }
    private void State_ToThrowing() {
        State = PullpushMode.Throwing;
        CustomCenterOfAllomancy = boneHand;
        timeInThrowing = 0;
    }


    // Modified child functions

    /// <summary>
    /// Updates the status of Pushing and Pulling from player input.
    /// </summary>
    protected override void UpdatePushingPulling() {
        bool pulling, pushing;
        bool keybindPulling = Keybinds.IronPulling();
        bool keybindPushing = Keybinds.SteelPushing();
        bool keybindPullingDown = Keybinds.PullDown();
        bool keybindPushingDown = Keybinds.PushDown();

        pulling = keybindPulling && HasIron;
        pushing = keybindPushing && HasSteel;

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
    protected override void UpdateTargetSelection(bool keybindPullingDown, bool keybindPulling, bool keybindPushingDown, bool keybindPushing) {

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

        // Highlight/unhighlight the centermost target
        if (lastBullseyeTarget != null) {
            if(PushingOrPullingOnTarget || bullseyeTarget == null) {
                lastBullseyeTarget.IsHighlighted = false;
                lastBullseyeTarget = null;
            } else if(lastBullseyeTarget != bullseyeTarget) {
                lastBullseyeTarget.IsHighlighted = false;
                bullseyeTarget.IsHighlighted = true;
                lastBullseyeTarget = bullseyeTarget;
            }
        } else {
            if(!PushingOrPullingOnTarget) {
                if(bullseyeTarget != null) {
                    bullseyeTarget.IsHighlighted = true;
                    lastBullseyeTarget = bullseyeTarget;
                }
            }
        }

        if (Keybinds.SelectUp() || Keybinds.SelectAlternateUp()) {
            // No longer need to worry about remarking what we just marked
            removedTarget = null;
        }
        // Assign targets depending on the control mode.
        switch (Mode) {
            case ControlMode.Manual:
                // set the bullseye target to be brighter
                if (bullseyeTarget)
                    bullseyeTarget.BrightenLine();
                if (markForPullDown) {
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
                } else if (markForPull) {
                    if (bullseyeTarget != null) {
                        if (PullTargets.IsTarget(bullseyeTarget)) {
                            PullTargets.Clear();
                            if (removedTarget != bullseyeTarget) {
                                AddPullTarget(bullseyeTarget);
                                removedTarget = null;
                            }
                        } else {
                            if (removedTarget != bullseyeTarget) {
                                // It's not a target. Add it. If we're multitargeting, preserve the old target.
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
                } else if (markForPush) {
                    if (bullseyeTarget != null) {
                        if (PushTargets.IsTarget(bullseyeTarget)) {
                            PushTargets.Clear();
                            if (removedTarget != bullseyeTarget) {
                                AddPushTarget(bullseyeTarget);
                                removedTarget = null;
                            }
                        } else {
                            if (removedTarget != bullseyeTarget) {
                                // It's not a target. Add it. If we're multitargeting, preserve the old target.
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
        }
    }

    /// <summary>
    /// Find nearby metals for targeting and render blue lines pointing to them.
    /// </summary>
    /// <returns>The radially closest valid target, null, null</returns>
    protected override (Magnetic, List<Magnetic>, List<Magnetic>) IronSteelSight() {
        Magnetic closestTarget = null;
        float closestTargetRadius = float.PositiveInfinity; // weight of the Magnetic currently "closest" to the bullseye

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

        foreach (Magnetic target in GameManager.MagneticsInScene) {

            if (target.isActiveAndEnabled) {
                // skip this target completely if it is too far away
                if ((target.CenterOfMass - transform.position).sqrMagnitude > distanceThresholdSqr) {
                    target.DisableBlueLine();
                } else {
                    float weight = SetLineProperties(target, out float radialDistance, out float linearDistance);
                    // If the Magnetic is on the screen
                    if (weight > 0 &&
                                radialDistance < maxSelectionRadius &&
                                radialDistance < closestTargetRadius) {
                        closestTarget = target;
                        closestTargetRadius = radialDistance;
                    }
                }
            }
        }

        return (closestTarget, null, null);
    }
}