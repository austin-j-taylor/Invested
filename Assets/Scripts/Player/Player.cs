using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
    
    private const float coinCooldown = 0.1f;
    private readonly Vector3 feet = Vector3.zero;

    //private Animator animator;
    private PlayerMovementController movementController;

    public AllomanticIronSteel IronSteel { get; private set; }
    public CoinPouch Pouch { get; private set; }

    private float lastCoinThrowTime = 0;
    
    void Awake() {
        movementController = GetComponent<PlayerMovementController>();

        //animator = GetComponent<Animator>();
        IronSteel = GetComponent<AllomanticIronSteel>();
        Pouch = GetComponentInChildren<CoinPouch>();
        SceneManager.sceneLoaded += ResetPosition;
    }
	
	void Update () {
        // Pausing
        if (Keybinds.Pause()) {
            PauseMenu.TogglePaused();
        }

        // On pressing COIN button
        if (Keybinds.WithdrawCoinDown() && lastCoinThrowTime + coinCooldown < Time.time) {
            lastCoinThrowTime = Time.time;
            // If jump button had also been held, throw coin downward and target it.
            if (Keybinds.Jump()) {
                Coin coin = Pouch.SpawnCoin(transform.position + feet);
                IronSteel.AddTarget(coin, false);

            } else { // If only pressing the COIN button, draw a coin into hand
                Coin coin = Pouch.WithdrawCoinToHand();
                IronSteel.AddTarget(coin, false);
            }
        }
	}

    public void ReloadPlayerIntoNewScene(int scene) {
        movementController.EnterNewScene();
        IronSteel.Clear();
        Pouch.Clear();

        SceneManager.LoadScene(scene);
    }

    private void ResetPosition(Scene scene, LoadSceneMode mode) {
        GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawn) {
            transform.position = spawn.transform.position;
            transform.rotation = spawn.transform.rotation;
            FPVCameraLock.Clear();
        }
    }
    
}
