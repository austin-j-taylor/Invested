using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Controls all aspects of the Player not related to Allomancy.
 */
public class Player : PewterEntity {

    private const float coinCooldownThreshold = 1f / 10;
    private const float zincTimeCoinThrowModifier = 2; // throw coins faster while in zinc time

    public enum CoinMode { Semi, Full, Spray };

    //private Animator animator;
    private Material frameMaterial, smokeMaterial;
    private Renderer playerFrame;

    // Player components that need to be referenced elsewhere
    public static Player PlayerInstance { get; private set; }
    public static PlayerAudioController PlayerAudioController { get; private set; }
    public static PlayerPullPushController PlayerIronSteel { get; private set; }
    public static PlayerMovementController PlayerPewter { get; private set; }
    public static PlayerFlywheelController PlayerFlywheelController { get; private set; }
    public static Magnetic PlayerMagnetic { get; private set; }
    public static FeruchemicalZinc PlayerZinc { get; set; }
    public static PlayerTransparencyController PlayerTransparancy { get; set; }

    public Hand CoinHand { get; private set; }
    public CoinMode CoinThrowingMode { get; set; }

    //public static bool CanControlWheel { get; set; }
    //public static bool CanControlZinc { get; set; }
    //public static bool CanControlMovement { get; set; }
    //private static bool canControlPushes = false;
    //public static bool CanControlPushes {
    //    get {
    //        return canControlPushes && !PauseMenu.IsPaused;
    //    }
    //    set {
    //        canControlPushes = value;
    //        if (!value) {
    //            PlayerIronSteel.SoftClear();
    //        }
    //    }
    //}
    private static bool canControl;
    public static bool CanControl {
        get {
            return canControl;
        }
        set {
            canControl = value;
            if (!value) {
                PlayerIronSteel.StopBurning();
            }
        }
    }
    public static bool CanPause { get; set; }
    public static bool CanControlZinc { get; set; }
    public static bool CanControlMovement { get; set; }

    //private static bool godMode = false;
    //public static bool GodMode { // Player does not run out of metals
    //    get {
    //        return godMode;
    //    }
    //    private set {
    //        if (value) {
    //            PlayerIronSteel.IronReserve.IsEndless = true;
    //            PlayerIronSteel.SteelReserve.IsEndless = true;
    //        } else {
    //            PlayerIronSteel.IronReserve.IsEndless = false;
    //            PlayerIronSteel.SteelReserve.IsEndless = false;
    //        }
    //        godMode = value;
    //    }
    //}
    // Some scenes (Storms, Sea of Metal) should feel larger than other scenes (Luthadel, MARL).
    // This is done by increasing camera distance and Allomantic strength.
    private static float feelingScale = 1;
    public static float FeelingScale {
        get {
            return feelingScale;
        }
        set {
            feelingScale = value;
            PlayerIronSteel.Strength = value;
        }
    }

    private float coinCooldownTimer = 0;

    protected override void Awake() {
        base.Awake();
        //animator = GetComponent<Animator>();
        Debug.Log("Starting.");

        foreach (Renderer rend in GetComponentsInChildren<Renderer>()) {
            if (rend.CompareTag("PlayerFrame")) {
                playerFrame = rend;
                frameMaterial = playerFrame.material;
            }
        }
        smokeMaterial = GetComponentInChildren<ParticleSystemRenderer>().material;

        PlayerInstance = this;
        PlayerAudioController = CameraController.ActiveCamera.GetComponentInChildren<PlayerAudioController>();
        PlayerIronSteel = GetComponentInChildren<PlayerPullPushController>();
        PlayerPewter = GetComponentInChildren<PlayerMovementController>();
        PlayerFlywheelController = GetComponentInChildren<PlayerFlywheelController>();
        PlayerMagnetic = GetComponentInChildren<Magnetic>();
        PlayerZinc = GetComponent<FeruchemicalZinc>();
        PlayerTransparancy = GetComponentInChildren<PlayerTransparencyController>();
        Health = 100;
        CoinThrowingMode = CoinMode.Semi;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ClearPlayerAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPlayerBeforeSceneChange;
    }

    void Update() {
        if (CanControl) {
            // On throwing a coin
            if (!CoinHand.Pouch.IsEmpty) {

                if (coinCooldownTimer > coinCooldownThreshold) {
                    // TODO: simplify logic. just like this for thinking
                    bool firing = false;
                    if (Keybinds.WithdrawCoinDown())
                        firing = true;
                    if (PlayerIronSteel.Mode == PlayerPullPushController.ControlMode.Coinshot) {
                        if (!PlayerIronSteel.HasPullTarget) {
                            if (Keybinds.PullDown() || CoinThrowingMode == CoinMode.Full && Keybinds.IronPulling()) {
                                firing = true;
                            }
                        }
                    } else {
                        if (CoinThrowingMode == CoinMode.Full && Keybinds.WithdrawCoin()) {
                            firing = true;
                        }
                    }

                    if (firing) {
                        coinCooldownTimer = 0;
                        if (CoinThrowingMode == CoinMode.Spray) {
                            Coin[] coins = CoinHand.WithdrawCoinSprayToHand();
                            for (int i = 0; i < Hand.spraySize; i++)
                                PlayerIronSteel.AddPushTarget(coins[i], false, true);
                        } else
                            PlayerIronSteel.AddPushTarget(CoinHand.WithdrawCoinToHand(), false, true);

                    }
                } else {
                    coinCooldownTimer += Time.deltaTime * (PlayerZinc.InZincTime ? 2 : 1); // throw coins 
                }
            }
        }
    }

    protected override void LateUpdate() {
        // Pausing
        if (Keybinds.EscapeDown() && !PauseMenu.IsPaused && CanPause) {
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
        PlayerFlywheelController.Clear();
        PlayerIronSteel.StopBurning(false);
        PlayerIronSteel.Clear();
        PlayerPewter.Clear();
        PlayerZinc.Clear();
        PlayerTransparancy.Clear();
        FeelingScale = 1;

        // Disable the cloud controller
        CameraController.ActiveCamera.GetComponent<CloudMaster>().enabled = false;
    }

    // Reset certain values AFTER the player enters a new scene
    // TODO Clear flags upon entering first level
    private void ClearPlayerAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            PlayerAudioController.Clear();
            PlayerIronSteel.Clear();
            CanControl = true;
            CanControlMovement = true;
            CanControlZinc = true;
            //GodMode = true;

            SetFrameMaterial(frameMaterial);
            SetSmokeMaterial(smokeMaterial);

            GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
            if (spawn && CameraController.ActiveCamera) { // if CameraController.Awake has been called
                transform.position = spawn.transform.position;
                //transform.rotation = spawn.transform.rotation;
                if (scene.buildIndex != SceneSelectMenu.sceneMain) {
                    CameraController.Clear();
                    CameraController.SetRotation(spawn.transform.eulerAngles);
                }
            }
        }
    }

    public void SetFrameMaterial(Material mat) {
        playerFrame.GetComponent<Renderer>().material = mat;
    }

    public void SetSmokeMaterial(Material mat) {
        GetComponentInChildren<ParticleSystemRenderer>().material = mat;
    }
}
