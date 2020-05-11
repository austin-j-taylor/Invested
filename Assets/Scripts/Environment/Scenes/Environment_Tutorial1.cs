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
        cameraPositionTarget.position = new Vector3(Mathf.Cos(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp, (1 + Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert)) * Environment_TitleScreen.speedAmp, Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp);
        vcam.transform.position = cameraPositionTarget.position;
        // Make camera look at Player
        vcam.LookAt = Player.PlayerInstance.transform;

        // Handle music
        StartCoroutine(Play_music());

        StartCoroutine(Procedure());
    }

    private IEnumerator Play_music() {
        while (GameManager.AudioManager.SceneTransitionIsPlaying) {
            yield return null;
        }
        GetComponent<AudioSource>().Play();
    }

    private IEnumerator Procedure() {

        yield return null;

        Player.CanControl = true;
        Player.CanControlMovement = true;
        vcam.enabled = false;
        CameraController.UsingCinemachine = false;
        CameraController.Clear();

    }
}
