using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Controls all aspects of the Player not related to Allomancy.
 */
public class Player : PewterEntity {

    private const float coinCooldownThreshold = 1f / 10;
    private const float zincTimeCoinThrowModifier = 2; // throw coins faster while in zinc time

    //private Animator animator;
    private PlayerMovementController movementController;
    private Material frameMaterial;
    private Renderer playerFrame;

    // Player components that need to be referenced elsewhere
    public static Player PlayerInstance { get; private set; }
    public static PlayerPullPushController PlayerIronSteel { get; private set; }
    public static AllomanticPewter PlayerPewter { get; private set; }
    public static Magnetic PlayerMagnetic { get; private set; }
    public static FeruchemicalZinc PlayerZinc { get; set; }

    public Hand CoinHand { get; private set; }
    private static bool canControlPlayer = false;
    private static bool godMode = false;
    public static bool CanControlPlayer {
        get {
            return canControlPlayer && !PauseMenu.IsPaused;
        }
        set {
            canControlPlayer = value;
            if (!value) {
                PlayerIronSteel.SoftClear();
                //PlayerZinc.Clear();
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

    private float coinCooldownTimer = 0;
    // In coinshot mode, clicking down to ironpull while pushing throws a coin, similar to conventional first-person shooters.
    public static bool CoinshotMode { get; private set; } = false;

    protected override void Awake() {
        base.Awake();
        movementController = GetComponentInChildren<PlayerMovementController>();
        //animator = GetComponent<Animator>();

        foreach (Renderer rend in GetComponentsInChildren<Renderer>()) {
            if (rend.CompareTag("PlayerFrame")) {
                playerFrame = rend;
                frameMaterial = playerFrame.material;
            }
        }
        PlayerInstance = this;
        PlayerIronSteel = GetComponentInChildren<PlayerPullPushController>();
        PlayerPewter = GetComponentInChildren<AllomanticPewter>();
        PlayerMagnetic = GetComponentInChildren<Magnetic>();
        PlayerZinc = GetComponent<FeruchemicalZinc>();
        Health = 100;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ClearPlayerAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPlayerBeforeSceneChange;
    }

    void Update() {
        if (CanControlPlayer) {
            if (Keybinds.ToggleCoinshotMode()) {
                CoinshotMode = !CoinshotMode;
                FlagsController.HelpOverlayFuller = true;
            }
            // On throwing a coin
            if ((CoinshotMode && Keybinds.IronPulling() && Keybinds.SteelPushing() || Keybinds.WithdrawCoinDown()) && coinCooldownTimer > coinCooldownThreshold) {
                coinCooldownTimer = 0;
                PlayerIronSteel.AddPushTarget(CoinHand.WithdrawCoinToHand());
            } else {
                coinCooldownTimer += Time.deltaTime * (PlayerZinc.InZincTime ? 2 : 1); // throw coins 
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
        GetComponentInChildren<AllomechanicalGlower>().RemoveAllEmissions();
        movementController.Clear();
        PlayerIronSteel.Clear(false);
        PlayerPewter.Clear();
        PlayerZinc.Clear();
    }

    // Reset certain values AFTER the player enters a new scene
    private void ClearPlayerAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            PlayerIronSteel.Clear();
            SetFrameMaterial(frameMaterial);
            CanControlPlayer = true;
            //if (scene.buildIndex == SceneSelectMenu.sceneLevel01)
            //    GodMode = true;
            //else
            if (scene.buildIndex == SceneSelectMenu.sceneLevel01) {
                CoinHand.Pouch.Clear();
                PlayerIronSteel.IronReserve.SetMass(0);
                PlayerIronSteel.SteelReserve.SetMass(0);
                PlayerPewter.PewterReserve.SetMass(0);
            } else {
                // For every scene except the tutorial, give metals and coins at the start.
                CoinHand.Pouch.Fill();
                PlayerIronSteel.IronReserve.SetMass(150);
                PlayerIronSteel.SteelReserve.SetMass(150);
                PlayerPewter.PewterReserve.SetMass(100);
            }
            GodMode = false;

            GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
            if (spawn && CameraController.ActiveCamera) { // if CameraController.Awake has been called
                transform.position = spawn.transform.position;
                transform.rotation = spawn.transform.rotation;
                CameraController.Clear();
            }
        }
    }

    public void SetFrameMaterial(Material mat) {
        playerFrame.GetComponent<Renderer>().material = mat;
    }
}
