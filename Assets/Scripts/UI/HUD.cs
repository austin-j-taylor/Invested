using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the heads-up display.
 * Has several fields that control UI elements that relate to the HUD.
 */
public class HUD : MonoBehaviour {

    public static readonly Color weakBlue = new Color(0, .75f, 1, 1);
    public static readonly Color strongBlue = new Color(0, 1f, 1, 1);

    private static Text fPSText;
    private static CanvasGroup hudGroup;
    private static CanvasGroup hudBehindWheelGroup;
    private static Animator anim;

    private float deltaTimeFPS = 0.0f;

    public static GameObject HudGameObject {
        get; private set;
    }
    public static Image Crosshair{
        get; private set;
    }
    public static BurnPercentageMeter BurnPercentageMeter {
        get; private set;
    }
    public static MetalReserveMeters MetalReserveMeters {
        get; private set;
    }
    public static ZincMeterController ZincMeterController {
        get; private set;
    }
    public static ThrowingAmmoMeter ThrowingAmmoMeter {
        get; private set;
    }
    public static TargetOverlayController TargetOverlayController {
        get; private set;
    }
    public static ControlWheelController ControlWheelController {
        get; private set;
    }
    public static MessageOverlayController MessageOverlayController {
        get; private set;
    }
    public static HelpOverlayController HelpOverlayController {
        get; private set;
    }

    void Awake() {
        HudGameObject = gameObject;
        fPSText = GetComponentInChildren<Text>();
        hudGroup = GetComponent<CanvasGroup>();
        hudBehindWheelGroup = transform.Find("BehindControlWheel").GetComponent<CanvasGroup>();
        anim = GetComponent<Animator>();
        Crosshair = transform.Find("BehindControlWheel/Crosshair").GetComponent<Image>();
        BurnPercentageMeter = GetComponentInChildren<BurnPercentageMeter>();
        TargetOverlayController = GetComponentInChildren<TargetOverlayController>();
        ThrowingAmmoMeter = GetComponentInChildren<ThrowingAmmoMeter>();
        MessageOverlayController = GetComponentInChildren<MessageOverlayController>();
        HelpOverlayController = GetComponentInChildren<HelpOverlayController>();
        MetalReserveMeters = GetComponentInChildren<MetalReserveMeters>();
        ZincMeterController = GetComponentInChildren<ZincMeterController>();
        ControlWheelController = GetComponentInChildren<ControlWheelController>();
    }

	void LateUpdate() {
        deltaTimeFPS += (Time.unscaledDeltaTime - deltaTimeFPS) * 0.1f;
        if (SettingsMenu.settingsData.fpsCounter == 1) {
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

    // Ready HUD elements for a certain simulation
    public static void EnableHUD() {
        hudGroup.alpha = 1;
    }

    public static void DisableHUD() {
        hudGroup.alpha = 0;
    }

    // Clears the values currently on the HUD
    public static void ResetHUD() {
        if (SettingsMenu.settingsData.hudEnabled == 1) {
            EnableHUD();
            if (BurnPercentageMeter) {
                BurnPercentageMeter.Clear();
                TargetOverlayController.Clear();
                ThrowingAmmoMeter.Clear();
                MetalReserveMeters.Clear();
                MessageOverlayController.Clear();
                ZincMeterController.Clear();
                ControlWheelController.Clear();
                anim.SetBool("ControlWheelVisible", false);
            }
        }
    }

    public static void UpdateText() {
        HelpOverlayController.UpdateText();
        ControlWheelController.UpdateText();
    }

    // Returns a string reading a single Force in Newtons or G's
    public static string ForceString(float force, float mass, int sigFigs = 2) {
        if(SettingsMenu.settingsData.forceUnits == 1) {
            return RoundStringToSigFigs(force).ToString() + "N";
        } else {
            return RoundStringToSigFigs(force / mass / 9.81f, sigFigs) + "G's";
        }
    }

    // Returns a string reading a single Mass in kilograms
    public static string MassString(float mass) {
        return (mass).ToString() + "kg";
    }

    // Returns a string reading the sum of an Allomantic Force and Allomantic Normal Force in Newtons or G's
    // e.g. "650N + 250N"
    public static string AllomanticSumString(Vector3 allomanticForce, Vector3 normalForce, float mass, int sigFigs = 2, bool invert = false) {
        string plusSign;
        if (invert && Vector3.Dot(allomanticForce, normalForce) <= 0 || !invert && Vector3.Dot(allomanticForce, normalForce) >= 0) {
            plusSign = TextCodes.Blue("+");
        } else {
            plusSign = TextCodes.Red("-");
        }

        if (SettingsMenu.settingsData.forceUnits == 1) {
            return RoundStringToSigFigs(allomanticForce.magnitude, sigFigs) + " " + plusSign + " " + RoundStringToSigFigs(normalForce.magnitude, sigFigs) + "N";
        } else {
            return RoundStringToSigFigs(allomanticForce.magnitude / mass / 9.81f, sigFigs)
                + " " + plusSign + " " +
                RoundStringToSigFigs(normalForce.magnitude / mass / 9.81f, sigFigs) + "G's";
        }
    }

    // Rounds a float to the given sig figs, returning a string
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

    // Fade HUD in/out with Control Wheel
    public static void ShowControlWheel() {
        anim.SetBool("ControlWheelVisible", true);
    }
    public static void HideControlWheel(bool noTransition = false) {
        anim.SetBool("ControlWheelVisible", false);
        if(noTransition) {
            anim.Play("HUD_HideControlWheel", anim.GetLayerIndex("Visibility"));
        }
    }

    // Clear unwanted fields after changing settings
    public void InterfaceRefresh() {
        BurnPercentageMeter.InterfaceRefresh();
        TargetOverlayController.InterfaceRefresh();
    }

    // Used by Settings to make HUD visible or invisible
    public void ShowHUD() {
        hudGroup.alpha = 1;
    }

    public void HideHUD() {
        hudGroup.alpha = 0;
    }
}
