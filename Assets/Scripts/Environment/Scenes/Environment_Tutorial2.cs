using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TextCodes;

public class Environment_Tutorial2 : EnvironmentCinematic {

    [SerializeField]
    private GameObject backWall = null;
    [SerializeField]
    private FacilityDoor_Red door = null;

    EnvironmentalTransitionManager musicManager;

    [SerializeField]
    private bool runCenimatic = false;
    [SerializeField]
    private GameObject cenimaticObjects = null;

    void Start() {
        if (runCenimatic) {
            //StartCoroutine(Cenimatic());
        } else {
            cenimaticObjects.SetActive(false);
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
    }

    // Invoked by the Timeline for this scene to start the scripting aspect of the cenimatic
    public void StartCenimatic() {
        StartCoroutine(Cenimatic());
    }
    private IEnumerator Cenimatic() {
        CameraController.ActiveCamera.GetComponent<AudioListener>().enabled = false;
        HUD.DisableHUD();
        HUD.ConversationHUDController.gameObject.SetActive(false);
        Player.PlayerInstance.transform.position = new Vector3(0, 16.26f, 230);
        Prima.PlayerPewter.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX;
        Prima.PrimaInstance.ActorIronSteel.SteelReserve.enabled = true;
        Prima.PrimaInstance.ActorIronSteel.BubbleOpen(false);
        Prima.PlayerPewter.ExternalMovementCommand = new Vector3(0, 0, 1);

        yield return new WaitForSeconds(15);
        HUD.ConversationHUDController.gameObject.SetActive(true);

        //GameManager.SceneTransitionManager.LoadScene(SceneSelectMenu.sceneSandbox, false);
        GameManager.SceneTransitionManager.LoadScene(SceneSelectMenu.sceneTutorial1, false);
    }

    private IEnumerator Play_music() {
        while (GameManager.AudioManager.SceneTransitionIsPlaying) {
            yield return null;
        }
        musicManager.StartExterior();
    }

    private IEnumerator Procedure() {
        backWall.SetActive(false);
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
        backWall.SetActive(true);
        FlagsController.SetFlag("pwr_steel");
        HUD.MessageOverlayCinematic.FadeIn(HowToPush + " to " + Push + ".");

        while (!Prima.PrimaInstance.ActorIronSteel.HasPushTarget) {
            yield return null;
        }
        HUD.MessageOverlayCinematic.FadeOut();
    }

    protected override IEnumerator Trigger1() {
        HUD.MessageOverlayCinematic.FadeIn("Like with " + Pulling + ", you can " + Mark_pushing + " metals for " + Pushing + " with " + KeyMark_Push + ".\n Mark multiple by holding " + KeyMultiMark + ".");
        while (!door.On)
            yield return null;
        HUD.MessageOverlayCinematic.FadeOut();
    }
}
