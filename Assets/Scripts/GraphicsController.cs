using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class GraphicsController : MonoBehaviour {

    [SerializeField]
    PostProcessingProfile profile;

	// Use this for initialization
	void Start () {
        profile.bloom.enabled = SettingsMenu.settingsData.bloom == 1;
        profile.antialiasing.enabled = SettingsMenu.settingsData.antialiasing == 1;
        profile.ambientOcclusion.enabled = SettingsMenu.settingsData.ambientOcclusion == 1;
        profile.motionBlur.enabled = SettingsMenu.settingsData.motionBlur == 1;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}