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

    [SerializeField]
    private PullpushMode combatMode;


    [SerializeField]
    private Transform boneCenterOfMass = null;

    private float timeInActive;

    protected override void Awake() {
        base.Awake();

        combatMode = PullpushMode.Idle;
        timeInActive = 0;
    }

    private void Update() {
        // Transitions
        switch (combatMode) {
            case PullpushMode.Idle:
                if (IsBurning)
                    combatMode = PullpushMode.Burning;
                break;
            case PullpushMode.Burning:
                if (!IsBurning)
                    combatMode = PullpushMode.Idle;
                else if (PushingOrPullingOnTarget)
                    combatMode = PullpushMode.Pullpushing;
                break;
            case PullpushMode.Pullpushing:
                if (!PushingOrPullingOnTarget) {
                    combatMode = PullpushMode.Active;
                    timeInActive = 0;
                }
                break;
            case PullpushMode.Active:
                if (!IsBurning)
                    combatMode = PullpushMode.Idle;
                else if (PushingOrPullingOnTarget)
                    combatMode = PullpushMode.Pullpushing;
                else {
                    timeInActive += Time.deltaTime;
                    if (timeInActive > timeInActiveMax)
                        combatMode = PullpushMode.Burning;
                }
                break;
        }
        // Actions

        switch (combatMode) {
            case PullpushMode.Idle:
                if(Kog.MovementController.Movement.sqrMagnitude > 0) {
                    Kog.KogAnimationController.SetHeadLookAtTarget(Kog.MovementController.Movement.normalized * 10, true);
                    Kog.MovementController.SetBodyLookAtTarget(Kog.MovementController.Movement);
                } else {
                    Kog.KogAnimationController.SetHeadLookAtTarget(transform.forward, true);
                }
                break;
            case PullpushMode.Burning:
                if (Kog.MovementController.Movement.sqrMagnitude > 0) {
                    Kog.MovementController.SetBodyLookAtTarget(Kog.MovementController.Movement);
                }
                Kog.KogAnimationController.SetHeadLookAtTarget(CameraController.ActiveCamera.transform.forward * 10, true);
                break;
            case PullpushMode.Pullpushing:
                Kog.MovementController.SetBodyLookAtTarget(CameraController.ActiveCamera.transform.forward);
                Kog.KogAnimationController.SetHeadLookAtTarget(CameraController.ActiveCamera.transform.forward * 10, true);
                break;
            case PullpushMode.Active:
                Kog.MovementController.SetBodyLookAtTarget(CameraController.ActiveCamera.transform.forward);
                Kog.KogAnimationController.SetHeadLookAtTarget(CameraController.ActiveCamera.transform.forward * 10, true);
                break;
        }

        // Set body look-at target to be
        // in front of movement if not pushpulling

    }

    protected override Vector3 CenterOfBlueLine(Vector3 direction) {
        return boneCenterOfMass.position;
    }
}