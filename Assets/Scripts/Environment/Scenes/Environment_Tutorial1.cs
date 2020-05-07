using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Environment_Tutorial1 : EnvironmentCinematic {

    void Start() {

        Player.CanControl = false;
        Player.CanControlMovement = false;
        Player.PlayerInstance.CoinHand.Pouch.Clear();
        Player.PlayerIronSteel.SteelReserve.IsEnabled = false;
        Player.PlayerPewter.PewterReserve.IsEnabled = false;
        Player.CanControlZinc = false;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedFully);
        HUD.HelpOverlayController.SetLockedState(HelpOverlayController.LockedState.Locked0);

        // Set cinemachine virtual camera properties
        InitializeCinemachine();
        // Set camera target position to be same as where we left of in tutorial (just a sine function of time)
        cameraPositionTarget.position = new Vector3(Mathf.Cos(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp, Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp, Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp);

        vcam.Follow = CameraController.CameraPositionTarget;
        CinemachineSmoothPath path = GetComponent<CinemachineSmoothPath>();
        CinemachineSmoothPath.Waypoint way0, way1;
        way0.position = cameraPositionTarget.position;
        way0.roll = 0;
        way1.position = CameraController.CameraPositionTarget.position;
        way1.roll = 0;
        path.m_Waypoints[0] = way0;
        path.m_Waypoints[1] = way1;
        dolly.m_PathPosition = 0;


        StartCoroutine(Procedure());
    }

    private IEnumerator Procedure() {

        yield return null;

        CinemachineSmoothPath.Waypoint way1;
        way1.roll = 0;
        CinemachineSmoothPath path = GetComponent<CinemachineSmoothPath>();
        dolly.m_PathPosition = dolly.m_Path.MaxPos;

        while (DollyHasntReachedTarget()) {
            way1.position = CameraController.CameraPositionTarget.position;
            path.m_Waypoints[1] = way1;
            yield return null;
        }
        Player.CanControl = true;
        Player.CanControlMovement = true;
        vcam.enabled = false;
        CameraController.UsingCinemachine = false;
        CameraController.Clear();
    }
}
