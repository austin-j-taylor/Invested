using UnityEngine;
using System.Collections;
using Cinemachine;

public class EnvironmentCinematic : Environment {

    private const float closenessThresholdSqr = .001f;

    protected CinemachineVirtualCamera vcam;
    protected CinemachineTrackedDolly dolly;
    protected Transform cameraPositionTarget;

    protected void InitializeCinemachine() {
        cameraPositionTarget = transform.Find("cameraTarget");
        vcam = GetComponentInChildren<CinemachineVirtualCamera>();
        vcam.LookAt = CameraController.CameraLookAtTarget;
        vcam.Follow = cameraPositionTarget;
        dolly = vcam.GetCinemachineComponent<CinemachineTrackedDolly>();
        CameraController.SetCinemachineCamera(vcam);
    }

    // For use in Coroutines. Returns true while the dolly is still moving towards the follow target.
    protected bool DollyHasntReachedTarget() {
        return (CameraController.ActiveCamera.transform.position - vcam.Follow.position).sqrMagnitude > closenessThresholdSqr;
    }
}
