using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Environment_Tutorial1 : EnvironmentCinematic {

    EnvironmentalTransitionManager musicManager;

    void Start() {
        musicManager = GetComponentInChildren<EnvironmentalTransitionManager>();

        Player.CanControl = false;
        Player.CanControlMovement = false;
        Player.CanThrowCoins = false;
        Player.PlayerIronSteel.IronReserve.IsEnabled = false;
        Player.PlayerIronSteel.SteelReserve.IsEnabled = false;
        Player.PlayerPewter.PewterReserve.IsEnabled = false;
        Player.CanControlZinc = false;
        HUD.ControlWheelController.SetLockedState(ControlWheelController.LockedState.LockedFully);

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
        }
        CameraController.DisableCinemachineCamera(vcam);
        Player.CanControl = true;
        Player.CanControlMovement = true;
        CameraController.Clear();

        // Look
        yield return new WaitForSeconds(2);
        if (CameraController.HasNotMovedCamera) {
            HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_look);
            while (CameraController.HasNotMovedCamera)
                yield return null;

            HUD.MessageOverlayCinematic.FadeOut();
            yield return new WaitForSeconds(5);
        }

        // Move
        if (Player.PlayerInstance.GetComponent<Rigidbody>().velocity.sqrMagnitude < .25f) {
            HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_move);

            while (Player.PlayerInstance.GetComponent<Rigidbody>().velocity.sqrMagnitude < .25f)
                yield return null;

            HUD.MessageOverlayCinematic.FadeOut();
        }

    }
    protected override IEnumerator Trigger0() {
        HUD.HelpOverlayController.EnableSimple();
        yield break;
    }
    protected override IEnumerator Trigger1() {
        GameManager.ConversationManager.StartConversation("THINK2");
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        Player.PlayerIronSteel.IronReserve.IsEnabled = true;
        HUD.MessageOverlayCinematic.FadeIn(Messages.tutorial_pull);
        while (!Player.PlayerIronSteel.IsBurning) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
        yield return new WaitForSeconds(1);
        HUD.MessageOverlayCinematic.Next();


        while (!Player.PlayerIronSteel.HasPullTarget) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
