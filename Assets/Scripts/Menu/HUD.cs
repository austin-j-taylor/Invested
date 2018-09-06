using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the heads-up display
 */
public class HUD : MonoBehaviour {

    public static readonly Color weakBlue = new Color(0, .75f, 1, 1);
    public static readonly Color strongBlue = new Color(0, 1f, 1, 1);

    private static Text coinCountText;

    private float deltaTimeFPS = 0.0f;
    private static GameObject hudGameObject;

    public static string CoinCountText {
        set {
            coinCountText.text = value;
        }
    }

    public static BurnRateMeter BurnRateMeter {
        get; private set;
    }
    public static TargetOverlayController TargetOverlayController {
        get; private set;
    }

    void Awake() {
        hudGameObject = gameObject;
        coinCountText = GetComponentsInChildren<Text>()[0];
        BurnRateMeter = GetComponentInChildren<BurnRateMeter>();
        TargetOverlayController = GetComponentInChildren<TargetOverlayController>();
    }

	void Update() {
        deltaTimeFPS += (Time.unscaledDeltaTime - deltaTimeFPS) * 0.1f;
	}

	void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
 
		GUIStyle style = new GUIStyle();
 
		Rect rect = new Rect(0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.75f, 0.75f, 0.75f, 1.0f);
		float fps = 1.0f / deltaTimeFPS;
		string text = string.Format("{0:0.} fps", fps);
		GUI.Label(rect, text, style);
	}

    public static void EnableHUD() {
        hudGameObject.SetActive(true);
    }

    public static void DisableHUD() {
        hudGameObject.SetActive(false);
    }

    // Clears the values currently on the HUD
    public static void ResetHUD() {
        EnableHUD();
        if (BurnRateMeter) {
            BurnRateMeter.Clear();
            TargetOverlayController.Clear();
            coinCountText.text = "50";
        }
    }

    // Returns a string reading a single Force in Newtons or G's
    public static string ForceString(float force, float mass) {
        if(SettingsMenu.settingsData.forceUnits == 1) {
            return RoundToTwoSigFigs(force).ToString() + "N";
        } else {
            return System.Math.Round(force / mass / 9.81, 2).ToString() + "G's";
        }
    }

    // Returns a string reading a single Mass in kilograms
    public static string MassString(float mass) {
        return (mass).ToString() + "kg";
    }

    // Returns a string reading the sum of an Allomantic Force and Allomantic Normal Force in Newtons or G's
    // e.g. "650N + 250N"
    public static string AllomanticSumString(Vector3 allomanticForce, Vector3 normalForce, float mass, bool invert = false) {
        string plusSign;
        plusSign = DecideSignColor(allomanticForce, normalForce, invert);

        if (SettingsMenu.settingsData.forceUnits == 1) {
            return ((int)allomanticForce.magnitude).ToString() + " " + plusSign + " " + ((int)normalForce.magnitude).ToString() + "N";
        } else {
            return System.Math.Round(allomanticForce.magnitude / mass / 9.81, 2).ToString()
                + " " + plusSign + " " +
                System.Math.Round(normalForce.magnitude / mass / 9.81, 2).ToString() + "G's";
        }
    }
    // Rounds an integer to two sig figs
    private static int RoundToTwoSigFigs(float num) {
        int rounded = (int)num;
        if (num < 100)
            return rounded;
        int newbase = (int)System.Math.Pow(10, ((int)Mathf.Log10(num) - 1));

        return (rounded / newbase) * newbase;
    }

    // Returns a blue plus sign if the vectors point in the same direction and a red minus sign if they point in opposite directions
    private static string DecideSignColor(Vector3 allomanticForce, Vector3 normalForce, bool invert) {
        if (invert && Vector3.Dot(allomanticForce, normalForce) <= 0 || !invert && Vector3.Dot(allomanticForce, normalForce) >= 0) {
            return "<color=#0080ff>+</color>";
        } else {
            return "<color=#ff0000>-</color>";
        }
    }
}
