using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System;
using Unity.Jobs;
using Unity.Collections;

/// <summary>
/// The AllomanticIronSteel specific for the Prima.
/// Controls the blue metal lines that point from the player to nearby metals.
/// Controls the means through which the player selects Pull/PushTargets.
/// </summary>
public class PrimaPullPushController : ActorPullPushController {

    #region constants
    private readonly Vector3 firstPersonCenterOfMassOffset = new Vector3(0, -0.20f, 0);

    // Control Mode constants
    private const float bubbleIntensityMin = 0.25f;
    private const float bubbleIntensityMax = 0.75f;
    // Other Constants
    private const float burnPercentageLerpConstant = .30f;
    private const float areaLerpConstant = .4f;
    private const int blueLineLayer = 10;
    private const float metalLinesLerpConstant = .30f;
    private const int defaultCharge = 1;
    #endregion

    #region props and fields

    #endregion

    #region clearing
    protected override void Awake() {
        base.Awake();
        bubbleRenderer = transform.Find("BubbleRange").GetComponent<Renderer>();
        BubbleTargets = new TargetArray(TargetArray.largeArrayCapacity);
        InitBubble();
    }

    #endregion


    #region update pushing and pulling

    protected override void UpdateBurnRateMeter() {
        base.UpdateBurnRateMeter();

        if (IsBurning) {
            // Set bubble edge glow
            bubbleRenderer.material.SetFloat("_Intensity", bubbleIntensityMin + bubbleIntensityMax * BubbleBurnPercentageTarget);
        }
    }

    protected override void SetTargetedLineProperties() {
        base.SetTargetedLineProperties();

        UpdateBlueLines(BubbleTargets, BubbleMetalStatus, BubbleBurnPercentageTarget);
    }

    #endregion


    #region controls manupulation


    #endregion


    protected override Vector3 CenterOfBlueLine(Vector3 direction) {
        // If the player is in first person view and looking up, move the endpoint of the blue lines so they don't clip into the camera.
        Vector3 offset = Vector3.zero;
        if (CameraController.Pitch < 60 && CameraController.IsFirstPerson && GameManager.CameraState == GameManager.GameCameraState.Standard) {
            offset = direction;
            offset.y = 0;
            offset = offset.normalized;
            if (CameraController.Pitch > 0) {
                offset *= (-1 / 60f * CameraController.Pitch + 1) / 6;
            } else {
                offset *= 1 / 6f;
            }
            offset += firstPersonCenterOfMassOffset;
        }
        return CenterOfMass + offset;
    }
}