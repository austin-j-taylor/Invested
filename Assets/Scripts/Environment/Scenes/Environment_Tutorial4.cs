using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextCodes;

public class Environment_Tutorial4 : EnvironmentCinematic {

    EnvironmentalTransitionManager musicManager;
    [SerializeField]
    private Node doorNode1 = null, doorNode2 = null;

    void Start() {
        Player.VoidHeight = -200;
        musicManager = GetComponentInChildren<EnvironmentalTransitionManager>();

        Player.CanControl = false;
        Player.CanControlMovement = false;

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
    }

    protected override IEnumerator Trigger0() {
        yield return null;

        Player.CanThrowCoins = true;
        FlagsController.SetFlag("pwr_coins");
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        if(Player.PlayerInstance.CoinHand.Pouch.Count == 0) {
            HUD.MessageOverlayCinematic.FadeIn(HowToPull + " near " + O_Coins + " to pick them up.");
            while (Player.PlayerInstance.CoinHand.Pouch.Count == 0)
                yield return null;

            HUD.MessageOverlayCinematic.FadeOut();
            yield return new WaitForSecondsRealtime(1);
        }
        HUD.MessageOverlayCinematic.FadeIn(HowToThrow + "  to throw and " + Push + " on a " + O_Coin + ".");
        while(!doorNode1.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOutInto("Open the " + ControlWheel + " and choose " + CoinshotMode + ".");
        while (Player.PlayerIronSteel.Mode != PlayerPullPushController.ControlMode.Coinshot) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOutInto("In " + CoinshotMode + ", " + HowToPull + " to throw " + O_Coins);
        while (!doorNode2.On) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }

    protected override IEnumerator Trigger1() {
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        yield return new WaitForSeconds(10);
        GameManager.ConversationManager.StartConversation("THINK2");

        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        FlagsController.SetFlag("pwr_zinc");
        HUD.MessageOverlayCinematic.FadeIn(HowToZincTime + " to tap " + Zinc + ".");
        while (!Player.PlayerZinc.InZincTime) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOutInto("The " + ZincBlue("zinc bank") + " slows time when tapped, and recharges when not in use.");
        yield return new WaitForSecondsRealtime(5);
        HUD.MessageOverlayCinematic.FadeOut();
    }
    protected override IEnumerator Trigger2() {
        HUD.MessageOverlayCinematic.FadeIn("Opening the " + ControlWheel + " also taps " + Zinc + ".");
        while (!Player.PlayerZinc.InZincTime) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
