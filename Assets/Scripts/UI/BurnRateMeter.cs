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
    
    private AllomanticIronSteel playerIronSteel;
    private Text metalLineText;
    private Text forceMagnitudeTargetText;
    private Text maximumForceMagnitudeText;
    private Image burnRateImage;

    public string MetalLineText {
        set {
            metalLineText.text = value;
        }
    }

    void Awake() {
        playerIronSteel = GameObject.FindGameObjectWithTag("Player").GetComponent<AllomanticIronSteel>();
        Text[] texts = GetComponentsInChildren<Text>();
        metalLineText = texts[0];
        forceMagnitudeTargetText = texts[1];
        maximumForceMagnitudeText = texts[2];

        burnRateImage = GetComponent<Image>();
        burnRateImage.color = new Color(0, .5f, 1, .75f);
        Clear();
    }

    // Set the meter using the Force Magnitude display configuration.
    public void SetBurnRateMeterForceMagnitude(float forceMagnitudeTarget, float maximumForceMagnitude) {
        float percent = 0;
        if (maximumForceMagnitude != 0) {
            percent = forceMagnitudeTarget / maximumForceMagnitude;
        }
        if (forceMagnitudeTarget < .01f) {
            forceMagnitudeTargetText.text = "";
            maximumForceMagnitudeText.text = "";
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 0, burnRateMeterLerpConstant);
        } else {
            if (PhysicsController.displayUnits == ForceDisplayUnits.Newtons) {
                forceMagnitudeTargetText.text = ((int)forceMagnitudeTarget).ToString() + "N";
                maximumForceMagnitudeText.text = ((int)maximumForceMagnitude).ToString() + "N";
            } else {
                forceMagnitudeTargetText.text = (System.Math.Round(forceMagnitudeTarget / playerIronSteel.Mass / 9.81, 2).ToString() + "G's");
                maximumForceMagnitudeText.text = (System.Math.Round(maximumForceMagnitude / playerIronSteel.Mass / 9.81, 2).ToString() + "G's");
            }
            if (percent > .99f) {
                burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 1, burnRateMeterLerpConstant);
            } else {
                burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, minAngle + (percent) * (maxAngle), burnRateMeterLerpConstant);
            }
        }
    }

    // Set the meter using the Percentage display configuration.
    public void SetBurnRateMeterPercentage(float ironBurnRate, float steelBurnRate, float maximumForceMagnitude) {
        float rate = Mathf.Max(ironBurnRate, steelBurnRate);
        int percent = (int)Mathf.Round(rate * 100);
        if (percent == 0) {
            forceMagnitudeTargetText.text = "";
            maximumForceMagnitudeText.text = "";
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 0, burnRateMeterLerpConstant);
        } else {

            if (PhysicsController.displayUnits == ForceDisplayUnits.Newtons) {
                forceMagnitudeTargetText.text = ((int)maximumForceMagnitude).ToString() + "N";
            } else {
                forceMagnitudeTargetText.text = (System.Math.Round(maximumForceMagnitude / playerIronSteel.Mass / 9.81, 2).ToString() + "G's");
            }

            if (percent > 99) {
                maximumForceMagnitudeText.text = "MAX";
                
                burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 1, burnRateMeterLerpConstant);
            } else {
                maximumForceMagnitudeText.text = percent + "%";
                
                burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, minAngle + (rate) * (maxAngle), burnRateMeterLerpConstant);
            }
        }
    }

    public void Clear() {
        metalLineText.text = "";
        burnRateImage.fillAmount = minAngle;
        forceMagnitudeTargetText.text = "";
        maximumForceMagnitudeText.text = "";
    }
}
