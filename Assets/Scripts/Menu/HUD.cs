using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    void Start() {
        coinCountText = GetComponentsInChildren<Text>()[0];
        BurnRateMeter = GetComponentInChildren<BurnRateMeter>();
    }

    public void EnableHUD() {
        gameObject.SetActive(true);
    }

    public void DisableHUD() {
        gameObject.SetActive(false);
    }

    public void ResetHUD() {
        if (BurnRateMeter) {
            BurnRateMeter.Clear();
            coinCountText.text = "50";
        }
    }
}
