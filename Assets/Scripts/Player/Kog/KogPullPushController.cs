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
    #endregion

    public enum PullpushMode { Idle, Burning, Pullpushing, Active, Caught, Throwing }

    public PullpushMode State { get; private set; }
    public Magnetic MainTarget => HasPullTarget ? PullTargets[0] : HasPushTarget ? PushTargets[0] : null;

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
                if (!PushingOrPullingOnTarget) {
                    State_ToActive();
                } else if (Kog.HandController.State == KogHandController.GrabState.Grabbed) {
                    State_ToCaught();
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
                    Kog.HandController.Release();
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
        HUD.Crosshair.Hide();
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
        CustomCenterOfAllomancy = boneHand;
    }
    private void State_ToThrowing() {
        State = PullpushMode.Throwing;
        CustomCenterOfAllomancy = boneHand;
        timeInThrowing = 0;
    }
}