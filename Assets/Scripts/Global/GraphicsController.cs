using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class GraphicsController : MonoBehaviour {

    private static GraphicsController instance = null;

    public static bool CloudsEnabled { get; private set; }

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
        profile.chromaticAberration.enabled = SettingsMenu.settingsData.aberration == 1;
        profile.vignette.enabled = SettingsMenu.settingsData.vignetteZinc == 1;
        CloudsEnabled = SettingsMenu.settingsData.clouds == 1;

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
    public static void SetAberration(bool enable) {
        instance.profile.chromaticAberration.enabled = enable;
    }
    public static void SetVignette(bool enable) {
        instance.profile.vignette.enabled = enable;
    }
    public static void SetClouds(bool enable) {
        CloudsEnabled = enable;
        CameraController.SetClouds(enable);
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
