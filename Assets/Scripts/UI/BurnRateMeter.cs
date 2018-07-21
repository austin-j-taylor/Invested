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
    private Text burnRateMeterPercent;
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
        burnRateMeterPercent.text = "";
    }

    public void RefreshBurnRateMeter(float ironBurnRate, float steelBurnRate) {
        float rate = Mathf.Max(ironBurnRate, steelBurnRate);
        int percent = (int)Mathf.Round(rate * 100);
        if (percent == 0) {
            burnRateMeterPercent.text = "";
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 0, burnRateMeterLerpConstant);
        } else if (percent > 99) {
            burnRateMeterPercent.text = "MAX";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, 1, burnRateMeterLerpConstant);
        } else {
            burnRateMeterPercent.text = percent + "%";
            //burnRateMeterPercent.color = Color.Lerp(burnRateMeterPercent.color, new Color(1 - rate * rMaxMeter, 1f - gMaxMeter * rate, bMaxMeter, 1f), burnRateMeterLerpConstant);
            burnRateImage.fillAmount = Mathf.Lerp(burnRateImage.fillAmount, minAngle + (rate) * (maxAngle), burnRateMeterLerpConstant);
        }
    }
}
