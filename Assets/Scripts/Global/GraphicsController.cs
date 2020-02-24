using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class GraphicsController : MonoBehaviour {


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
        profile.chromaticAberration.enabled = false;
        profile.vignette.enabled = false;

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

    // returns intensity
    public float SetZincEffect(bool enable, float intensity = 0) {
        if (HUD.ControlWheelController.IsOpen) {
            //enabled = false; // If Control Wheel is open, don't show the effect
            intensity = intensity / 3;
        }
        profile.chromaticAberration.enabled = enable;
        profile.vignette.enabled = enable;
        
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
