using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Represents the player.
 */
public class Player : Entity {

    private const float coinCooldown = 1f / 4;

    //private Animator animator;
    private PlayerMovementController movementController;
    private Material playerMaterial;

    public static AllomanticIronSteel PlayerIronSteel { get; private set; }
    public static PlayerPullPushController PushPullController { get; private set; }
    public static Player PlayerInstance { get; private set; }
    public static bool CanControlPlayer { get; set; }
    public Hand CoinHand { get; private set; }

    private float lastCoinThrowTime = 0;
    // In coinshot mode, clicking down to ironpull while pushing throws a coin, similar to conventional first-person shooters.
    private bool coinshotMode = false;

    void Awake() {
        movementController = GetComponentInChildren<PlayerMovementController>();
        //animator = GetComponent<Animator>();
        playerMaterial = GetComponentInChildren<MeshRenderer>().material;
        PlayerIronSteel = GetComponentInChildren<AllomanticIronSteel>();
        PushPullController = GetComponentInChildren<PlayerPullPushController>();
        PlayerInstance = this;
        Health = 100;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ClearPlayerAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPlayerBeforeSceneChange;
    }

    void Update() {
        if (CanControlPlayer) {
            // Pausing
            if (Keybinds.EscapeDown() && !PauseMenu.IsPaused) {
                PauseMenu.Pause();
            }
            if (!PauseMenu.IsPaused) {
                if(Keybinds.ToggleCoinshotMode()) {
                    coinshotMode = !coinshotMode;
                }
                // On throwing a coin
                if((coinshotMode && Keybinds.IronPulling() && Keybinds.SteelPushing() || Keybinds.WithdrawCoinDown()) && lastCoinThrowTime + coinCooldown < Time.time) {
                    lastCoinThrowTime = Time.time;
                    PlayerIronSteel.AddPushTarget(CoinHand.WithdrawCoinToHand());
                }
            }
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
            PushPullController.Clear();
            GetComponentInChildren<MeshRenderer>().material = playerMaterial;
            CanControlPlayer = true;

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
