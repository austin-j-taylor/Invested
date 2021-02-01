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
    private const float kogAllomanticStrength = 2;
    #endregion

    public enum PullpushMode { Idle, Burning, Pullpushing, Active }

    public PullpushMode State { get; private set; }
    public Magnetic MainTarget => HasPullTarget ? PullTargets[0] : HasPushTarget ? PushTargets[0] : null;
    
    [SerializeField]
    private Transform boneCenterOfMass = null;
    [SerializeField]
    private Transform boneHand = null;

    private float timeInActive;

    protected override void Awake() {
        base.Awake();

        CustomCenterOfAllomancy = boneCenterOfMass;
        BaseStrength = kogAllomanticStrength;
        timeInActive = 0;
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
}