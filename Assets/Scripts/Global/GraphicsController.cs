using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class GraphicsController : MonoBehaviour {

    private static GraphicsController instance = null;

    [SerializeField]
    PostProcessingProfile profile = null;

    ChromaticAberrationModel.Settings aberrationSettings;
    VignetteModel.Settings vignetteSettings;

    private void Awake() {
        instance = this;
    }
    void Start () {
        profile.bloom.enabled = SettingsMenu.settingsData.bloom == 1;
        profile.antialiasing.enabled = SettingsMenu.settingsData.antialiasing == 1;
        profile.ambientOcclusion.enabled = SettingsMenu.settingsData.ambientOcclusion == 1;
        profile.motionBlur.enabled = SettingsMenu.settingsData.motionBlur == 1;
        profile.chromaticAberration.enabled = false;
        profile.vignette.enabled = false;

        aberrationSettings = profile.chromaticAberration.settings;
        vignetteSettings = profile.vignette.settings;
    }

    public static void SetAntialiasing(bool enable) {
        instance.profile.antialiasing.enabled = enable;
    }

    public static void SetAmbientOcclusion(bool enable) {
        instance.profile.ambientOcclusion.enabled = enable;
    }

    public static void SetMotionBlur(bool enable) {
        instance.profile.motionBlur.enabled = enable;
    }

    public static void SetBloom(bool enable) {
        instance.profile.bloom.enabled = enable;
    }

    // returns intensity
    public float SetZincEffect(bool enable, float intensity = 0) {
        if (HUD.ControlWheelController.IsOpen) {
            //enabled = false; // If Control Wheel is open, don't show the effect
            intensity = intensity / 3;
        }
        profile.chromaticAberration.enabled = enable && (SettingsMenu.settingsData.aberration == 1);
        profile.vignette.enabled = enable && (SettingsMenu.settingsData.vignetteZinc == 1);
        
        if (enable) {
            aberrationSettings.intensity = intensity;
            vignetteSettings.intensity = intensity / 3;
            profile.chromaticAberration.settings = aberrationSettings;
            profile.vignette.settings = vignetteSettings;
            return intensity;
        }
        return 0;
    }

}
