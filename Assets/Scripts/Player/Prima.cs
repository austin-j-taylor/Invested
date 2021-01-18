using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the player when controlling Prima.
/// </summary>
public class Prima : Actor {

    #region constants
    private const float coinCooldownThreshold = 1f / 10;
    private const float zincTimeCoinThrowModifier = 2; // throw coins faster while in zinc time
    #endregion

    #region properties
    public enum CoinMode { Semi, Full, Spray };

    public static Prima PrimaInstance { get; private set; }

    //private Animator animator;
    private Material frameMaterial, smokeMaterial;
    private Renderer playerFrame;

    // Player components that need to be referenced elsewhere
    public static PewterEntity PrimaEntity { get; private set; }
    public static PrimaAudioController PrimaAudioController { get; private set; }
    public static PrimaMovementController PlayerPewter { get; private set; }
    public static PrimaFlywheelController PrimaFlywheelController { get; private set; }
    public static Magnetic PlayerMagnetic { get; private set; }
    public static PrimaTransparencyController PlayerTransparancy { get; set; }
    public static AllomechanicalGlower PlayerGlower { get; set; }

    public Hand CoinHand { get; private set; }
    public CoinMode CoinThrowingMode { get; set; }
    #endregion

    private float coinCooldownTimer = 0;

    protected override void Awake() {
        PrimaInstance = this;
        type = ActorType.Prima;
        RespawnHeightOffset = 0;

        base.Awake();

        foreach (Renderer rend in GetComponentsInChildren<Renderer>()) {
            if (rend.CompareTag("PlayerFrame")) {
                playerFrame = rend;
                frameMaterial = playerFrame.material;
            }
        }
        smokeMaterial = GetComponentInChildren<ParticleSystemRenderer>().material;

        PrimaEntity = GetComponentInChildren<PewterEntity>();
        PrimaAudioController = GetComponentInChildren<PrimaAudioController>();
        PlayerPewter = GetComponentInChildren<PrimaMovementController>();
        PrimaFlywheelController = GetComponentInChildren<PrimaFlywheelController>();
        PlayerMagnetic = GetComponentInChildren<Magnetic>();
        PlayerTransparancy = GetComponentInChildren<PrimaTransparencyController>();
        PlayerGlower = GetComponentInChildren<AllomechanicalGlower>();
        CoinThrowingMode = CoinMode.Semi;
        CoinHand = GetComponentInChildren<Hand>();
        SceneManager.sceneLoaded += ClearPrimaAfterSceneChange;
        SceneManager.sceneUnloaded += ClearPrimaBeforeSceneChange;
    }

    #region updates
    void Update() {
        if (Player.CanControl) {
            // Coin management
            if (Player.CanThrowCoins) {
                // If releasing the "throw coin" button, remove all vacuous coin targets
                if (Keybinds.WithdrawCoinUp()) {
                    ActorIronSteel.RemoveAllCoins();
                } else if (!CoinHand.Pouch.IsEmpty) {
                    // For throwing coins
                    if (coinCooldownTimer > coinCooldownThreshold) {
                        // TODO: simplify logic. just like this for thinking
                        bool firing = false;
                        if (Keybinds.WithdrawCoinDown() || Keybinds.TossCoinDown())
                            firing = true;
                        if (ActorIronSteel.Mode == PrimaPullPushController.ControlMode.Coinshot) {
                            if (!ActorIronSteel.HasPullTarget) {
                                if (Keybinds.PullDown() || (CoinThrowingMode == CoinMode.Full && Keybinds.IronPulling())) {
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
                            if (CoinThrowingMode == CoinMode.Spray) {
                                Coin[] coins = CoinHand.WithdrawCoinSprayToHand(false);
                                if (!Keybinds.TossCoinCondition()) {
                                    ActorIronSteel.RemoveAllCoins();
                                    for (int i = 0; i < Hand.spraySize; i++)
                                        ActorIronSteel.AddPushTarget(coins[i], false, true);
                                }
                                //PlayerIronSteel.AddPushTarget(coins[i], false, !Keybinds.MultipleMarks());
                            } else {
                                Coin coin = CoinHand.WithdrawCoinToHand(false);
                                if (!Keybinds.TossCoinCondition()) {
                                    ActorIronSteel.RemoveAllCoins();
                                    ActorIronSteel.AddPushTarget(coin, false, true);
                                }
                                //PlayerIronSteel.AddPushTarget(coin, false, !Keybinds.MultipleMarks());
                            }
                        }
                    } else {
                        coinCooldownTimer += Time.deltaTime * (Player.PlayerZinc.InZincTime ? 2 : 1); // throw coins 
                    }
                }
            }
        }
    }
    #endregion

    #region clearing
    /// <summary>
    /// Reset certain values BEFORE the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that we are leaving</param>
    public void ClearPrimaBeforeSceneChange(Scene scene) {
        PlayerGlower.Clear();
        PrimaFlywheelController.Clear();
        ActorIronSteel.StopBurning(false);
        ActorIronSteel.Clear();
        PlayerPewter.Clear();
        PlayerTransparancy.Clear();
    }

    /// <summary>
    /// Reset certain values AFTER the player enters a new scene
    /// </summary>
    /// <param name="scene">the scene that will be entered</param>
    /// <param name="mode">the sceme loading mode</param>
    private void ClearPrimaAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (mode == LoadSceneMode.Single) { // Not loading all of the scenes, as it does at startup
            PrimaAudioController.Clear();

            ResetFrameMaterial();
            SetSmokeMaterial(smokeMaterial);

            if (scene.buildIndex == SceneSelectMenu.sceneTitleScreen) {
                Player.CanControl = false;
                Player.CanPause = false;
                transform.position = GameObject.FindGameObjectWithTag("PlayerSpawn").transform.position;
                ActorIronSteel.IronReserve.IsEnabled = true;
            } else {
                Player.CanPause = true;
                Player.CanControl = true;
                Player.CanControlMovement = true;

                // Set unlocked abilities
                ActorIronSteel.IronReserve.IsEnabled = true;
                ActorIronSteel.SteelReserve.IsEnabled = FlagsController.GetData("pwr_steel");
                PlayerPewter.PewterReserve.IsEnabled = FlagsController.GetData("pwr_pewter");
                Player.CanControlZinc = FlagsController.GetData("pwr_zinc");
                Player.CanThrowCoins = FlagsController.GetData("pwr_coins");
                if (Player.CanThrowCoins) {
                    CoinHand.Pouch.Fill();
                } else {
                    CoinHand.Pouch.Clear();
                }
            }
        }
    }

    public override void RespawnClear() {
        base.RespawnClear();

        PlayerPewter.Clear();
    }

    #endregion

    /// <summary>
    /// Sets the player body's outer frame material
    /// </summary>
    /// <param name="mat">the new material</param>
    public void SetFrameMaterial(Material mat) {
        playerFrame.GetComponent<Renderer>().material = mat;
    }
    public void ResetFrameMaterial() {
        playerFrame.GetComponent<Renderer>().material = frameMaterial;
    }

    /// <summary>
    /// Sets the material of the smoke particles that appear during collisions
    /// </summary>
    /// <param name="mat">the new material</param>
    public void SetSmokeMaterial(Material mat) {
        GetComponentInChildren<ParticleSystemRenderer>().material = mat;
    }
}
