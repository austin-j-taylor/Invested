using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class GraphicsController : MonoBehaviour {

    [SerializeField]
    PostProcessingProfile profile = null;

	// Use this for initialization
	void Start () {
        profile.bloom.enabled = SettingsMenu.settingsData.bloom == 1;
        profile.antialiasing.enabled = SettingsMenu.settingsData.antialiasing == 1;
        profile.ambientOcclusion.enabled = SettingsMenu.settingsData.ambientOcclusion == 1;
        profile.motionBlur.enabled = SettingsMenu.settingsData.motionBlur == 1;
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

}
