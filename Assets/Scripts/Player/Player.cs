using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Represents the player.
 */
public class Player : MonoBehaviour {

    private const float coinCooldown = 0.1f;

    //private Animator animator;
    private PlayerMovementController movementController;

    public static AllomanticIronSteel PlayerIronSteel { get; private set; }
    public static PlayerPullPushController PushPullController { get; private set; }
    public static Player PlayerInstance { get; private set; }
    public static bool CanControlPlayer { get; set; }
    public Hand CoinHand { get; private set; }

    private float lastCoinThrowTime = 0;
    
    void Awake() {
        movementController = GetComponent<PlayerMovementController>();

        //animator = GetComponent<Animator>();
        PlayerIronSteel = GetComponent<AllomanticIronSteel>();
        PushPullController = GetComponent<PlayerPullPushController>();
        PlayerInstance = this;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ClearPlayerAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPlayerBeforeSceneChange;
    }
	
	void Update () {
        if (CanControlPlayer) {
            // Pausing
            if (Keybinds.EscapeDown() && !PauseMenu.IsPaused) {
                PauseMenu.Pause();
            }
            if (!PauseMenu.IsPaused) {
                // On pressing COIN button
                if (Keybinds.WithdrawCoinDown() && lastCoinThrowTime + coinCooldown < Time.time) {
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
        CoinHand.Clear();
        GetComponentInChildren<MeshRenderer>().material = GameManager.Material_Gebaude;
        CanControlPlayer = true;

        GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawn && CameraController.ActiveCamera) { // if CameraController.Awake has been called
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
            CameraController.Clear();
        }
    }
    
}
