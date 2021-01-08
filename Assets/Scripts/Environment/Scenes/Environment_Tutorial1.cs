using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Cinemachine;
using static TextCodes;
using UnityEngine.UI;

public class Environment_Tutorial1 : EnvironmentCinematic {

    EnvironmentalTransitionManager musicManager;

    [SerializeField]
    private bool runCenimatic = false;
    [SerializeField]
    private GameObject cenimaticObjects = null;

    private bool chattering = true;

    void Start() {
        if(runCenimatic) {
            //StartCoroutine(Cenimatic());
        } else {
            cenimaticObjects.SetActive(false);
            musicManager = GetComponentInChildren<EnvironmentalTransitionManager>();

            Player.CanControl = false;
            Player.CanControlMovement = false;
            if (!FlagsController.GetData("pwr_steel"))
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
    }

    // Invoked by the Timeline for this scene to start the scripting aspect of the cenimatic
    public void StartCenimatic() {
        StartCoroutine(Cenimatic());
    }
    private IEnumerator Cenimatic() {
        CameraController.ActiveCamera.GetComponent<AudioListener>().enabled = false;
        GameManager.Canvas.Find("CinematicBlackScreen").GetComponent<Image>().enabled = true;
        HUD.ConversationHUDController.gameObject.SetActive(true);
        HUD.DisableHUD();
        yield return new WaitForSeconds(1f);
        HUD.ConversationHUDController.GetComponent<CanvasGroup>().ignoreParentGroups = true;
        GameManager.ConversationManager.StartConversation("THINK1");
        yield return new WaitForSeconds(5.5f);
        GameManager.ConversationManager.Clear();
        yield return new WaitForSeconds(1);
        HUD.ConversationHUDController.GetComponent<CanvasGroup>().ignoreParentGroups = false;
        GameManager.Canvas.Find("CinematicBlackScreen").GetComponent<Image>().enabled = false;


        yield return new WaitForSeconds(5.5f);
        GameManager.SceneTransitionManager.LoadScene(SceneSelectMenu.sceneTutorial4, false);
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
        if(!HUD.HelpOverlayController.IsOpen)
            HUD.HelpOverlayController.Toggle();
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
        HUD.MessageOverlayCinematic.FadeOutInto("Look at a " + LightBlue("metal") + " and " + HowToPull + " to " + Pull + ".");

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
        HUD.MessageOverlayCinematic.FadeOutInto("The " + ZincBlue("zinc bank") + " slows time and brightens " + LightBlue("blue lines") + ".");
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
