using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class GraphicsController : MonoBehaviour {

    private const float a = 2f;
    private const float b = 42;
    private const float c = 0.67f;
    private const float d = 11f;
    private const float f = 2.4f;
    private const float g = 72f;

    [SerializeField]
    PostProcessingProfile profile = null;

    ChromaticAberrationModel.Settings aberrationSettings;
    VignetteModel.Settings vignetteSettings;

    // Use this for initialization
    void Start () {
        profile.bloom.enabled = SettingsMenu.settingsData.bloom == 1;
        profile.antialiasing.enabled = SettingsMenu.settingsData.antialiasing == 1;
        profile.ambientOcclusion.enabled = SettingsMenu.settingsData.ambientOcclusion == 1;
        profile.motionBlur.enabled = SettingsMenu.settingsData.motionBlur == 1;

        aberrationSettings = profile.chromaticAberration.settings;
        vignetteSettings = profile.vignette.settings;
    }

    public void SetAntialiasing(bool enable) {
        profile.antialiasing.enabled = enable;
    }

    public void SetAmbientOcclusion(bool enable) {
        profile.ambientOcclusion.enabled = enable;
    }

    public void SetBloom(bool enable) {
        profile.bloom.enabled = enable;
    }

    public void SetMotionBlur(bool enable) {
        profile.motionBlur.enabled = enable; 
    }

    public void SetZincEffect(bool enable, float x = 1, float S = 1) {
        profile.chromaticAberration.enabled = enable;
        profile.vignette.enabled = enable;
        if (enable) {
            // hot formula that makes a nice curve
            //float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f * (x - 1)) - Mathf.Exp(g * (x - 1)));
            float intensity = a * (-Mathf.Exp(-b * x) + Mathf.Exp(-d * x)) + c * (Mathf.Exp(f / S * (x - S)) - Mathf.Exp(g * (x - S)));
            aberrationSettings.intensity = intensity;
            vignetteSettings.intensity = intensity / 4;
            profile.chromaticAberration.settings = aberrationSettings;
            profile.vignette.settings = vignetteSettings;
        }
    }

}
