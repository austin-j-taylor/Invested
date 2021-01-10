using UnityEngine;
using System.Collections;
using System.IO;
using static UnityEngine.UI.Dropdown;
using System.Collections.Generic;

public class JSONSettings_Graphics : JSONSettings {

    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Graphics_default.json");

    public int quality;
    public int highlightedTargetOutline; // 0 for Disabled, 1 for Enabled
    public int pullTargetLineColor; // 0 for blue, 1 for light blue, 2 for green
    public int pushTargetLineColor; // 0 for blue, 1 for red
    public int renderblueLines; // 0 for Disabled, 1 for Enabled
    public int velocityZoom;
    public int postProcessingEnabled;
    public int antialiasing;
    public int ambientOcclusion;
    public int motionBlur;
    public int bloom;
    public int aberration;
    public int vignetteZinc;
    public int clouds;
    public float cloudParticleCount;

    protected override void Awake() {
        ConfigFileName = Path.Combine(Application.persistentDataPath, "Data", "Config", "config_Graphics.json");
        // Quick and dirty fix to add new settings to old config files
        settings = GetComponentsInChildren<Setting>();
        ResetToDefaults(false, false);
        LoadSettings(false);
        //base.Awake();

        Debug.Log("Loading: " + cloudParticleCount);
        SaveSettings();
    }

    /// <summary>
    /// Manually apply certain setting effects when they are changed
    /// </summary>
    public override void SetSettingsWhenChanged() {
        GraphicsController.SetQualityLevel(quality);
        GameManager.CloudsManager.SetParticleCount((int)cloudParticleCount);
    }
}
