using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextCodes;

public class Environment_Tutorial4 : EnvironmentCinematic {

    private const int distanceFromEndToThrumming = 335;

    EnvironmentalTransitionManager musicManager;
    [SerializeField]
    private GateRising door1 = null, door2 = null, door3 = null;
    [SerializeField]
    private HarmonyTarget harmonyTarget = null;

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
        while(!door1.Unlocked) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOutInto("Open the " + ControlWheel + " and choose " + CoinshotMode + ".");
        while (Player.PlayerIronSteel.Mode != PlayerPullPushController.ControlMode.Coinshot) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOutInto("In " + CoinshotMode + ", " + HowToPull + " to throw " + O_Coins);
        while (!door2.Unlocked) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }

    protected override IEnumerator Trigger1() {
        //  Give them coins if they sequence broke
        Player.CanThrowCoins = true;
        FlagsController.SetFlag("pwr_coins");
        while (HUD.ConversationHUDController.IsOpen)
            yield return null;
        yield return new WaitForSeconds(20);
        if(!door3.Unlocked)
            GameManager.ConversationManager.StartConversation("THINK2");
    }
    protected override IEnumerator Trigger2() {
        // Fade in the Thrumming
        AudioSource source = GetComponent<AudioSource>();
        source.volume = 0;
        source.Play();

        while(true) {
            float distance = (Player.PlayerInstance.transform.position - harmonyTarget.transform.position).magnitude;
            // Reset if player respawns
            if(distance > 5 + distanceFromEndToThrumming) {
                source.Stop();
                musicManager.SetExteriorVolume(1);
                while (distance > distanceFromEndToThrumming) {
                    distance = (Player.PlayerInstance.transform.position - harmonyTarget.transform.position).magnitude;
                    yield return null;
                }
                source.Play();
            }
            // Fade in as it gets closer
            float vol = distance / distanceFromEndToThrumming;
            musicManager.SetExteriorVolume(vol);
            source.volume = 1 - vol;

            yield return null;
        }
    }
}
