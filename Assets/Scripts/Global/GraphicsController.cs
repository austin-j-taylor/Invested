using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

/// <summary>
/// Manages Graphics and Video settings.
/// </summary>
public class GraphicsController : MonoBehaviour {

    public static GraphicsController instance = null;

    public static bool CloudsEnabled { get; private set; }
    private static bool postProcessingEnabled;
    // Properties for whether each setting should be enabled
    private static bool Bloom => postProcessingEnabled && SettingsMenu.settingsGraphics.bloom == 1;
    private static bool Antialiasing => postProcessingEnabled && SettingsMenu.settingsGraphics.antialiasing == 1;
    private static bool AmbientOcclusion => postProcessingEnabled && SettingsMenu.settingsGraphics.ambientOcclusion == 1;
    private static bool MotionBlur => postProcessingEnabled && SettingsMenu.settingsGraphics.motionBlur == 1;
    private static bool Aberration => postProcessingEnabled && SettingsMenu.settingsGraphics.aberration == 1;
    private static bool Vignette => postProcessingEnabled && SettingsMenu.settingsGraphics.vignetteZinc == 1;

    [SerializeField]
    private PostProcessingProfile profile = null;
    [SerializeField]
    public DropdownSetting resolutionDropdown = null, qualityDropdown = null;

    ChromaticAberrationModel.Settings aberrationSettings;
    VignetteModel.Settings vignetteSettings;

    private void Awake() {
        instance = this;

        // Need to set the Resolution Setting slider with the valid resolutions for this monitor
        Resolution[] resolutions = Screen.resolutions;
        List<OptionData> resOptions = new List<OptionData>();
        for (int i = 0; i < resolutions.Length; i++) {
            resOptions.Add(new OptionData() {
                text = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate,
                image = null
            });
        }
        resolutionDropdown.dropdown.options = resOptions;

        // Need to set the Quality Setting dropdown with the valid resolutions for this monitor
        string[] names = QualitySettings.names;
        List<OptionData> qualityOptions = new List<OptionData>();
        for (int i = 0; i < names.Length; i++) {
            qualityOptions.Add(new OptionData() {
                text = names[i],
                image = null
            });
        }
        qualityDropdown.dropdown.options = qualityOptions;
    }
    void Start() {
        postProcessingEnabled = SettingsMenu.settingsGraphics.postProcessingEnabled == 1;
        if (postProcessingEnabled) {
            SetBloom(SettingsMenu.settingsGraphics.bloom == 1);
            SetAntialiasing(SettingsMenu.settingsGraphics.antialiasing == 1);
            SetAmbientOcclusion(SettingsMenu.settingsGraphics.ambientOcclusion == 1);
            SetMotionBlur(SettingsMenu.settingsGraphics.motionBlur == 1);
            SetAberration(false);
            SetVignette(false);
        } else {
            SetPostProcessing(false);
        }

        CloudsEnabled = SettingsMenu.settingsGraphics.clouds == 1;

        aberrationSettings = profile.chromaticAberration.settings;
        vignetteSettings = profile.vignette.settings;
    }

    #region accessorsGraphics
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
    /// <summary>
    /// Sets the Zinc effect depending on the desired intensity
    /// </summary>
    /// <param name="enable">whether the Zinc effect should actually be enabled</param>
    /// <param name="intensity">the intensity of the effect [0,1]</param>
    /// <returns></returns>
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
    #endregion

    #region accessorsVideo
    /// <summary>
    /// Change the resolution or fullscreen-ness of the application
    /// </summary>
    /// <param name="index">the index of the resolution, depending on the monitor. e.g. index=0 is usually 800x600</param>
    /// <param name="mode">the fullscreen/windowed mode</param>
    public static void SetFullscreenResolution(int index, FullScreenMode mode) {
        Resolution[] resolutions = Screen.resolutions;
        // Interpret an index of -1 to be the max possible resolution
        if (index == -1 || index >= resolutions.Length)
            index = resolutions.Length - 1;
        // Check if a change has occurred
        if (Screen.currentResolution.width != resolutions[index].width ||
            Screen.currentResolution.height != resolutions[index].height ||
            Screen.currentResolution.refreshRate != resolutions[index].refreshRate ||
            mode != Screen.fullScreenMode) {
            //Debug.Log("Set resolution: " + resolutions[index].width + " " + resolutions[index].height + " " + mode);
            Screen.SetResolution(resolutions[index].width, resolutions[index].height, mode, resolutions[index].refreshRate);
        }
    }

    /// <summary>
    /// Change the quality level of the application's grpahics
    /// </summary>
    /// <param name="quality">the new quality level</param>
    public static void SetQualityLevel(int quality) {
        string[] names = QualitySettings.names;
        // Interpret an index of -1 to be the max possible quality
        if (quality == -1 || quality >= names.Length)
            quality = names.Length - 1;
        // If a change has occurred
        if (quality != QualitySettings.GetQualityLevel())
            QualitySettings.SetQualityLevel(quality, true);
    }
    #endregion
}
