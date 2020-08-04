﻿using UnityEngine;
using System.Collections;
using System.IO;

public class JSONSettings_World : JSONSettings {

    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_World_default.json");

    public int playerGravity; // 0 for Disabled, 1 for Enabled, 2 for Inverted
    public int playerAirResistance; // 0 for Disabled, 1 for Enabled
    public float timeScale;

    protected override void Awake() {
        ConfigFileName = Path.Combine(Application.persistentDataPath, "Data", "Config", "config_World.json");
        base.Awake();
    }
    /// <summary>
    /// Manually apply certain setting effects when they are changed
    /// </summary>
    public override void SetSettingsWhenChanged() {
        TimeController.CurrentTimeScale = timeScale;
    }
}
