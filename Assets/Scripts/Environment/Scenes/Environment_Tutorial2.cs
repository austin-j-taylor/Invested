using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextCodes;

public class Environment_Tutorial2 : EnvironmentCinematic {

    [SerializeField]
    private Node doorNode = null;
    [SerializeField]
    private GameObject backWall = null;
    [SerializeField]
    private FacilityDoor_Red door = null;

    EnvironmentalTransitionManager musicManager;

    void Start() {
        musicManager = GetComponentInChildren<EnvironmentalTransitionManager>();

        Player.CanControl = false;
        Player.CanControlMovement = false;
        Player.CanThrowCoins = false;
        Player.PlayerIronSteel.SteelReserve.IsEnabled = false;
        Player.PlayerPewter.PewterReserve.IsEnabled = false;
        Player.CanControlZinc = false;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedToArea);

        // Set cinemachine virtual camera properties
        InitializeCinemachine();
        // Set camera target position to be same as where we left of in tutorial (just a sine function of time)
        vcam.transform.position = new Vector3(Mathf.Cos(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp, (1 + Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert)) * Environment_TitleScreen.speedAmp, Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp);
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
        musicManager.StartExterior();
    }

    private IEnumerator Procedure() {
        // Skip the intro cutscene if the player is starting elsewhere in the level
        if ((Player.PlayerInstance.transform.position - vcam.transform.position).magnitude < 30) {
            yield return null;
            vcam.enabled = false;
            yield return new WaitForSeconds(2);
        } else {
            Player.PlayerIronSteel.SteelReserve.IsEnabled = true;
        }
        CameraController.DisableCinemachineCamera(vcam);
        Player.CanControl = true;
        Player.CanControlMovement = true;
        CameraController.Clear();

        backWall.SetActive(false);
    }

    protected override IEnumerator Trigger0() {
        backWall.SetActive(true);
        Player.PlayerIronSteel.SteelReserve.IsEnabled = true;

        HUD.MessageOverlayCinematic.FadeIn(s_Hold_ + _KeyPush + " to " + Push + ".");

        while (!Player.PlayerIronSteel.HasPushTarget) {
            yield return null;
        }

        HUD.MessageOverlayCinematic.FadeOutInto("Hit the target on the ceiling.");
        while (!doorNode.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }

    protected override IEnumerator Trigger1() {
        HUD.MessageOverlayCinematic.FadeIn("Like with " + Pulling + ", you can " + Mark_pushing + " metals for " + Pushing + " with " + _KeySelectAlternate + ".\n Mark multiple by holding " + KeyMultiMark + ".");
        while (!door.On)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
