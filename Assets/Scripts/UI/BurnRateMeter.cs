using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BurnRateMeter : MonoBehaviour {

    // Constants for Burn Rate Meter
    private const float burnRateMeterLerpConstant = .30f;
    private const float minAngle = .12f;
    private const float maxAngle = 1f - 2 * minAngle;

    [SerializeField]
    private Text metalLineText;
    [SerializeField]
    private Text forceMagnitudeTargetText;
    [SerializeField]
    private Text maximumForceMagnitudeText;
    private Image burnRateImage;

    public string MetalLineText {
        set {
            metalLineText.text = value;
        }
    }

    void Start () {
        burnRateImage = GetComponent<Image>();
        metalLineText.text = "";
        burnRateImage.color = new Color(0, .5f, 1, .75f);
        burnRateImage.fillAmount = minAngle;
        forceMagnitudeTargetText.text = "";
        maximumForceMagnitudeText.text = "";
    }

    public void SetBurnRateMeterForceMagnitude(float forceMagnitudeTarget, float maximumForceMagnitude) {
        float percent = 0;
        if (maximumForceMagnitude != 0) {
            percent = forceMagnitudeTarget / maximumForceMagnitude;
        }
        if (forceMagnitudeTarget < .01f) {
            forceMagnitudeTargetText.text = "";
            maximumForceMagnitudeText.text = "";
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 0, burnRateMeterLerpConstant);
        } else if (percent > .99f) {
            forceMagnitudeTargetText.text = ((int)forceMagnitudeTarget).ToString() + "N";
            maximumForceMagnitudeText.text = ((int)maximumForceMagnitude).ToString() + "N";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 1, burnRateMeterLerpConstant);
        } else {
            forceMagnitudeTargetText.text = ((int)forceMagnitudeTarget).ToString() + "N";
            maximumForceMagnitudeText.text = ((int)maximumForceMagnitude).ToString() + "N";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, minAngle + (percent) * (maxAngle), burnRateMeterLerpConstant);
        }
    }
    public void SetBurnRateMeterPercentage(float ironBurnRate, float steelBurnRate, float maximumForceMagnitude) {
        float rate = Mathf.Max(ironBurnRate, steelBurnRate);
        int percent = (int)Mathf.Round(rate * 100);
        if (percent == 0) {
            forceMagnitudeTargetText.text = "";
            maximumForceMagnitudeText.text = "";
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 0, burnRateMeterLerpConstant);
        } else if (percent > 99) {
            forceMagnitudeTargetText.text = "MAX";
            maximumForceMagnitudeText.text = ((int)maximumForceMagnitude).ToString() + "N";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 1, burnRateMeterLerpConstant);
        } else {
            forceMagnitudeTargetText.text = percent + "%";
            maximumForceMagnitudeText.text = ((int)maximumForceMagnitude).ToString() + "N";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, minAngle + (rate) * (maxAngle), burnRateMeterLerpConstant);
        }
    }
}
