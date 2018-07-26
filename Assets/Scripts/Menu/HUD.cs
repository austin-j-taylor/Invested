using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the heads-up display
 */
public class HUD : MonoBehaviour {

    private static Text coinCountText;

    public static string CoinCountText {
        set {
            coinCountText.text = value;
        }
    }

    public static BurnRateMeter BurnRateMeter {
        get; private set;
    }

    void Awake() {
        coinCountText = GetComponentsInChildren<Text>()[0];
        BurnRateMeter = GetComponentInChildren<BurnRateMeter>();
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
            coinCountText.text = "50";
        }
    }
}
