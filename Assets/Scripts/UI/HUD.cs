using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manages the heads-up display.
/// Manages all the sub-controllers that manage elements on the HUD, like metal reserves and the crosshair.
/// </summary>
public class HUD : MonoBehaviour {

    // Colors of HUD elements
    public static readonly Color weakBlue = new Color(0, .75f, 1, 1);
    public static readonly Color strongBlue = new Color(0, 1f, 1, 1);
    public static readonly Color goldColor = new Color(1, 0.9411765f, .5f, .75f);

    public static bool IsOpen => hudGroup.alpha > 0;

    private static Text fPSText;
    private static CanvasGroup hudGroup;
    private static CanvasGroup hudBehindWheelGroup;
    private static Animator anim;

    private float deltaTimeFPS = 0.0f;
    private static GameObject instance;

    // HUD elements
    // Bad practice to give them the same names as their classes
    public static CrosshairController Crosshair { get; private set; }
    public static BurnPercentageMeter BurnPercentageMeter { get; private set; }
    public static MetalReserveMeters MetalReserveMeters { get; private set; }
    public static ZincMeterController ZincMeterController { get; private set; }
    public static ThrowingAmmoMeter ThrowingAmmoMeter { get; private set; }
    public static TargetOverlayController TargetOverlayController { get; private set; }
    public static ControlWheelController ControlWheelController { get; private set; }
    public static MessageOverlayDescriptive MessageOverlayDescriptive { get; private set; }
    public static MessageOverlayCinematic MessageOverlayCinematic { get; private set; }
    public static HelpOverlayController HelpOverlayController { get; private set; }
    public static ConsoleController ConsoleController { get; private set; }
    public static ConversationHUDController ConversationHUDController { get; private set; }

    void Awake() {
        instance = gameObject;
        fPSText = GetComponentInChildren<Text>();
        hudGroup = GetComponent<CanvasGroup>();
        hudBehindWheelGroup = transform.Find("BehindControlWheel").GetComponent<CanvasGroup>();
        anim = GetComponent<Animator>();
        Crosshair = GetComponentInChildren<CrosshairController>();
        BurnPercentageMeter = GetComponentInChildren<BurnPercentageMeter>();
        TargetOverlayController = GetComponentInChildren<TargetOverlayController>();
        ThrowingAmmoMeter = GetComponentInChildren<ThrowingAmmoMeter>();
        MessageOverlayDescriptive = GetComponentInChildren<MessageOverlayDescriptive>();
        MessageOverlayCinematic = GetComponentInChildren<MessageOverlayCinematic>();
        HelpOverlayController = GetComponentInChildren<HelpOverlayController>();
        MetalReserveMeters = GetComponentInChildren<MetalReserveMeters>();
        ZincMeterController = GetComponentInChildren<ZincMeterController>();
        ControlWheelController = GetComponentInChildren<ControlWheelController>();
        ConsoleController = GetComponentInChildren<ConsoleController>();
        ConversationHUDController = GetComponentInChildren<ConversationHUDController>();
        SceneManager.sceneUnloaded += ClearHUDBeforeSceneChange;
        SceneManager.sceneLoaded += ClearAfterSceneChange;
    }
    //void Start() {
    //    DisableHUD();
    //}

    void LateUpdate() {
        deltaTimeFPS += (Time.unscaledDeltaTime - deltaTimeFPS) * 0.1f;
        if (SettingsMenu.settingsInterface.fpsCounter == 1) {
            float fps = 1.0f / deltaTimeFPS;
            string text = string.Format("{0:0.} fps", fps);
            fPSText.text = text;
        } else {
            fPSText.text = "";
        }
        if (Player.PlayerIronSteel.IsBurning) {
            TargetOverlayController.SoftRefresh();
        }
    }
    private void ClearAfterSceneChange(Scene scene, LoadSceneMode mode) {
        if (scene.buildIndex != SceneSelectMenu.sceneTitleScreen) {
            TimeController.CurrentTimeScale = SettingsMenu.settingsWorld.timeScale;
            ResetHUD();
        }
    }
    // Reset the hud
    public void ClearHUDBeforeSceneChange(Scene scene) {
        ResetHUD();
    }

    // Ready HUD elements for a certain simulation
    public static void EnableHUD() {
        //instance.SetActive(true);
        hudGroup.alpha = 1;
    }

    public static void DisableHUD() {
        //instance.SetActive(false);
        hudGroup.alpha = 0;
    }
    // Clears the values currently on the HUD
    public static void ResetHUD() {
        if (SettingsMenu.settingsInterface.hudEnabled == 1) {
            EnableHUD();
            if (BurnPercentageMeter) {
                BurnPercentageMeter.Clear();
                TargetOverlayController.Clear();
                ThrowingAmmoMeter.Clear();
                MetalReserveMeters.Clear();
                MessageOverlayDescriptive.Clear();
                MessageOverlayCinematic.Clear();
                HelpOverlayController.Clear();
                ZincMeterController.Clear();
                ControlWheelController.Clear();
                ConsoleController.Clear();
                ConversationHUDController.Clear();
                Crosshair.Clear();
                anim.SetBool("ControlWheelVisible", false);
            }
        }
    }

    public static void UpdateText() {
        HelpOverlayController.UpdateText();
        ControlWheelController.RefreshText();
    }


    #region staticHelpers
    /// <summary>
    /// Converts a force into a string in Newtons or G's depending on the current Setting
    /// </summary>
    /// <param name="force">the force to convert</param>
    /// <param name="mass">the mass of the object, used for G's</param>
    /// <param name="sigFigs">the number of significant figures to write to the string</param>
    /// <returns>the string representation of the force</returns>
    public static string ForceString(float force, float mass, int sigFigs = 2) {
        if (SettingsMenu.settingsInterface.forceUnits == 1) {
            return RoundStringToSigFigs(force).ToString() + "N";
        } else {
            return RoundStringToSigFigs(force / mass / 9.81f, sigFigs) + "G's";
        }
    }

    /// <summary>
    /// Converts a mass to a string in kilograms
    /// </summary>
    /// <param name="mass">the mass to convert</param>
    /// <returns>the string representation of the mass</returns>
    public static string MassString(float mass) {
        return (mass).ToString() + "kg";
    }

    /// <summary>
    /// Converts an Allomantic Force to a string representation of its components
    /// e.g. "650N + 250N"
    /// </summary>
    /// <param name="allomanticForce">the Allomantic Force component of the force</param>
    /// <param name="normalForce">the Anchored Push Boost component of the forces</param>
    /// <param name="mass">the mass of the object, used for G's</param>
    /// <param name="sigFigs">the number of significant figures to write to the string</param>
    /// <param name="invert">flip the positive/negative convention for the +/- sign and color</param>
    /// <returns>the string representation of the force</returns>
    public static string AllomanticSumString(Vector3 allomanticForce, Vector3 normalForce, float mass, int sigFigs = 2, bool invert = false) {
        string plusSign;
        if (invert && Vector3.Dot(allomanticForce, normalForce) <= 0 || !invert && Vector3.Dot(allomanticForce, normalForce) >= 0) {
            plusSign = TextCodes.Blue("+");
        } else {
            plusSign = TextCodes.Red("-");
        }

        if (SettingsMenu.settingsInterface.forceUnits == 1) {
            return RoundStringToSigFigs(allomanticForce.magnitude, sigFigs) + " " + plusSign + " " + RoundStringToSigFigs(normalForce.magnitude, sigFigs) + "N";
        } else {
            return RoundStringToSigFigs(allomanticForce.magnitude / mass / 9.81f, sigFigs)
                + " " + plusSign + " " +
                RoundStringToSigFigs(normalForce.magnitude / mass / 9.81f, sigFigs) + "G's";
        }
    }

    /// <summary>
    /// Rounds a float to the given significant figures and converts it to a string
    /// </summary>
    /// <param name="num">the number to convert</param>
    /// <param name="sigFigs">the number of significant figures to use</param>
    /// <returns></returns>
    public static string RoundStringToSigFigs(float num, int sigFigs = 2) {
        float mag = num < 0 ? -num : num;

        if (mag < Mathf.Pow(10, sigFigs - 1)) {
            int logLevel = (int)(-Mathf.Log10(mag) + sigFigs);
            if (logLevel > 15 || logLevel < 0)
                return "0";
            return System.Math.Round(num, logLevel).ToString("." + new string('0', logLevel));
        }
        // divide it down to the right number of digits, round to int, multiply it back up
        int tenPower = (int)Mathf.Pow(10, (int)Mathf.Log10(mag) + 1 - sigFigs);
        int low = (int)Mathf.Round(num / tenPower);
        return (low * tenPower).ToString();
    }

    /// <summary>
    /// Formats a double as a time as "MM:SS:ms" and converts it into a string
    /// </summary>
    /// <param name="input">the time</param>
    /// <returns>the time as a string</returns>
    public static string TimeMMSSMS(double input) {
        int minutes = (int)input / 60;
        int seconds = (int)input % 60;
        int milliseconds = (int)((input - (int)input) * 100);
        return (minutes < 10 ? "0" : "") + minutes + (seconds < 10 ? ":0" : ":") + seconds + (milliseconds < 10 ? ":0" : ":") + milliseconds;
    }
    #endregion

    /// <summary>
    /// Fade HUD in/out with Control Wheel
    /// </summary>
    public static void ShowControlWheel() {
        if (SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad)
            CameraController.UnlockCamera();

        anim.SetBool("ControlWheelVisible", true);
    }
    public static void HideControlWheel(bool noTransition = false) {
        if (SettingsMenu.settingsGameplay.controlScheme != JSONSettings_Gameplay.Gamepad)
            CameraController.LockCamera();

        anim.SetBool("ControlWheelVisible", false);
        if (noTransition) {
            anim.Play("HUD_HideControlWheel", anim.GetLayerIndex("Visibility"));
        }
    }

    /// <summary>
    /// Clear unwanted fields after changing settings
    /// </summary>
    public void InterfaceRefresh() {
        BurnPercentageMeter.InterfaceRefresh();
        TargetOverlayController.InterfaceRefresh();
    }
}
