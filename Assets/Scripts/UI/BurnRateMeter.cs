using UnityEngine;
using UnityEngine.UI;

/*
 * Controls the circular Burn Rate Meter.
 */
public class BurnRateMeter : MonoBehaviour {

    // Constants for Burn Rate Meter
    private const float burnRateMeterLerpConstant = .30f;
    private const float minAngle = .12f;
    private const float maxAngle = 1f - 2 * minAngle;
    
    private Text actualForceText;
    private Text sumForceText;
    private Text playerInputText;
    private Text metalLineCountText;
    private Image burnRateImage;

    public string MetalLineText {
        set {
            metalLineCountText.text = value;
        }
    }

    void Awake() {
        Text[] texts = GetComponentsInChildren<Text>();
        actualForceText = texts[0];
        playerInputText = texts[1];
        sumForceText = texts[2];
        metalLineCountText = texts[3];

        burnRateImage = GetComponent<Image>();
        burnRateImage.color = burnRateImage.color;
        Clear();
    }

    // Set the meter using the Force Magnitude display configuration.
    public void SetBurnRateMeterForceMagnitude(Vector3 allomanticForce, Vector3 normalForce, float targetForce) {
        float netForce = (allomanticForce + normalForce).magnitude;
        float percent = 0;
        if (netForce != 0) {
            percent = targetForce / (netForce);
        }
        if(percent < 1f) {
            SetActualForceText(targetForce);
            SetSumForceText(allomanticForce * percent, normalForce * percent);
        } else { // trying to push at a magnitude you can't reach
            SetActualForceText(netForce);
            SetSumForceText(allomanticForce, normalForce);
        }

        playerInputText.text = HUD.ForceString(targetForce);
        LerpToPercent(percent);
    }

    // Set the meter using the Percentage display configuration.
    public void SetBurnRateMeterPercentage(Vector3 allomanticForce, Vector3 normalForce, float rate) {
        float netForce = (allomanticForce + normalForce).magnitude;
        int percent = (int)Mathf.Round(rate * 100);
        playerInputText.text = percent + "%";

        LerpToPercent(rate);
        SetActualForceText(netForce);
        SetSumForceText(allomanticForce, normalForce);
    }

    private void SetActualForceText(float forceActual) {
        actualForceText.text = HUD.ForceString(forceActual);
    }

    private void SetSumForceText(Vector3 allomanticForce, Vector3 normalForce) {
        float allomanticMagnitude = allomanticForce.magnitude;
        float normalMagnitude = normalForce.magnitude;
        sumForceText.text = HUD.AllomanticSumString(allomanticForce, normalForce);
    }

    private void LerpToPercent(float percent) {
        burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, minAngle + (percent) * (maxAngle), burnRateMeterLerpConstant);
    }

    public void Clear() {
        actualForceText.text = "";
        playerInputText.text = "";
        sumForceText.text = "";
        metalLineCountText.text = "";
        burnRateImage.fillAmount = minAngle;
    }
}
