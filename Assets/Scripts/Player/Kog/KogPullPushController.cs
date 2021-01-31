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
    #endregion

    public enum PullpushMode { Idle, Burning, Pullpushing, Active }

    public PullpushMode State { get; private set; }
    public Magnetic MainTarget => HasPullTarget ? PullTargets[0] : HasPushTarget ? PushTargets[0] : null;
    
    [SerializeField]
    private Transform boneCenterOfMass = null;

    private float timeInActive;

    protected override void Awake() {
        base.Awake();

        State = PullpushMode.Idle;
        timeInActive = 0;
    }

    private void Update() {
        // Transitions
        switch (State) {
            case PullpushMode.Idle:
                if (IsBurning)
                    State = PullpushMode.Burning;
                break;
            case PullpushMode.Burning:
                if (!IsBurning)
                    State = PullpushMode.Idle;
                else if (PushingOrPullingOnTarget)
                    State = PullpushMode.Pullpushing;
                break;
            case PullpushMode.Pullpushing:
                if (!PushingOrPullingOnTarget) {
                    State = PullpushMode.Active;
                    timeInActive = 0;
                }
                break;
            case PullpushMode.Active:
                if (!IsBurning)
                    State = PullpushMode.Idle;
                else if (PushingOrPullingOnTarget)
                    State = PullpushMode.Pullpushing;
                else {
                    timeInActive += Time.deltaTime;
                    if (timeInActive > timeInActiveMax)
                        State = PullpushMode.Burning;
                }
                break;
        }
        // Actions

        switch (State) {
            case PullpushMode.Idle:
                if(Kog.MovementController.Movement.sqrMagnitude > 0) {
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

    protected override Vector3 CenterOfBlueLine(Vector3 direction) {
        return boneCenterOfMass.position;
    }
}