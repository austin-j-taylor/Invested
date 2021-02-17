using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls all general aspects of the Player, including swapping control between Kog and Prima
/// </summary>
public class Player : MonoBehaviour {

    #region constants
    private const float defaultVoidHeight = -50; // Voiding out - if the player falls beneath voidHeight, respawn.
    public const float respawnTime = 1.25f; // Time between reaching the void and teleporting back to the respawn point
    #endregion

    #region properties
    public enum PlayerRespawnState { Normal, Respawning };

    // Player components that need to be referenced elsewhere
    public static Player PlayerInstance { get; private set; }
    public static FeruchemicalZinc PlayerZinc { get; set; }
    public static Actor CurrentActor { get; set; }

    public PlayerRespawnState respawnState { get; set; }
    public Transform RespawnPoint { get; set; }

    private static bool canControl;
    public static bool CanControl {
        get {
            return canControl;
        }
        set {
            canControl = value;
            if (!value) {
                CurrentActor.ActorIronSteel.StopBurning();
            }
        }
    }
    public static bool CanPause { get; set; }
    public static bool CanControlZinc { get; set; }
    public static bool CanControlMovement { get; set; }
    public static bool CanThrowCoins { get; set; }
    public static bool IsPrima => CurrentActor.GetType() == typeof(Prima);

    // Some scenes (Storms, Sea of Metal) should feel larger than other scenes (Luthadel, MARL).
    // This is done by increasing camera distance and Allomantic strength.
    private static float feelingScale = 1;
    public static float FeelingScale {
        get {
            return feelingScale;
        }
        set {
            feelingScale = value;
            CurrentActor.ActorIronSteel.StrengthModifier = value;
        }
    }
    // Some scenes also set the void height
    public static float VoidHeight { get; set; }
    #endregion

    void Awake() {
        PlayerInstance = this;
        PlayerZinc = GetComponent<FeruchemicalZinc>();
        CurrentActor = Prima.PrimaInstance;

        SceneManager.sceneLoaded += ClearPlayerAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPlayerBeforeSceneChange;
    }

    #region updates
    void Update() {
        // Handle "Voiding out" if the player falls too far into the "void"
        if (CurrentActor.transform.position.y < VoidHeight) {
            Respawn();
        }
    }

    private void LateUpdate() {
        // Displaying Help Overlay
        if (Keybinds.ToggleHelpOverlay()) {
            HUD.HelpOverlayController.Toggle();
        }
        if (!GameManager.MenusController.pauseMenu.IsOpen && Keybinds.ToggleConsole()) {
            HUD.ConsoleController.Toggle();
        }
        // Changing perspective
        if (Keybinds.TogglePerspective()) {
            CameraController.TogglePerspective();
        }
    }
    #endregion

    #region clearing
    /// <summary>
    /// Reset certain values BEFORE the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that we are leaving</param>
    public void ClearPlayerBeforeSceneChange(Scene scene) {
        PlayerZinc.Clear();
        FeelingScale = 1;
        VoidHeight = defaultVoidHeight;

        // Disable the cloud controller
        CameraController.ActiveCamera.GetComponent<CloudMaster>().enabled = false;

        Kog.KogInstance.ClearKogBeforeSceneChange(scene);
    }

    /// <summary>
    /// Reset certain values AFTER the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that will be entered</param>
    /// <param name="mode">the sceme loading mode</param>
    private void ClearPlayerAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            respawnState = PlayerRespawnState.Normal;

            if (scene.buildIndex != SceneSelectMenu.sceneTitleScreen) {
                GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
                if (spawn && CameraController.ActiveCamera) { // if CameraController.Awake has been called
                    CurrentActor.transform.position = spawn.transform.position + new Vector3(0, CurrentActor.RespawnHeightOffset, 0);
                    //transform.rotation = spawn.transform.rotation;
                    if (scene.buildIndex != SceneSelectMenu.sceneMain) {
                        CameraController.SetRotation(spawn.transform.eulerAngles);
                        CameraController.Clear();
                    }
                }
                RespawnPoint = spawn.transform;
            }
        }
        Kog.KogInstance.ClearKogAfterSceneChange(scene, mode);
    }

    /// <summary>
    /// Special collisions for player
    /// </summary>
    /// <param name="collision">the collision</param>
    private void OnCollisionEnter(Collision collision) {
        // During challenges, check for The Floor Is Lava
        if (GameManager.PlayState == GameManager.GamePlayState.Challenge) {
            if (collision.transform.CompareTag("ChallengeFailure")) {
                ChallengesManager.FailCurrentChallenge();
            }
        }
    }

    /// <summary>
    /// Resets the player and returns them to their respawn point
    /// </summary>
    public void Respawn() {
        if (RespawnPoint && respawnState == PlayerRespawnState.Normal) {
            respawnState = PlayerRespawnState.Respawning;
            StartCoroutine(RespawnRoutine());
        }
    }
    private IEnumerator RespawnRoutine() {
        HUD.LoadingFadeController.Enshroud();
        CanControlMovement = false;
        CanPause = false;
        yield return new WaitForSecondsRealtime(respawnTime);
        CurrentActor.transform.position = RespawnPoint.position + new Vector3(0, CurrentActor.RespawnHeightOffset, 0);
        CurrentActor.RespawnClear();
        CameraController.Clear();
        CameraController.SetRotation(RespawnPoint.eulerAngles);
        CanControlMovement = true;
        CanPause = true;
        respawnState = PlayerRespawnState.Normal;
    }

    /// <summary>
    /// Changes the actor to the new one.
    /// </summary>
    /// <param name="actor">The new actor</param>
    public void SetActor(Actor actor) {
        switch(actor.Type) {
            case Actor.ActorType.Prima:
                Prima.PrimaInstance.gameObject.SetActive(true);
                Kog.KogInstance.gameObject.SetActive(false);
                break;
            case Actor.ActorType.Kog:
                Kog.KogInstance.gameObject.SetActive(true);
                Prima.PrimaInstance.gameObject.SetActive(false);
                break;
        }
        CurrentActor = actor;
        actor.ActorIronSteel.StrengthModifier = feelingScale;
        HUD.ControlWheelController.RefreshOptions();
    }
    #endregion


    /// <summary>
    /// Used by Triggers to check if they collided with the player
    /// </summary>
    /// <param name="other">the trigger to compare to</param>
    /// <returns>true if other is the player's trigger</returns>
    public static bool IsPlayerTrigger(Collider other) {
        return other.CompareTag("Player") && !other.isTrigger;
    }
}
