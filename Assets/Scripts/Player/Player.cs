using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls all general aspects of the Player not covered by the sub-Player______ scripts
/// </summary>
public class Player : PewterEntity {

    #region constants
    private const float coinCooldownThreshold = 1f / 10;
    private const float zincTimeCoinThrowModifier = 2; // throw coins faster while in zinc time
    private const float defaultVoidHeight = -50; // Voiding out - if the player falls beneath voidHeight, respawn.
    #endregion

    #region properties
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
    public static VoiceBeeper PlayerVoiceBeeper { get; set; }

    public Hand CoinHand { get; private set; }
    public CoinMode CoinThrowingMode { get; set; }
    public Transform RespawnPoint { get; set; }

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
    public static bool CanThrowCoins { get; set; }

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
    // Some scenes also set the void height
    public static float VoidHeight { get; set; }
    #endregion

    [SerializeField]
    private Coin thrownCoin = null;
    private float coinCooldownTimer = 0;

    protected override void Awake() {
        base.Awake();
        //animator = GetComponent<Animator>();

        foreach (Renderer rend in GetComponentsInChildren<Renderer>()) {
            if (rend.CompareTag("PlayerFrame")) {
                playerFrame = rend;
                frameMaterial = playerFrame.material;
            }
        }
        smokeMaterial = GetComponentInChildren<ParticleSystemRenderer>().material;

        PlayerInstance = this;
        PlayerAudioController = GetComponentInChildren<PlayerAudioController>();
        PlayerIronSteel = GetComponentInChildren<PlayerPullPushController>();
        PlayerPewter = GetComponentInChildren<PlayerMovementController>();
        PlayerFlywheelController = GetComponentInChildren<PlayerFlywheelController>();
        PlayerMagnetic = GetComponentInChildren<Magnetic>();
        PlayerZinc = GetComponent<FeruchemicalZinc>();
        PlayerTransparancy = GetComponentInChildren<PlayerTransparencyController>();
        PlayerVoiceBeeper = GetComponentInChildren<VoiceBeeper>();
        Health = 100;
        CoinThrowingMode = CoinMode.Semi;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ClearPlayerAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPlayerBeforeSceneChange;
    }

    #region updates
    void Update() {
        // Handle "Voiding out" if the player falls too far into the "void"
        if (transform.position.y < VoidHeight) {
            Respawn();
        }

        if (CanControl) {
            // On throwing a coin
            if (CanThrowCoins && !CoinHand.Pouch.IsEmpty) {

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
                        // Anchoring: pressing key will toss coin
                        // Not anchoring: pressing key will add coin as Vacuous Push Target and Push on it
                        // Holding MultiTarget will mark the coin, as well
                        if (CoinThrowingMode == CoinMode.Spray) {
                            Coin[] coins = CoinHand.WithdrawCoinSprayToHand(!PlayerIronSteel.IsBurning);
                            if (!Keybinds.Walk())
                                for (int i = 0; i < Hand.spraySize; i++)
                                    PlayerIronSteel.AddPushTarget(coins[i], false, true);
                            //PlayerIronSteel.AddPushTarget(coins[i], false, !Keybinds.MultipleMarks());
                        } else {
                            Coin coin = CoinHand.WithdrawCoinToHand(!PlayerIronSteel.IsBurning);
                            if (!Keybinds.Walk()) {
                                PlayerIronSteel.RemovePushTarget(thrownCoin);
                                PlayerIronSteel.AddPushTarget(coin, false, true);
                            }
                            thrownCoin = coin;
                            //PlayerIronSteel.AddPushTarget(coin, false, !Keybinds.MultipleMarks());
                        }
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
        GetComponentInChildren<AllomechanicalGlower>().RemoveAllEmissions();
        PlayerFlywheelController.Clear();
        PlayerIronSteel.StopBurning(false);
        PlayerIronSteel.Clear();
        PlayerPewter.Clear();
        PlayerZinc.Clear();
        PlayerTransparancy.Clear();
        FeelingScale = 1;
        VoidHeight = defaultVoidHeight;

        // Disable the cloud controller
        CameraController.ActiveCamera.GetComponent<CloudMaster>().enabled = false;
    }

    /// <summary>
    /// Reset certain values AFTER the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that will be entered</param>
    /// <param name="mode">the sceme loading mode</param>
    private void ClearPlayerAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            PlayerAudioController.Clear();
            PlayerIronSteel.Clear();

            SetFrameMaterial(frameMaterial);
            SetSmokeMaterial(smokeMaterial);

            if (scene.buildIndex == SceneSelectMenu.sceneTitleScreen) {
                CanControl = false;
                CanPause = false;
                transform.position = GameObject.FindGameObjectWithTag("PlayerSpawn").transform.position;
            } else {
                CanPause = true;
                CanControl = true;
                CanControlMovement = true;

                // Set unlocked abilities
                PlayerIronSteel.IronReserve.IsEnabled = true;
                PlayerIronSteel.SteelReserve.IsEnabled = FlagsController.GetData("pwr_steel");
                PlayerPewter.PewterReserve.IsEnabled = FlagsController.GetData("pwr_pewter");
                CanControlZinc = FlagsController.GetData("pwr_zinc");
                CanThrowCoins = FlagsController.GetData("pwr_coins");
                if (FlagsController.GetData("pwr_coins")) {
                    CanThrowCoins = true;
                    CoinHand.Pouch.Fill();
                } else {
                    CanThrowCoins = false;
                    CoinHand.Pouch.Clear();
                }

                GameObject spawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
                if (spawn && CameraController.ActiveCamera) { // if CameraController.Awake has been called
                    transform.position = spawn.transform.position;
                    //transform.rotation = spawn.transform.rotation;
                    if (scene.buildIndex != SceneSelectMenu.sceneMain) {
                        CameraController.SetRotation(spawn.transform.eulerAngles);
                        CameraController.Clear();
                    }
                }
                RespawnPoint = spawn.transform;
            }
        }
    }

    /// <summary>
    /// Special collisions for player
    /// </summary>
    /// <param name="collision">the collision</param>
    protected override void OnCollisionEnter(Collision collision) {
        base.OnCollisionEnter(collision);
        // During challenges, check for The Floor Is Lava
        if (GameManager.State == GameManager.GameState.Challenge) {
            if (collision.transform.CompareTag("ChallengeFailure")) {
                ChallengesManager.FailCurrentChallenge();
            }
        }
    }

    /// <summary>
    /// Resets the player and returns them to their respawn point
    /// </summary>
    public void Respawn() {
        if (RespawnPoint) {
            transform.position = RespawnPoint.position;
            PlayerPewter.Clear();
            CameraController.Clear();
            CameraController.SetRotation(RespawnPoint.eulerAngles);
        }
    }
    #endregion

    /// <summary>
    /// Sets the player body's outer frame material
    /// </summary>
    /// <param name="mat">the new material</param>
    public void SetFrameMaterial(Material mat) {
        playerFrame.GetComponent<Renderer>().material = mat;
    }

    /// <summary>
    /// Sets the material of the smoke particles that appear during collisions
    /// </summary>
    /// <param name="mat">the new material</param>
    public void SetSmokeMaterial(Material mat) {
        GetComponentInChildren<ParticleSystemRenderer>().material = mat;
    }

    /// <summary>
    /// Used by Triggers to check if they collided with the player
    /// </summary>
    /// <param name="other">the trigger to compare to</param>
    /// <returns>true if other is the player's trigger</returns>
    public static bool IsPlayerTrigger(Collider other) {
        return other.CompareTag("Player") && !other.isTrigger;
    }
}
