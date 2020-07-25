﻿using UnityEngine;
using System.Collections;
using System.IO;

public class JSONSettings_Video : JSONSettings {

    protected override string ConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Video.json");
    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Video_default.json");

    public int fullscreen;
    public int resolution;

    /// <summary>
    /// Manually apply certain setting effects when they are changed
    /// </summary>
    public override void SetSettingsWhenChanged() {
        GraphicsController.SetFullscreenResolution(resolution, (FullScreenMode)fullscreen);
    }
}