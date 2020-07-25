﻿using UnityEngine;
using System.Collections;
using System.IO;

public class JSONSettings_Graphics : JSONSettings {

    protected override string ConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Graphics.json");
    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Graphics_default.json");

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
}