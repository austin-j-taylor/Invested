using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour {
    
    private const float coinCooldown = 0.1f;
    private readonly Vector3 feet = Vector3.zero;

    //private Animator animator;
    private PauseMenu pauseMenu;
    private PlayerMovementController movementController;
    private FPVCameraLock cameraController;

    public AllomanticIronSteel IronSteel { get; private set; }
    public CoinPouch Pouch { get; private set; }

    private float lastCoinThrowTime = 0;
    
    void Awake () {
        pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu").GetComponent<PauseMenu>();
        movementController = GetComponent<PlayerMovementController>();
        cameraController = GetComponentInChildren<FPVCameraLock>();

        //animator = GetComponent<Animator>();
        IronSteel = GetComponent<AllomanticIronSteel>();
        Pouch = GetComponentInChildren<CoinPouch>();
    }
	
	void Update () {
        // Pausing
        if (Keybinds.Pause()) {
            pauseMenu.TogglePaused();
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
        GetComponentInChildren<Camera>().enabled = true;
        IronSteel.Clear();
        Pouch.Clear();

        SceneManager.LoadScene(scene);
    }

    public void ResetPosition(PlayerSpawn spawn) {
        transform.position = spawn.transform.position;
        transform.rotation = spawn.transform.rotation;
        cameraController.Clear();
    }
}
