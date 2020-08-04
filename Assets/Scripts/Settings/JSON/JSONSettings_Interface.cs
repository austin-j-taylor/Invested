using UnityEngine;
using System.Collections;
using System.IO;

public class JSONSettings_Interface : JSONSettings {

    protected override string ConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Interface.json");
    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Interface_default.json");

    public int forceUnits; // 0 for G's, 1 for Newtons
    public int forceComplexity; // 0 for net only, 1 for full sums
    public int hudEnabled; // 0 for Disabled, 1 for Enabled
    public int hudForces; // 0 for Disabled, 1 for Enabled
    public int hudMasses; // 0 for Disabled, 1 for Enabled
    public int fpsCounter; // 0 for Disabled, 1 for Enabled
    public int helpOverlay; // 0 for Disabled, 1 for Simple, 2 for Verbose

    /// <summary>
    /// Manually apply certain setting effects when they are changed
    /// </summary>
    public override void SetSettingsWhenChanged() {
        HUD.HelpOverlayController.SetState(helpOverlay);
        Debug.Log(Application.persistentDataPath);
    }
}
