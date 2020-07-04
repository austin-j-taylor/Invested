using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the HUD element that shows the player's burn percentage and force acting on them.
/// </summary>
public class BurnPercentageMeter : MonoBehaviour {

    // Constants for Burn Rate Meter
    private const float minAngle = .12f;
    //private const float maxAngle = 1f - 2 * minAngle;
    private const float maxAngle = 0.3735f - minAngle;

    private Text actualForceText;
    private Text sumForceText;
    private Text playerInputText;
    private Text metalLineCountText;

    void Awake() {
        Text[] texts = GetComponentsInChildren<Text>();
        actualForceText = texts[0];
        playerInputText = texts[1];
        sumForceText = texts[2];
        metalLineCountText = texts[3];

        Clear();
    }

    public void Clear() {
        actualForceText.text = "";
        playerInputText.text = "";
        sumForceText.text = "";
        metalLineCountText.text = "";
    }

    // Hide unwanted fields after changing Settings
    public void InterfaceRefresh() {
        if (SettingsMenu.settingsInterface.forceComplexity == 0) {
            sumForceText.text = "";
        }
    }

    #region textSetters
    /// <summary>
    /// Set the meter using the Force Magnitude for Force Percentage display configuration
    /// </summary>
    /// <param name="allomanticForce">the Allomantic Force component of the force</param>
    /// <param name="normalForce">the Anchored Push Boost component of the forces</param>
    /// <param name="rate">the percentage of the force for iron</param>
    /// <param name="rateAlternate">the percentage of the force for steel</param>
    /// <param name="targetForce">the desired force for Magnitude mode</param>
    public void SetBurnRateMeterForceMagnitude(Vector3 allomanticForce, Vector3 normalForce, float rate, float rateAlternate, float targetForce) {
        playerInputText.text = HUD.ForceString(targetForce, Player.PlayerIronSteel.Mass);

        SetActualForceText((allomanticForce + normalForce).magnitude);
        SetSumForceText(allomanticForce, normalForce);
        SetFillPercent(rate, rateAlternate);
    }

    // 
    /// <summary>
    /// Set the meter using the Percentage display configuration.
    /// </summary>
    /// <param name="allomanticForce">the Allomantic Force component of the force</param>
    /// <param name="normalForce">the Anchored Push Boost component of the forces</param>
    /// <param name="rate">the percentage of the force for iron</param>
    /// <param name="rateAlternate">the percentage of the force for steel</param>
    public void SetBurnRateMeterPercentage(Vector3 allomanticForce, Vector3 normalForce, float rate, float rateAlternate) {
        playerInputText.text = (int)Mathf.Round(Mathf.Max(rate, rateAlternate) * 100) + "%";

        SetActualForceText((allomanticForce + normalForce).magnitude);
        SetSumForceText(allomanticForce, normalForce);
        SetFillPercent(rate, rateAlternate);
    }

    /// <summary>
    /// Sets the force text string
    /// </summary>
    /// <param name="forceActual">the force to set it to</param>
    private void SetActualForceText(float forceActual) {
        actualForceText.text = HUD.ForceString(forceActual, Player.PlayerIronSteel.Mass);
    }

    /// <summary>
    /// Sets the sum text string
    /// </summary>
    /// <param name="allomanticForce">the Allomantic Force component of the force</param>
    /// <param name="normalForce">the Anchored Push Boost component of the forces</param>
    private void SetSumForceText(Vector3 allomanticForce, Vector3 normalForce) {
        if (SettingsMenu.settingsInterface.forceComplexity == 1) {
            float allomanticMagnitude = allomanticForce.magnitude;
            float normalMagnitude = normalForce.magnitude;
            sumForceText.text = HUD.AllomanticSumString(allomanticForce, normalForce, Player.PlayerIronSteel.Mass);
        }
    }
    #endregion

    private void SetFillPercent(float rate, float rateAlternate) {
        HUD.Crosshair.SetFillPercent(rate);
    }

    public void SetForceTextColorStrong() {
        actualForceText.color = HUD.strongBlue;
    }
    public void SetForceTextColorWeak() {
        actualForceText.color = HUD.weakBlue;
    }

    public void SetMetalLineCountTextManual() {
        metalLineCountText.text = "";
    }
    public void SetMetalLineCountTextArea(float radius) {
        metalLineCountText.text = HUD.RoundStringToSigFigs(radius) + "%";
    }
    public void SetMetalLineCountTextBubble(float radius) {
        metalLineCountText.text = HUD.RoundStringToSigFigs(radius) + "m";
    }
}
