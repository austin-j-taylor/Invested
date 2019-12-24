using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the circular Burn Percentage Meter.
 */
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
        if (SettingsMenu.settingsData.forceComplexity == 0) {
            sumForceText.text = "";
        }
    }

    /* 
     * Set the meter using the Force Magnitude or Force Percentage display configuration, depending on the targetForce argument.
     *  targetForce only appears in the playerInputText field.
     *  The force calculated from rate as a % of the net force is used for the blue circle bar and the ActualForceText field.
     */
    public void SetBurnRateMeterForceMagnitude(Vector3 allomanticForce, Vector3 normalForce, float rate, float rateAlternate, float targetForce) {
        playerInputText.text = HUD.ForceString(targetForce, Player.PlayerIronSteel.Mass);

        SetActualForceText((allomanticForce + normalForce).magnitude);
        SetSumForceText(allomanticForce, normalForce);
        SetFillPercent(rate, rateAlternate);
    }

    // Set the meter using the Percentage display configuration.
    public void SetBurnRateMeterPercentage(Vector3 allomanticForce, Vector3 normalForce, float rate, float rateAlternate) {
        playerInputText.text = (int)Mathf.Round(Mathf.Max(rate, rateAlternate) * 100) + "%";

        SetActualForceText((allomanticForce + normalForce).magnitude);
        SetSumForceText(allomanticForce, normalForce);
        SetFillPercent(rate, rateAlternate);
    }

    private void SetActualForceText(float forceActual) {
        actualForceText.text = HUD.ForceString(forceActual, Player.PlayerIronSteel.Mass);
    }

    private void SetSumForceText(Vector3 allomanticForce, Vector3 normalForce) {
        if (SettingsMenu.settingsData.forceComplexity == 1) {
            float allomanticMagnitude = allomanticForce.magnitude;
            float normalMagnitude = normalForce.magnitude;
            sumForceText.text = HUD.AllomanticSumString(allomanticForce, normalForce, Player.PlayerIronSteel.Mass);
        }
    }

    private void SetFillPercent(float rate, float rateAlternate) {
        HUD.Crosshair.SetFillPercent(rate);
    }

    public void SetForceTextColorStrong() {
        actualForceText.color = HUD.strongBlue;
    }

    public void SetForceTextColorWeak() {
        actualForceText.color = HUD.weakBlue;
    }

    public void SetMetalLineCountTextManual(int sizeOfTargetArrays) {
        metalLineCountText.text = sizeOfTargetArrays.ToString();
    }
    public void SetMetalLineCountTextArea(float radius) {
        metalLineCountText.text = HUD.RoundStringToSigFigs(radius) + "%";
    }
    public void SetMetalLineCountTextBubble(float radius) {
        metalLineCountText.text = HUD.RoundStringToSigFigs(radius) + "m";
    }
}
