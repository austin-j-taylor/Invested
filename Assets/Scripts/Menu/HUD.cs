using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the heads-up display
 */
public class HUD : MonoBehaviour {

    public static readonly Color weakBlue = new Color(0, .75f, 1, 1);
    public static readonly Color strongBlue = new Color(0, 1f, 1, 1);

    private static Text coinCountText;
    private static AllomanticIronSteel playerIronSteel;


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
        playerIronSteel = GameObject.FindGameObjectWithTag("Player").GetComponent<AllomanticIronSteel>();
        coinCountText = GetComponentsInChildren<Text>()[0];
        BurnRateMeter = GetComponentInChildren<BurnRateMeter>();
        TargetOverlayController = GetComponentInChildren<TargetOverlayController>();
    }

    public void EnableHUD() {
        gameObject.SetActive(true);
    }

    public void DisableHUD() {
        gameObject.SetActive(false);
    }

    // Clears the values currently on the HUD
    public void ResetHUD() {
        EnableHUD();
        if (BurnRateMeter) {
            BurnRateMeter.Clear();
            TargetOverlayController.Clear();
            coinCountText.text = "50";
        }
    }

    // Returns a string reading a single Force in Newtons or G's
    public static string ForceString(float force) {
        switch (SettingsMenu.displayUnits) {
            case ForceDisplayUnits.Newtons: {
                    return RoundToTwoSigFigs(force).ToString() + "N";
                }
            default: {
                    return System.Math.Round(force / playerIronSteel.Mass / 9.81, 2).ToString() + "G's";
                }
        }
    }

    // Returns a string reading a single Mass in kilograms
    public static string MassString(float mass) {
        return (mass).ToString() + "kg";
    }

    // Returns a string reading the sum of an Allomantic Force and Allomantic Normal Force in Newtons or G's
    // e.g. "650N + 250N"
    public static string AllomanticSumString(Vector3 allomanticForce, Vector3 normalForce, bool invert = false) {
        string plusSign;
        plusSign = DecideSignColor(allomanticForce, normalForce, invert);

        switch (SettingsMenu.displayUnits) {
            case ForceDisplayUnits.Newtons: {
                    return ((int)allomanticForce.magnitude).ToString() + " " + plusSign + " " + ((int)normalForce.magnitude).ToString() + "N";
                }
            default: {
                    return System.Math.Round(allomanticForce.magnitude / playerIronSteel.Mass / 9.81, 2).ToString()
                        + " " + plusSign + " " +
                        System.Math.Round(normalForce.magnitude / playerIronSteel.Mass / 9.81, 2).ToString() + "G's";
                }
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
