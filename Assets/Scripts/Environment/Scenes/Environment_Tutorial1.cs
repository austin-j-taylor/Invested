using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;
using static TextCodes;

public class Environment_Tutorial1 : EnvironmentCinematic {

    EnvironmentalTransitionManager musicManager;

    private bool chattering = true;

    void Start() {
        musicManager = GetComponentInChildren<EnvironmentalTransitionManager>();

        Player.CanControl = false;
        Player.CanControlMovement = false;
        if(!FlagsController.GetData("pwr_steel"))
            Player.PlayerIronSteel.IronReserve.IsEnabled = false; // Skip restricting iron if they've already got steel

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
            Player.PlayerIronSteel.IronReserve.IsEnabled = true;
        }
        CameraController.DisableCinemachineCamera(vcam);
        Player.CanControl = true;
        Player.CanControlMovement = true;
        CameraController.Clear();

        // Look
        yield return new WaitForSeconds(2);
        if (CameraController.HasNotMovedCamera) {
            HUD.MessageOverlayCinematic.FadeIn(HowToLook + " to look around.");
            while (CameraController.HasNotMovedCamera)
                yield return null;

            HUD.MessageOverlayCinematic.FadeOut();
            yield return new WaitForSeconds(5);
        }

        // Move
        if (Player.PlayerInstance.GetComponent<Rigidbody>().velocity.sqrMagnitude < .25f) {
            HUD.MessageOverlayCinematic.FadeIn(HowToMove + " to move.");

            while (Player.PlayerInstance.GetComponent<Rigidbody>().velocity.sqrMagnitude < .25f)
                yield return null;

            HUD.MessageOverlayCinematic.FadeOut();
        }

    }
    protected override IEnumerator Trigger0() {
        HUD.HelpOverlayController.SetState(1);
        yield break;
    }
    protected override IEnumerator Trigger1() {
        GameManager.ConversationManager.StartConversation("THINK2");
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        Player.PlayerIronSteel.IronReserve.IsEnabled = true;
        HUD.MessageOverlayCinematic.FadeIn(HowToStartBurningIron + " to start burning " + Iron + ".");
        while (!Player.PlayerIronSteel.IsBurning) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOutInto("Look at a " + LightBlue("blue line") + " and " + HowToPull + " to " + Pull + ".");

        while (!Player.PlayerIronSteel.HasPullTarget) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
        yield return new WaitForSeconds(10);
        FlagsController.SetFlag("pwr_zinc");
        HUD.MessageOverlayCinematic.FadeIn(HowToZincTime + " to tap " + Zinc + ".");
        while (!Player.PlayerZinc.InZincTime) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOutInto("The " + ZincBlue("zinc bank") + " slows time when tapped, and recharges when not in use.");
        yield return new WaitForSecondsRealtime(8);
        HUD.MessageOverlayCinematic.FadeOut();

        yield return new WaitForSeconds(10);
        if(chattering)
            GameManager.ConversationManager.StartConversation("CHATTER");
    }
    protected override IEnumerator Trigger2() {
        chattering = false;
        yield break;
    }
}
