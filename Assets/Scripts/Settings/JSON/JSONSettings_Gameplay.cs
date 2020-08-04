using UnityEngine;
using System.Collections;
using System.IO;

public class JSONSettings_Gameplay : JSONSettings {

    // Constants for setting states
    public const int MK54 = 0; // Mouse/Keyboard with Select and SelectAlternate on 5 and 4, etc.
    public const int MK45 = 1;
    public const int MKEQ = 2;
    public const int MKQE = 3;
    public const int Gamepad = 4;

    protected override string DefaultConfigFileName => Path.Combine(Application.streamingAssetsPath, "Data", "Config", "config_Gameplay_default.json");

    public bool UsingGamepad => controlScheme == Gamepad;

    public int controlScheme;
    public int gamepadRumble; // 0 for Disabled, 1 for Enabled
    public float gamepadRumbleIntensity;
    public int cameraFirstPerson; // 0 for third person, 1 for first person
    public float cameraDistance;
    public int cameraClamping; // 0 for Unclamped, 1 for Clamped
    public int cameraInvertX; // 0 for disabled, 1 for inverted
    public int cameraInvertY;
    public float mouseSensitivityX;
    public float mouseSensitivityY;
    public float gamepadSensitivityX;
    public float gamepadSensitivityY;

    protected override void Awake() {
        ConfigFileName = Path.Combine(Application.persistentDataPath, "Data", "Config", "config_Gameplay.json");
        base.Awake();
    }
    /// <summary>
    /// Manually apply certain setting effects when they are changed
    /// </summary>
    public override void SetSettingsWhenChanged() {
        HUD.ControlWheelController.RefreshHotkeys();
        HUD.HelpOverlayController.UpdateText();
    }
}
