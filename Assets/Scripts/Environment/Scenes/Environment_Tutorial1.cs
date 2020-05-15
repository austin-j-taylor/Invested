using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;

public class Environment_Tutorial1 : EnvironmentCinematic {

    [SerializeField]
    private MessageTrigger trigger_pull;

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
        vcam.transform.position = new Vector3(Mathf.Cos(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp, (1 + Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert)) * Environment_TitleScreen.speedAmp, Mathf.Sin(Time.unscaledTime * Environment_TitleScreen.speedVert) * Environment_TitleScreen.speedAmp);
        // Make camera look at Player
        vcam.LookAt = Player.PlayerInstance.transform;

        trigger_pull.routine = Trigger_Pull();

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
        vcam.enabled = false;
        yield return new WaitForSeconds(2);
        CameraController.UsingCinemachine = false;
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
    private IEnumerator Trigger_Pull() {
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
