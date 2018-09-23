using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {

    private const float coinCooldown = 0.1f;

    //private Animator animator;
    private PlayerMovementController movementController;

    public static AllomanticIronSteel PlayerIronSteel { get; private set; }
    public static PlayerPushPullController PushPullController { get; private set; }
    public static Player PlayerInstance { get; private set; }
    public static bool CanControlPlayer { get; set; }
    public Hand CoinHand { get; private set; }

    private float lastCoinThrowTime = 0;
    
    void Awake() {
        movementController = GetComponent<PlayerMovementController>();

        //animator = GetComponent<Animator>();
        PlayerIronSteel = GetComponent<AllomanticIronSteel>();
        PushPullController = GetComponent<PlayerPushPullController>();
        PlayerInstance = this;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ResetPosition;
    }
	
	void Update () {
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

    // Reset certain values before the player enters a new scene
    public void ReloadPlayerIntoNewScene(int scene) {
        CanControlPlayer = true;
        GetComponentInChildren<MeshRenderer>().material = GameManager.Material_Gebaude;

        movementController.Clear();
        PlayerIronSteel.Clear();
        CoinHand.Clear();
    }

    // Reset position to SpawnPosition at beginning of each scene
    private void ResetPosition(Scene scene, LoadSceneMode mode) {
        GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawn && CameraController.ActiveCamera) { // if CameraController.Awake has been called
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
            CameraController.Clear();
        }
    }
    
}
