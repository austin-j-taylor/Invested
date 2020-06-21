using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;

public class GraphicsController : MonoBehaviour {

    private static GraphicsController instance = null;

    public static bool CloudsEnabled { get; private set; }
    private static bool postProcessingEnabled;
    // Properties for whether each setting should be enabled
    private static bool Bloom => postProcessingEnabled && SettingsMenu.settingsData.bloom == 1;
    private static bool Antialiasing => postProcessingEnabled && SettingsMenu.settingsData.antialiasing == 1;
    private static bool AmbientOcclusion => postProcessingEnabled && SettingsMenu.settingsData.ambientOcclusion == 1;
    private static bool MotionBlur => postProcessingEnabled && SettingsMenu.settingsData.motionBlur == 1;
    private static bool Aberration => postProcessingEnabled && SettingsMenu.settingsData.aberration == 1;
    private static bool Vignette => postProcessingEnabled && SettingsMenu.settingsData.vignetteZinc == 1;

    [SerializeField]
    private PostProcessingProfile profile = null;
    [SerializeField]
    private SliderSetting resolutionSlider = null;

    ChromaticAberrationModel.Settings aberrationSettings;
    VignetteModel.Settings vignetteSettings;

    private void Awake() {
        instance = this;
    }
    void Start() {
        postProcessingEnabled = SettingsMenu.settingsData.postProcessingEnabled == 1;
        if (postProcessingEnabled) {
            SetBloom(SettingsMenu.settingsData.bloom == 1);
            SetAntialiasing(SettingsMenu.settingsData.antialiasing == 1);
            SetAmbientOcclusion(SettingsMenu.settingsData.ambientOcclusion == 1);
            SetMotionBlur(SettingsMenu.settingsData.motionBlur == 1);
            SetAberration(false);
            SetVignette(false);
        } else {
            SetPostProcessing(false);
        }

        CloudsEnabled = SettingsMenu.settingsData.clouds == 1;

        aberrationSettings = profile.chromaticAberration.settings;
        vignetteSettings = profile.vignette.settings;

        // Need to set the Resolution Setting slider with the valid resolutions for this monitor
        resolutionSlider.updateTextWhenChanged = false;
        resolutionSlider.slider.onValueChanged.AddListener(RefreshResolutionSlider);
        RefreshResolutionSlider(SettingsMenu.settingsData.resolution);
    }

    public static void SetPostProcessing(bool enable) {
        postProcessingEnabled = enable;
        SetBloom(enable);
        SetAntialiasing(enable);
        SetAmbientOcclusion(enable);
        SetMotionBlur(enable);
        if (!enable) {
            SetAberration(false);
            SetVignette(false);
        }
    }
    public static void SetBloom(bool enable) {
        instance.profile.bloom.enabled = enable && Bloom;
    }
    public static void SetAntialiasing(bool enable) {
        instance.profile.antialiasing.enabled = enable && Antialiasing;
    }
    public static void SetAmbientOcclusion(bool enable) {
        instance.profile.ambientOcclusion.enabled = enable && AmbientOcclusion;
    }
    public static void SetMotionBlur(bool enable) {
        instance.profile.motionBlur.enabled = enable && MotionBlur;
    }
    // Doesn't actually toggle aberration/vignette on and off; just gives then the option of doing so in zinc time
    public static void SetAberration(bool enable) {
        if (!enable)
            instance.profile.chromaticAberration.enabled = false;
    }
    public static void SetVignette(bool enable) {
        if (!enable)
            instance.profile.vignette.enabled = false;
    }
    public static void SetClouds(bool enable) {
        CloudsEnabled = enable;
        GameManager.CloudsManager.SetClouds(enable);
    }

    // The "resolution" is a value in [0.0, 0.9999] that corresponds to a resolution that the monitor can handle
    public static void SetFullscreenResolution(float value, FullScreenMode mode) {
        Resolution[] resolutions = Screen.resolutions;
        int index = (int)(value * resolutions.Length);
        if (index >= resolutions.Length)
            index = resolutions.Length - 1;
        if (Screen.currentResolution.width != resolutions[index].width ||
            Screen.currentResolution.height != resolutions[index].height ||
            mode != Screen.fullScreenMode) {
#if UNITY_EDITOR
            //Debug.Log("Set resolution: " + resolutions[index].width + " " + resolutions[index].height + " " + mode);
#else
            Screen.SetResolution(resolutions[index].width, resolutions[index].height, mode);
#endif
        }
    }

    public static void RefreshResolutionSlider(float value) {
        Resolution[] resolutions = Screen.resolutions;
        int index = (int)(value * resolutions.Length);
        if (index >= resolutions.Length)
            index = resolutions.Length - 1;
        instance.resolutionSlider.valueText.text = resolutions[index].width + " x " + resolutions[index].height;
    }

    // returns intensity
    public float SetZincEffect(bool enable, float intensity = 0) {
        if (HUD.ControlWheelController.IsOpen) {
            //enable = false; // If Control Wheel is open, don't show the effect
            intensity = intensity / 3;
        }
        instance.profile.chromaticAberration.enabled = enable && Aberration;
        instance.profile.vignette.enabled = enable && Vignette;

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
