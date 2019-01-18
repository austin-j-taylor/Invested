using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Controls all aspects of the Player not related to Allomancy.
 */
public class Player : Entity {

    private const float coinCooldown = 1f / 8;

    //private Animator animator;
    private PlayerMovementController movementController;
    private Material playerMaterial;

    // Player components that need to be referenced elsewhere
    public static Player PlayerInstance { get; private set; }
    public static PlayerPullPushController PlayerIronSteel { get; private set; }
    public static Magnetic PlayerMagnetic { get; private set; }

    public Hand CoinHand { get; private set; }
    private static bool canControlPlayer = false;
    private static bool godMode = false;
    public static bool CanControlPlayer {
        get {
            return canControlPlayer;
        }
        set {
            canControlPlayer = value;
            if (!value) {
                PlayerIronSteel.SoftClear();
            }
        }
    }
    public static bool GodMode { // Player does not run out of metals
        get {
            return godMode;
        }
        private set {
            if (value) {
                PlayerIronSteel.IronReserve.IsEndless = true;
                PlayerIronSteel.SteelReserve.IsEndless = true;
            } else {
                PlayerIronSteel.IronReserve.IsEndless = false;
                PlayerIronSteel.SteelReserve.IsEndless = false;
            }
            godMode = value;
        }
    }

    private float lastCoinThrowTime = 0;
    // In coinshot mode, clicking down to ironpull while pushing throws a coin, similar to conventional first-person shooters.
    private bool coinshotMode = false;

    void Awake() {
        movementController = GetComponentInChildren<PlayerMovementController>();
        //animator = GetComponent<Animator>();
        PlayerInstance = this;
        playerMaterial = GetComponentInChildren<MeshRenderer>().material;
        PlayerIronSteel = GetComponentInChildren<PlayerPullPushController>();
        PlayerMagnetic = GetComponentInChildren<Magnetic>();
        Health = 100;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ClearPlayerAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPlayerBeforeSceneChange;
    }

    void Update() {
        if (CanControlPlayer) {
            if (!PauseMenu.IsPaused) {
                if (Keybinds.ToggleCoinshotMode()) {
                    coinshotMode = !coinshotMode;
                }
                // On throwing a coin
                if ((coinshotMode && Keybinds.IronPulling() && Keybinds.SteelPushing() || Keybinds.WithdrawCoinDown()) && lastCoinThrowTime + coinCooldown < Time.time) {
                    lastCoinThrowTime = Time.time;
                    PlayerIronSteel.AddPushTarget(CoinHand.WithdrawCoinToHand());
                }
            }
        }
    }

    protected override void LateUpdate() {
        // Pausing
        if (Keybinds.EscapeDown() && !PauseMenu.IsPaused) {
            PauseMenu.Pause();
        }
        // Displaying Help Overlay
        if (Keybinds.ToggleHelpOverlay()) {
            HUD.HelpOverlayController.Toggle();
        }
    }

    // Reset certain values BEFORE the player enters a new scene
    public void ClearPlayerBeforeSceneChange(Scene scene) {
        movementController.Clear();
        PlayerIronSteel.Clear(false);
    }

    // Reset certain values AFTER the player enters a new scene
    private void ClearPlayerAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            CoinHand.Clear();
            PlayerIronSteel.Clear();
            GetComponentInChildren<MeshRenderer>().material = playerMaterial;
            CanControlPlayer = true;
            //if (scene.buildIndex == SceneSelectMenu.sceneLevel01)
            //    GodMode = true;
            //else
            GodMode = false;

            GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
            if (spawn && CameraController.ActiveCamera) { // if CameraController.Awake has been called
                transform.position = spawn.transform.position;
                transform.rotation = spawn.transform.rotation;
                CameraController.Clear();
            }
        }
    }

    public override void OnHit(float damage) {
        base.OnHit(damage);
    }
}
